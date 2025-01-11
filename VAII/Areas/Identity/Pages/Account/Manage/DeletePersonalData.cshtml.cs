// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VAII.Data;
using VAII.Data.Migrations;
using VAII.Models.Entities;

namespace VAII.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        private readonly IWebHostEnvironment environment;
        private readonly ApplicationDbContext dbContext;

        public DeletePersonalDataModel(
            UserManager<CustomUser> userManager,
            SignInManager<CustomUser> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            dbContext = context;
            environment = env;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }
            var imagePath = Path.Combine(environment.WebRootPath, Path.GetDirectoryName(user.ImagePath.TrimStart('/')));
            if (Directory.Exists(imagePath) && user.ImagePath != "/images/user.png")
            {
                Directory.Delete(imagePath, true);
            }

            var userId = await _userManager.GetUserIdAsync(user);
            await DeleteRelatedData(userId);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }

        private async Task DeleteRelatedData(string userId)
        {
            var games = await dbContext.Games.Where(g => g.UserID == userId).ToListAsync();
            dbContext.Games.RemoveRange(games);

            foreach (Game game in games)
            {
                var gameTags = dbContext.GameTags.Where(gt => gt.GameID == game.GameID);

                if (gameTags == null) return;

                var tagIds = gameTags.Select(gt => gt.TagID).ToList();
                var tags = await dbContext.Tags.Where(t => tagIds.Contains(t.TagID)).ToListAsync();

                foreach (var gameTag in gameTags)
                {
                    Tag tag = tags.FirstOrDefault(t => t.TagID == gameTag.TagID);
                    if (tag != null)
                    {
                        tag.UseCount--;
                        if (tag.UseCount == 0)
                        {
                            dbContext.Tags.Remove(tag);
                        }
                        else
                        {
                            dbContext.Tags.Update(tag);
                        }
                    }
                }
                dbContext.GameTags.RemoveRange(gameTags);

                var imagePath = Path.Combine(environment.WebRootPath, Path.GetDirectoryName(game.ImagePath.TrimStart('/')));
                var filePath = Path.Combine(environment.WebRootPath, Path.GetDirectoryName(game.FilePath.TrimStart('/')));

                // Delete the image file if it exists
                if (Directory.Exists(imagePath) && game.ImagePath != "/images/nothing.png")
                {
                    Directory.Delete(imagePath, true);
                }

                // Delete the game file if it exists
                if (Directory.Exists(filePath))
                {
                    Directory.Delete(filePath, true);
                }

                dbContext.Games.Remove(game);

                List<Review> reviews = dbContext.Reviews
                               .Where(r => r.GameID == game.GameID)
                               .ToList();
                foreach (var review in reviews)
                {
                    dbContext.Reviews.Remove(review);
                }
            }           

            await dbContext.SaveChangesAsync();
        }
    }
    
}
