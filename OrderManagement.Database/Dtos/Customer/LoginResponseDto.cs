namespace OrderManagement.Database.Dtos.Customer;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public CustomerProfileDto Profile { get; set; } = null!;
}
