using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.AccountViewModels;

namespace WaterSupply.Web.Controllers;

public sealed class AccountController(
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext context) : Controller
{
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim();
        var normalizedEmail = email.ToLowerInvariant();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError(nameof(model.Email), "That email is already registered.");
            return View(model);
        }

        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };
        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddErrors(createResult);
            return View(model);
        }

        if (!await roleManager.RoleExistsAsync("Resident"))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole("Resident"));
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                AddErrors(roleResult);
                return View(model);
            }
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, "Resident");
        if (!addRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            AddErrors(addRoleResult);
            return View(model);
        }

        var existingProfile = await context.Residents
            .SingleOrDefaultAsync(resident => resident.Email.ToLower() == normalizedEmail);
        if (existingProfile?.IdentityUserId is not null)
        {
            await userManager.DeleteAsync(user);
            ModelState.AddModelError(nameof(model.Email), "A resident profile with this email is already activated. Try logging in instead.");
            return View(model);
        }

        if (existingProfile is null)
        {
            context.Residents.Add(new Resident(
                0,
                model.FullName,
                email,
                model.Phone,
                model.Address,
                DateOnly.FromDateTime(DateTime.Today),
                identityUserId: user.Id));
        }
        else
        {
            existingProfile.UpdateContact(model.FullName, email, model.Phone, model.Address);
            existingProfile.Activate();
            existingProfile.LinkIdentityUser(user.Id);
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            await userManager.DeleteAsync(user);
            ModelState.AddModelError(string.Empty, "Your account could not be linked to a resident profile. Please try again or contact the administrator.");
            return View(model);
        }
        await signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "ResidentPortal");
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            var destination = model.ReturnUrl is not null && Url.IsLocalUrl(model.ReturnUrl)
                ? model.ReturnUrl
                : "/Dashboard";
            return LocalRedirect(destination);
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [Authorize(Roles = "Resident")]
    public async Task<IActionResult> Profile()
    {
        var resident = await FindCurrentResidentAsync();
        return resident is null
            ? Forbid()
            : View(new ResidentProfileViewModel
            {
                FullName = resident.FullName,
                Email = resident.Email,
                Phone = resident.Phone,
                Address = resident.Address
            });
    }

    [HttpPost, Authorize(Roles = "Resident"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ResidentProfileViewModel model)
    {
        var resident = await FindCurrentResidentAsync();
        if (resident is null) return Forbid();
        if (!ModelState.IsValid) return View(model);

        resident.UpdateContact(model.FullName, resident.Email, model.Phone, model.Address);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Your profile could not be updated. Please try again.");
            return View(model);
        }

        return RedirectToAction("Index", "ResidentPortal");
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded) return RedirectToAction("Index", "Dashboard");
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        return View(model);
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
    }

    private Task<Resident?> FindCurrentResidentAsync()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
        return context.Residents.SingleOrDefaultAsync(resident =>
            (identityUserId != null && resident.IdentityUserId == identityUserId)
            || (email != null && resident.Email == email));
    }
}
