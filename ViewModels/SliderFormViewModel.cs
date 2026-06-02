using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.ViewModels;

public class SliderFormViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn ảnh slider.")]
    [Display(Name = "Ảnh slider")]
    public IFormFile? ImageFile { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải từ 0 trở lên.")]
    [Display(Name = "Thứ tự hiển thị")]
    public int DisplayOrder { get; set; }
}
