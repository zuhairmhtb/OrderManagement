using AutoMapper;
using OrderManagement.Database.Commands;
using OrderManagement.Database.Commands.Customer;
using OrderManagement.Database.Commands.Order;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Dtos;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Database.Dtos.Order;
using OrderManagement.Database.Models;

namespace OrderManagement.Database.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Map AddAddressCommand to Address
        CreateMap<AddAddressCommand, Address>();

        // Map Address to AddressDto
        CreateMap<Address, AddressDto>();

        // Map UpdateAddressCommand to Address (partial update: null source values are skipped)
        CreateMap<UpdateAddressCommand, Address>()
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.AddressId))
            .ForAllMembers(opt => opt.Condition((src, dst, srcMember) => srcMember != null));

        // Map Customer to CustomerProfileDto
        CreateMap<Customer, CustomerProfileDto>();

        // Map UpdateProfileCommand to Customer (partial update: null source values are skipped)
        CreateMap<UpdateProfileCommand, Customer>()
            .ForAllMembers(opt => opt.Condition((src, dst, srcMember) => srcMember != null));

        // Map PurchasedProduct to OrderItemDto
        CreateMap<PurchasedProduct, OrderItemDto>();

        // Map Order to CustomerOrderDto
        // ShippingAddress / BillingAddress are stored as flat columns on Order, so they
        // are projected into AddressDto objects via explicit MapFrom lambdas.
        // Customer is not a navigation property on Order, so it is intentionally
        // ignored here and set manually in the service after the map call.
        CreateMap<Order, CustomerOrderDto>()
            .ForMember(dst => dst.OrderId,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.Total,
                opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dst => dst.OrderStatus,
                opt => opt.MapFrom(src => src.OrderStatus.ToString()))
            .ForMember(dst => dst.Currency,
                opt => opt.MapFrom(src => src.Currency.ToString()))
            .ForMember(dst => dst.ShippingAddress,
                opt => opt.MapFrom(src => new AddressDto
                {
                    Street     = src.ShippingStreet,
                    City       = src.ShippingCity,
                    PostalCode = src.ShippingPostalCode,
                    Country    = src.ShippingCountry,
                    State      = src.ShippingState
                }))
            .ForMember(dst => dst.BillingAddress,
                opt => opt.MapFrom(src => new AddressDto
                {
                    Street     = src.BillingStreet,
                    City       = src.BillingCity,
                    PostalCode = src.BillingPostalCode,
                    Country    = src.BillingCountry,
                    State      = src.BillingState
                }))
            .ForMember(dst => dst.Items,
                opt => opt.MapFrom(src => src.Products))
            .ForMember(dst => dst.Customer,
                opt => opt.Ignore());

        // Map PlaceOrderCommand to Order.
        // Shipping / Billing addresses are stored as flat columns, so each field is
        // projected explicitly from the nested AddressDto on the command.
        // OrderStatus is hardcoded to Pending — a new order is always in that state.
        // Fields that depend on the loaded Customer entity, computed totals, or the
        // built PurchasedProduct list are Ignored here and set in the service.
        CreateMap<PlaceOrderCommand, Order>()
            .ForMember(dst => dst.OrderStatus,
                opt => opt.MapFrom(_ => OrderStatus.Pending))
            .ForMember(dst => dst.ShippingStreet,
                opt => opt.MapFrom(src => src.ShippingAddress.Street))
            .ForMember(dst => dst.ShippingCity,
                opt => opt.MapFrom(src => src.ShippingAddress.City))
            .ForMember(dst => dst.ShippingPostalCode,
                opt => opt.MapFrom(src => src.ShippingAddress.PostalCode))
            .ForMember(dst => dst.ShippingCountry,
                opt => opt.MapFrom(src => src.ShippingAddress.Country))
            .ForMember(dst => dst.ShippingState,
                opt => opt.MapFrom(src => src.ShippingAddress.State))
            .ForMember(dst => dst.BillingStreet,
                opt => opt.MapFrom(src => src.BillingAddress.Street))
            .ForMember(dst => dst.BillingCity,
                opt => opt.MapFrom(src => src.BillingAddress.City))
            .ForMember(dst => dst.BillingPostalCode,
                opt => opt.MapFrom(src => src.BillingAddress.PostalCode))
            .ForMember(dst => dst.BillingCountry,
                opt => opt.MapFrom(src => src.BillingAddress.Country))
            .ForMember(dst => dst.BillingState,
                opt => opt.MapFrom(src => src.BillingAddress.State))
            // Resolved from the loaded Customer entity — set after the map call
            .ForMember(dst => dst.CustomerEmail,         opt => opt.Ignore())
            .ForMember(dst => dst.CustomerContactNumber, opt => opt.Ignore())
            // Already validated and parsed in the service — set after the map call
            .ForMember(dst => dst.Currency,              opt => opt.Ignore())
            // Computed from product prices — set after the map call
            .ForMember(dst => dst.Subtotal,              opt => opt.Ignore())
            .ForMember(dst => dst.TotalAmount,           opt => opt.Ignore())
            // Built from loaded Product entities — set after the map call
            .ForMember(dst => dst.Products,              opt => opt.Ignore())
            // Auto-initialised by the model; left at their default values
            .ForMember(dst => dst.Id,                    opt => opt.Ignore())
            .ForMember(dst => dst.OrderDate,             opt => opt.Ignore())
            .ForMember(dst => dst.DeliveryDate,          opt => opt.Ignore())
            .ForMember(dst => dst.Vat,                   opt => opt.Ignore())
            .ForMember(dst => dst.ShippingCost,          opt => opt.Ignore())
            .ForMember(dst => dst.AdditionalCharges,     opt => opt.Ignore());
    }
}
