namespace Ecommerce.Dto.AddressDto;

public class AddressResponseDto
{
    public int AddressId { get; set; }

    public int CustomerId { get; set; }

    public string AddressLine { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public DateTime CreatedDate { get; set; }
}