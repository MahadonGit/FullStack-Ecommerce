using AutoMapper;
using Ecommerce.Dto.CartDto;
using Ecommerce.Dto.CustomerDto;
using Ecommerce.Dto.OrderDto;
using Ecommerce.Dto.ProductDto;
using Ecommerce.Dto.AddressDto;
using Ecommerce.Dto.CategoryDto;
using Ecommerce.Dto.FeedbackDto;
using Ecommerce.Models;

namespace Ecommerce.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Customer mappings
            CreateMap<CustomerCreateDto, Customer>();

            CreateMap<CustomerUpdateDto, Customer>();

            CreateMap<Customer, CustomerResponseDto>();

            // Address mappings
            CreateMap<AddressCreateDto, Address>();

            CreateMap<AddressUpdateDto, Address>();

            CreateMap<Address, AddressResponseDto>();

            // Category mappings
            CreateMap<CategoryCreateDto, Category>();

            CreateMap<CategoryUpdateDto, Category>();

            CreateMap<Category, CategoryResponseDto>();

            // Product mappings
            CreateMap<ProductCreateDto, Product>();

            CreateMap<ProductUpdateDto, Product>();

            CreateMap<Product, ProductResponseDto>()
                .ForMember(
                    dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category.Name)
                );


            // Cart mappings
            CreateMap<CartItem, CartItemResponseDto>()
                .ForMember(
                    dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name)
                )
                .ForMember(
                    dest => dest.Price,
                    opt => opt.MapFrom(src => src.Product.Price)
                )
                .ForMember(
                    dest => dest.TotalPrice,
                    opt => opt.MapFrom(src => src.Product.Price * src.Quantity)
                );

            CreateMap<Cart, CartResponseDto>();


            // Order mappings
            CreateMap<Order, OrderResponseDto>();

            CreateMap<OrderDetail, OrderDetailResponseDto>()
                .ForMember(
                    dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name)
                );


            // Feedback
            CreateMap<CreateFeedbackDto, Feedback>();

            CreateMap<UpdateFeedbackDto, Feedback>();

            CreateMap<Feedback, FeedbackResponseDto>()
                .ForMember(
                    dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer.Name)
                )
                .ForMember(
                    dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name)
                );
        }


    }
}