// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VAII.Models.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VAII.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;

        public IndexModel(
            UserManager<CustomUser> userManager,
            SignInManager<CustomUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [BindProperty]
        [Display(Name = "Profile Picture")]
        public string ProfilePicture { get; set; }
        
        [BindProperty]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [BindProperty]
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, ErrorMessage = "The username must be between {2} and {1} characters.", MinimumLength = 3)]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public IFormFile NewProfilePicture { get; set; }

        private async Task LoadAsync(CustomUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var profilePicturePath = user.ImagePath;

            Username = userName;
            Email = email;
            ProfilePicture = string.IsNullOrEmpty(profilePicturePath)
                ? "/image/user.png" // Default profile picture
                : profilePicturePath;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (!string.Equals(user.UserName, Username, StringComparison.OrdinalIgnoreCase))
            {
                var setUsernameResult = await _userManager.SetUserNameAsync(user, Username);
                if (!setUsernameResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, $"Failed to update username.");
                    foreach (var error in setUsernameResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await LoadAsync(user);
                    return Page();
                }
            }

            if (!string.Equals(user.Email, Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Email);
                if (!setEmailResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Failed to update email.");
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await LoadAsync(user);
                    return Page();
                }
            }

            if (NewProfilePicture != null && NewProfilePicture.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".svg", ".ico" };
                var extension = Path.GetExtension(NewProfilePicture.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("NewProfilePicture", "Only image files are allowed (JPG, JPEG, PNG, GIF, BMP, TIFF, WEBP, SVG, ICO).");
                    return Page();
                }
                if (NewProfilePicture.Length > 12 * 1024 * 1024) // 15 MB limit
                {
                    ModelState.AddModelError("NewProfilePicture", "File size must not exceed 15 MB.");
                    return Page();
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", user.Id);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Path.GetFileName(NewProfilePicture.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                if (!ProfilePicture.Equals($"/images/{user.Id}/{fileName}"))
                {
                    if (Directory.Exists(ProfilePicture) && user.ImagePath != "/images/user.png")
                    {
                        Directory.Delete(ProfilePicture, true);
                    }

                    // Save the new profile picture
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await NewProfilePicture.CopyToAsync(stream);
                    }

                    // Update the user's profile picture path
                    user.ImagePath = $"/images/{user.Id}/{fileName}";
                    var updateResult = await _userManager.UpdateAsync(user);

                    if (!updateResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Failed to update profile picture.");
                        await LoadAsync(user);
                        return Page();
                    }
                }                
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }    
    }
}
