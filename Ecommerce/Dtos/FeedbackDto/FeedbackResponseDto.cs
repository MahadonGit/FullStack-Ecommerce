namespace Ecommerce.Dto.FeedbackDto;

public class FeedbackResponseDto
{
    public int FeedbackId { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime FeedbackDate { get; set; }
}