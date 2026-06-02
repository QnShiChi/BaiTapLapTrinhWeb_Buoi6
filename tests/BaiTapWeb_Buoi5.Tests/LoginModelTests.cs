using BaiTapWeb_Buoi5.Areas.Identity.Pages.Account;
using BaiTapWeb_Buoi5.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace BaiTapWeb_Buoi5.Tests;

public class LoginModelTests
{
    [Fact]
    public async Task OnPostAsync_UsesEmailToResolveUserBeforeSigningIn()
    {
        var userManager = BuildUserManager();
        userManager.Setup(manager => manager.FindByEmailAsync("admin@qlbanhang.local"))
            .ReturnsAsync(new ApplicationUser { UserName = "admin", Email = "admin@qlbanhang.local" });

        var signInManager = BuildSignInManager(userManager.Object);
        signInManager.Setup(manager => manager.PasswordSignInAsync(
                "admin",
                "Admin@123",
                false,
                false))
            .ReturnsAsync(IdentitySignInResult.Success);

        var model = CreateModel(userManager.Object, signInManager.Object);
        model.Input = new LoginModel.InputModel
        {
            Email = "admin@qlbanhang.local",
            Password = "Admin@123",
            RememberMe = false
        };

        var result = await model.OnPostAsync();

        Assert.IsType<LocalRedirectResult>(result);
        userManager.Verify(manager => manager.FindByEmailAsync("admin@qlbanhang.local"), Times.Once);
        signInManager.Verify(manager => manager.PasswordSignInAsync("admin", "Admin@123", false, false), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_ReturnsPageWhenEmailIsInvalid()
    {
        var userManager = BuildUserManager();
        var signInManager = BuildSignInManager(userManager.Object);

        var model = CreateModel(userManager.Object, signInManager.Object);
        model.Input = new LoginModel.InputModel
        {
            Email = "admin",
            Password = "Admin@123",
            RememberMe = true
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.True(model.ModelState.ContainsKey("Input.Email"));
        userManager.Verify(manager => manager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        signInManager.Verify(manager => manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
    }

    private static LoginModel CreateModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var httpContext = new DefaultHttpContext();
        var model = new LoginModel(signInManager, userManager, Mock.Of<ILogger<LoginModel>>())
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>())
        };

        return model;
    }

    private static Mock<UserManager<ApplicationUser>> BuildUserManager()
    {
        return new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());
    }

    private static Mock<SignInManager<ApplicationUser>> BuildSignInManager(UserManager<ApplicationUser> userManager)
    {
        return new Mock<SignInManager<ApplicationUser>>(
            userManager,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());
    }
}
