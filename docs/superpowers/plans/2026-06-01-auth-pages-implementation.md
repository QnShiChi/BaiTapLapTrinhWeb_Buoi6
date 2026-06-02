# Auth Pages Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Đồng bộ giao diện `Login/Register` với website và cho phép đăng nhập bằng cả email hoặc username.

**Architecture:** Tạo Razor Page `Login` custom trong `Areas/Identity` để override giao diện mặc định của Identity, đồng thời đổi model đăng nhập sang một trường `Login` duy nhất và resolve ra `UserName` trước khi gọi `PasswordSignInAsync`. Giữ nguyên `RegisterModel`, chỉ chỉnh markup, text hiển thị và CSS auth dùng chung để đồng bộ với layout hiện tại.

**Tech Stack:** ASP.NET Core MVC/Razor Pages, ASP.NET Core Identity, xUnit, Moq

---

## File Structure

- Create: `Areas/Identity/Pages/Account/Login.cshtml`
- Create: `Areas/Identity/Pages/Account/Login.cshtml.cs`
- Modify: `Areas/Identity/Pages/Account/Register.cshtml`
- Modify: `Areas/Identity/Pages/Account/Register.cshtml.cs`
- Modify: `wwwroot/css/site.css`
- Create: `tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj`
- Create: `tests/BaiTapWeb_Buoi5.Tests/LoginModelTests.cs`

### Task 1: Add focused tests for login identifier resolution

**Files:**
- Create: `tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj`
- Create: `tests/BaiTapWeb_Buoi5.Tests/LoginModelTests.cs`
- Test: `tests/BaiTapWeb_Buoi5.Tests/LoginModelTests.cs`

- [ ] **Step 1: Write the failing test project**

Create `tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../BaiTapWeb_Buoi5.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Write the failing tests**

Create `tests/BaiTapWeb_Buoi5.Tests/LoginModelTests.cs`:

```csharp
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
using System.Text.Encodings.Web;
using Xunit;

namespace BaiTapWeb_Buoi5.Tests;

public class LoginModelTests
{
    [Fact]
    public async Task OnPostAsync_UsesUserNameWhenLoginInputIsPlainUserName()
    {
        var userManager = BuildUserManager();
        userManager.Setup(manager => manager.FindByNameAsync("admin"))
            .ReturnsAsync(new ApplicationUser { UserName = "admin", Email = "admin@qlbanhang.local" });

        var signInManager = BuildSignInManager(userManager.Object);
        signInManager.Setup(manager => manager.PasswordSignInAsync(
                "admin",
                "Admin@123",
                false,
                false))
            .ReturnsAsync(SignInResult.Success);

        var model = CreateModel(userManager.Object, signInManager.Object);
        model.Input = new LoginModel.InputModel
        {
            Login = "admin",
            Password = "Admin@123",
            RememberMe = false
        };

        var result = await model.OnPostAsync();

        Assert.IsType<LocalRedirectResult>(result);
        userManager.Verify(manager => manager.FindByNameAsync("admin"), Times.Once);
        userManager.Verify(manager => manager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        signInManager.Verify(manager => manager.PasswordSignInAsync("admin", "Admin@123", false, false), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_ResolvesEmailToUserNameBeforeSigningIn()
    {
        var userManager = BuildUserManager();
        userManager.Setup(manager => manager.FindByEmailAsync("admin@qlbanhang.local"))
            .ReturnsAsync(new ApplicationUser { UserName = "admin", Email = "admin@qlbanhang.local" });

        var signInManager = BuildSignInManager(userManager.Object);
        signInManager.Setup(manager => manager.PasswordSignInAsync(
                "admin",
                "Admin@123",
                true,
                false))
            .ReturnsAsync(SignInResult.Success);

        var model = CreateModel(userManager.Object, signInManager.Object);
        model.Input = new LoginModel.InputModel
        {
            Login = "admin@qlbanhang.local",
            Password = "Admin@123",
            RememberMe = true
        };

        var result = await model.OnPostAsync();

        Assert.IsType<LocalRedirectResult>(result);
        userManager.Verify(manager => manager.FindByEmailAsync("admin@qlbanhang.local"), Times.Once);
        userManager.Verify(manager => manager.FindByNameAsync(It.IsAny<string>()), Times.Never);
        signInManager.Verify(manager => manager.PasswordSignInAsync("admin", "Admin@123", true, false), Times.Once);
    }

    private static LoginModel CreateModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var model = new LoginModel(signInManager, userManager, Mock.Of<ILogger<LoginModel>>())
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext()
            },
            TempData = new TempDataDictionary(
                new DefaultHttpContext(),
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
```

- [ ] **Step 3: Run test to verify it fails**

Run:

```bash
dotnet test tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj --filter LoginModelTests -v minimal
```

Expected:

```text
FAIL ... The type or namespace name 'LoginModel' could not be found
```

- [ ] **Step 4: Commit the red test**

```bash
git add tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj tests/BaiTapWeb_Buoi5.Tests/LoginModelTests.cs
git commit -m "test: cover login with username or email"
```

### Task 2: Implement custom Login page with email-or-username sign-in

**Files:**
- Create: `Areas/Identity/Pages/Account/Login.cshtml`
- Create: `Areas/Identity/Pages/Account/Login.cshtml.cs`
- Test: `tests/BaiTapWeb_Buoi5.Tests/LoginModelTests.cs`

- [ ] **Step 1: Write minimal `LoginModel` to satisfy the tests**

Create `Areas/Identity/Pages/Account/Login.cshtml.cs`:

```csharp
using BaiTapWeb_Buoi5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BaiTapWeb_Buoi5.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email hoặc tên đăng nhập.")]
        [Display(Name = "Email hoặc tên đăng nhập")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await ResolveUserAsync(Input.Login);
        if (user is null || string.IsNullOrWhiteSpace(user.UserName))
        {
            ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không hợp lệ.");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in.");
            return LocalRedirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không hợp lệ.");
        return Page();
    }

    private Task<ApplicationUser?> ResolveUserAsync(string login)
    {
        return new EmailAddressAttribute().IsValid(login)
            ? _userManager.FindByEmailAsync(login)
            : _userManager.FindByNameAsync(login);
    }
}
```

- [ ] **Step 2: Add the custom login markup**

Create `Areas/Identity/Pages/Account/Login.cshtml`:

```cshtml
@page
@model BaiTapWeb_Buoi5.Areas.Identity.Pages.Account.LoginModel
@{
    ViewData["Title"] = "Đăng nhập";
}

<section class="auth-shell">
    <div class="auth-card auth-card-wide">
        <p class="eyebrow">Tai khoan</p>
        <h1>Đăng nhập</h1>
        <p class="auth-intro">Đăng nhập để tiếp tục mua sắm hoặc truy cập khu vực quản trị nếu tài khoản của bạn có quyền.</p>

        <form method="post" class="auth-form">
            <div asp-validation-summary="ModelOnly" class="validation-summary-errors"></div>

            <div class="auth-field">
                <label asp-for="Input.Login"></label>
                <input asp-for="Input.Login" class="form-control auth-input" placeholder="admin hoặc admin@qlbanhang.local" />
                <span asp-validation-for="Input.Login" class="text-danger"></span>
            </div>

            <div class="auth-field">
                <label asp-for="Input.Password"></label>
                <input asp-for="Input.Password" class="form-control auth-input" />
                <span asp-validation-for="Input.Password" class="text-danger"></span>
            </div>

            <label asp-for="Input.RememberMe" class="auth-checkbox">
                <input asp-for="Input.RememberMe" />
                <span>@Html.DisplayNameFor(model => model.Input.RememberMe)</span>
            </label>

            <div class="auth-actions">
                <button type="submit" class="button-primary">Đăng nhập</button>
                <a asp-page="./Register" asp-route-returnUrl="@Model.ReturnUrl" class="button-secondary">Tạo tài khoản</a>
            </div>
        </form>
    </div>
</section>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

- [ ] **Step 3: Run tests to verify green**

Run:

```bash
dotnet test tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj --filter LoginModelTests -v minimal
```

Expected:

```text
Passed! 2 tests passed.
```

- [ ] **Step 4: Commit the login behavior**

```bash
git add Areas/Identity/Pages/Account/Login.cshtml Areas/Identity/Pages/Account/Login.cshtml.cs tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj tests/BaiTapWeb_Buoi5.Tests/LoginModelTests.cs
git commit -m "feat: allow login by email or username"
```

### Task 3: Redesign Register page to match site styling

**Files:**
- Modify: `Areas/Identity/Pages/Account/Register.cshtml`
- Modify: `Areas/Identity/Pages/Account/Register.cshtml.cs`

- [ ] **Step 1: Update register labels and validation text**

Modify `Areas/Identity/Pages/Account/Register.cshtml.cs` `InputModel` attributes:

```csharp
public class InputModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [Display(Name = "Tên đăng nhập")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
    [StringLength(250)]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "{0} phải có ít nhất {2} ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Update register markup to shared auth layout**

Replace `Areas/Identity/Pages/Account/Register.cshtml` with:

```cshtml
@page
@model BaiTapWeb_Buoi5.Areas.Identity.Pages.Account.RegisterModel
@{
    ViewData["Title"] = "Đăng ký";
}

<section class="auth-shell">
    <div class="auth-card auth-card-wide">
        <p class="eyebrow">Thanh vien moi</p>
        <h1>Tạo tài khoản</h1>
        <p class="auth-intro">Tạo tài khoản để lưu thông tin cá nhân, theo dõi sản phẩm và sử dụng các chức năng dành cho thành viên.</p>

        <form method="post" class="auth-form">
            <div asp-validation-summary="ModelOnly" class="validation-summary-errors"></div>

            <div class="auth-grid">
                <div class="auth-field">
                    <label asp-for="Input.UserName"></label>
                    <input asp-for="Input.UserName" class="form-control auth-input" />
                    <span asp-validation-for="Input.UserName" class="text-danger"></span>
                </div>

                <div class="auth-field">
                    <label asp-for="Input.Email"></label>
                    <input asp-for="Input.Email" class="form-control auth-input" />
                    <span asp-validation-for="Input.Email" class="text-danger"></span>
                </div>

                <div class="auth-field">
                    <label asp-for="Input.FullName"></label>
                    <input asp-for="Input.FullName" class="form-control auth-input" />
                    <span asp-validation-for="Input.FullName" class="text-danger"></span>
                </div>

                <div class="auth-field">
                    <label asp-for="Input.Address"></label>
                    <input asp-for="Input.Address" class="form-control auth-input" />
                    <span asp-validation-for="Input.Address" class="text-danger"></span>
                </div>

                <div class="auth-field">
                    <label asp-for="Input.Password"></label>
                    <input asp-for="Input.Password" class="form-control auth-input" />
                    <span asp-validation-for="Input.Password" class="text-danger"></span>
                </div>

                <div class="auth-field">
                    <label asp-for="Input.ConfirmPassword"></label>
                    <input asp-for="Input.ConfirmPassword" class="form-control auth-input" />
                    <span asp-validation-for="Input.ConfirmPassword" class="text-danger"></span>
                </div>
            </div>

            <div class="auth-actions">
                <button type="submit" class="button-primary">Đăng ký</button>
                <a asp-page="./Login" asp-route-returnUrl="@Model.ReturnUrl" class="button-secondary">Đã có tài khoản? Đăng nhập</a>
            </div>
        </form>
    </div>
</section>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

- [ ] **Step 3: Build to verify register page compiles**

Run:

```bash
dotnet build
```

Expected:

```text
Build succeeded.
```

- [ ] **Step 4: Commit the register redesign**

```bash
git add Areas/Identity/Pages/Account/Register.cshtml Areas/Identity/Pages/Account/Register.cshtml.cs
git commit -m "feat: redesign register page"
```

### Task 4: Add shared auth styling and verify the full flow

**Files:**
- Modify: `wwwroot/css/site.css`
- Test: `tests/BaiTapWeb_Buoi5.Tests/LoginModelTests.cs`

- [ ] **Step 1: Add auth-specific CSS without disturbing other pages**

Append to `wwwroot/css/site.css`:

```css
.auth-shell {
  display: grid;
  place-items: center;
  min-height: calc(100vh - 220px);
  padding: 24px 0 12px;
}

.auth-card {
  width: min(100%, 560px);
  padding: 32px;
}

.auth-card-wide {
  width: min(100%, 760px);
}

.auth-intro {
  margin: 12px 0 0;
  color: var(--color-body);
  line-height: 1.7;
}

.auth-form {
  display: grid;
  gap: 18px;
  margin-top: 28px;
}

.auth-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 18px;
}

.auth-field {
  display: grid;
  gap: 8px;
}

.auth-field label {
  font-weight: 600;
  color: var(--color-ink);
}

.auth-input {
  min-height: 52px;
  border: 1px solid var(--color-hairline);
  border-radius: var(--radius-md);
  padding: 0 16px;
  background: #fff;
}

.auth-input:focus {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 4px rgba(255, 56, 92, 0.12);
}

.auth-checkbox {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  color: var(--color-body);
}

.auth-actions {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.button-primary,
.button-secondary {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 48px;
  padding: 0 20px;
  border-radius: var(--radius-full);
  font-weight: 600;
}

.button-primary {
  border: 0;
  color: #fff;
  background: linear-gradient(135deg, var(--color-primary), var(--color-primary-active));
}

.button-secondary {
  border: 1px solid var(--color-hairline);
  background: #fff;
  color: var(--color-ink);
}

.validation-summary-errors {
  padding: 14px 16px;
  border-radius: var(--radius-md);
  background: rgba(193, 53, 21, 0.08);
  color: var(--color-danger);
}

.validation-summary-errors ul {
  margin: 0;
  padding-left: 18px;
}

@media (max-width: 768px) {
  .auth-card,
  .auth-card-wide {
    width: 100%;
    padding: 24px;
  }

  .auth-grid {
    grid-template-columns: 1fr;
  }

  .auth-actions {
    flex-direction: column;
    align-items: stretch;
  }
}
```

- [ ] **Step 2: Run focused tests and application build**

Run:

```bash
dotnet test tests/BaiTapWeb_Buoi5.Tests/BaiTapWeb_Buoi5.Tests.csproj --filter LoginModelTests -v minimal
dotnet build
```

Expected:

```text
Passed! 2 tests passed.
Build succeeded.
```

- [ ] **Step 3: Manual verification**

Run:

```bash
dotnet run
```

Verify in browser:

```text
1. Open /Identity/Account/Login and confirm the page uses the new card layout.
2. Log in with username: admin / Admin@123.
3. Sign out, then log in with email: admin@qlbanhang.local / Admin@123.
4. Open /Identity/Account/Register and confirm spacing, labels, and CTA match the site.
5. After admin login, open /Admin/Product and /Admin/Category.
```

- [ ] **Step 4: Commit the auth styling**

```bash
git add wwwroot/css/site.css
git commit -m "style: align identity pages with site theme"
```

## Self-Review

- Spec coverage:
  - Login/Register UI redesign is covered by Task 2, Task 3, Task 4.
  - Email-or-username login behavior is covered by Task 1 and Task 2.
  - Responsive styling and visual consistency are covered by Task 4.
  - Admin access after login is covered by Task 4 manual verification.
- Placeholder scan:
  - No `TODO`, `TBD`, or "similar to Task N" shortcuts remain.
- Type consistency:
  - `LoginModel.InputModel.Login` is used consistently by tests, page model, and markup.
  - `ReturnUrl` stays on both login and register pages.
