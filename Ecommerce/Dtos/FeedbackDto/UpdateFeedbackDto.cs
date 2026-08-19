using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Dto.FeedbackDto;

public class UpdateFeedbackDto
{
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 2,
        ErrorMessage = "Comment must be between 2 and 500 characters.")]
    public string Comment { get; set; } = string.Empty;
}