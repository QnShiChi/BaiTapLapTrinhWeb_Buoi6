using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.Models;

public class SliderImage
{
    public int Id { get; set; }

    [Required]
    [StringLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
