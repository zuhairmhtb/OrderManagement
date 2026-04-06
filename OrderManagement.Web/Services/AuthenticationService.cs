using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Database.Commands.Authentication;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Configuration;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Database.Models;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IMapper mapper,
        ApplicationDbContext context,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _mapper = mapper;
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginCommand command)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", command.Email);

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == command.Email.ToLowerInvariant());

            if (customer == null)
            {
                _logger.LogWarning("Login failed - email not found: {Email}", command.Email);
                return null;
            }

            var passwordValid = BCrypt.Net.BCrypt.Verify(command.Password, customer.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Login failed - invalid password for email: {Email}", command.Email);
                return null;
            }

            var jwtConfig = _configuration.GetSection("Jwt").Get<JwtConfig>()
                ?? throw new InvalidOperationException("JWT configuration is missing.");

            var token = GenerateJwtToken(customer, jwtConfig);
            var expiresAt = DateTime.UtcNow.AddMinutes(jwtConfig.ExpirationMinutes);

            _logger.LogInformation("Login successful for customer ID: {CustomerId}", customer.Id);

            return new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                Profile = _mapper.Map<CustomerProfileDto>(customer)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", command.Email);
            throw;
        }
    }

    private static string GenerateJwtToken(Customer customer, JwtConfig jwtConfig)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, customer.Email),
            new Claim(ClaimTypes.Role, customer.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtConfig.Issuer,
            audience: jwtConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtConfig.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<CustomerProfileDto> RegisterAsync(SignupCommand command, UserRole role)
    {
        try
        {
            _logger.LogInformation("Attempting to register new customer with email: {Email}", command.Email);

            // Validate that passwords match
            if (command.Password != command.ConfirmPassword)
            {
                _logger.LogWarning("Password confirmation mismatch for email: {Email}", command.Email);
                throw new ArgumentException("Password and confirm password do not match.");
            }

            // Check if customer already exists
            var existingCustomer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == command.Email);

            if (existingCustomer != null)
            {
                _logger.LogWarning("Registration attempt for existing email: {Email}", command.Email);
                throw new InvalidOperationException($"Customer with email {command.Email} already exists.");
            }

            // Hash the password using BCrypt
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(command.Password);

            // Create new customer entity
            var customer = new Customer
            {
                Email = command.Email.ToLowerInvariant(), // Normalize email
                Password = hashedPassword,
                Role = role,
                FirstName = command.FirstName?.Trim(),
                LastName = command.LastName?.Trim(),
                PhoneNumber = command.PhoneNumber?.Trim()
            };

            // Add customer to database
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully registered new customer with ID: {CustomerId}, Email: {Email}", 
                customer.Id, customer.Email);

            // Map to DTO and return (excluding password)
            var customerProfileDto = _mapper.Map<CustomerProfileDto>(customer);
            return customerProfileDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during customer registration for email: {Email}", command.Email);
            throw;
        }
    }
    public async Task<bool> UpdatePasswordAsync(UpdatePasswordCommand command)
    {
        try
        {
            _logger.LogInformation("Attempting to update password for user ID: {UserId}", command.UserId);

            // Validate that passwords match
            if (command.Password != command.ConfirmPassword)
            {
                _logger.LogWarning("Password confirmation mismatch for user ID: {UserId}", command.UserId);
                throw new ArgumentException("Password and confirm password do not match.");
            }

            // Find the customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == command.UserId);

            if (customer == null)
            {
                _logger.LogWarning("Customer not found for password update, user ID: {UserId}", command.UserId);
                throw new ArgumentException($"Customer with ID {command.UserId} not found.");
            }

            // Hash the new password using BCrypt
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(command.Password);

            // Update the password
            customer.Password = hashedPassword;

            // Save changes to database
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated password for user ID: {UserId}, Email: {Email}", 
                customer.Id, customer.Email);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating password for user ID: {UserId}", command.UserId);
            throw;
        }
    }
}