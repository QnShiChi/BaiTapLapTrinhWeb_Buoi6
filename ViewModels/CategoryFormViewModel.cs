using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.ViewModels;

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}
