using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Address { get; set; } = string.Empty;
}
