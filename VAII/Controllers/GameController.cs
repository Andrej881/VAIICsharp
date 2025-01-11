using Microsoft.AspNetCore.Mvc;
using VAII.Models.DTO;
using VAII.Data;
using VAII.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Azure;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace VAII.Controllers
{
    public class GameController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IWebHostEnvironment environment;
        private readonly UserManager<CustomUser> userManager;

        public GameController(ApplicationDbContext dbContext, IWebHostEnvironment environment, UserManager<CustomUser> userManager)
        {
            this.dbContext = dbContext;
            this.environment = environment;
            this.userManager = userManager;
        }        

        [HttpGet]
        public IActionResult UploadGame()
        {
            if (userManager.GetUserId(User) is null)
            {
                var returnUrl = Request.Path.ToString();
                return Redirect($"/Identity/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}"); 
            }
            var tags = dbContext.Tags.ToList();
            var viewModel = new EditGameViewModel
            {
                AvailableTags = tags.IsNullOrEmpty() ? tags : new List<Tag>(),
                SelectedTags = new List<string>(),
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> UploadGame(EditGameViewModel model)
        {
            if(model == null)
            {
                TempData["ErrorMessage"] = "Error in recieving model data : model is null.(probably sending more than 1GB of data)";
                return RedirectToAction("Error", "Home");
            }
            model.SelectedTags = model.SelectedTags is null ? new () : model.SelectedTags;
            if (model.AvailableTags is null)
            {
                var tags = dbContext.Tags.ToList();
                model.AvailableTags = tags is null ? new() : tags;
            }

            bool imagePathTest = model.ImagePath != null && model.ImagePath.Length > 0;
            bool fileTest = model.FilePath != null && model.FilePath.Length > 0;
            var userId = userManager.GetUserId(User);

            if (model.Title == String.Empty)
            {
                
                TempData["ErrorMessage"] = "You need to write Title";
                return View(model);
            }
            if (!fileTest)
            {
                TempData["ErrorMessage"] = "You need to upload file";
                return View(model);
            }
            if (model.FilePath.Length > 300 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "File sze is more than 300 MB";
                return View(model);
            }
            if (imagePathTest)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".svg", ".ico" };
                var extension = Path.GetExtension(model.ImagePath.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "Only image files are allowed (JPG, JPEG, PNG, GIF, BMP, TIFF, WEBP, SVG, ICO).";
                    return View(model);
                }
            }
            var game = new Game
            {
                UserID = userId,
                Title = model.Title,
                Description = model.Description is null ? "" : model.Description,
                ImagePath = "",
                FilePath = $"/games/{model.Title}/{model.FilePath.FileName}",
                UploadDate = DateTime.Now
            };


            await dbContext.Games.AddAsync(game);
            await dbContext.SaveChangesAsync();

            game.ImagePath = imagePathTest ? $"/images/{game.GameID}/{model.ImagePath.FileName}" : "/images/nothing.png";

            string uploads, imagePath;
            if (imagePathTest)
            {               

                uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", $"{game.GameID}");
                imagePath = Path.Combine(uploads, model.ImagePath.FileName);
            }
            else
            {
                uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                imagePath = Path.Combine(uploads, "nothing.png");
            }

            var uploadsFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "games", $"{game.GameID}");
            var filePath = Path.Combine(uploadsFile, model.FilePath.FileName);

            // Ensure the directory exists
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }
            if (!Directory.Exists(uploadsFile))
            {
                Directory.CreateDirectory(uploadsFile);
            }

            // Save the uploaded file to the server
            if (imagePathTest)
            {
                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    await model.ImagePath.CopyToAsync(fileStream);
                }
            }
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.FilePath.CopyToAsync(fileStream);
            }

            dbContext.Games.Update(game);
            await dbContext.SaveChangesAsync();

            if (model.SelectedTags is not null)
            {
                foreach (var tagName in model.SelectedTags)
                {
                    var tag = await dbContext.Tags
                        .Where(t => t.TagName.ToLower() == tagName.ToLower())
                        .FirstOrDefaultAsync();

                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            TagName = tagName,
                            UseCount = 1
                        };
                        dbContext.Tags.Add(tag);
                    }
                    else
                    {
                        tag.UseCount += 1;

                        dbContext.Tags.Update(tag);
                    }
                    await dbContext.SaveChangesAsync();
                    var gameTag = new GameTag
                    {
                        GameID = game.GameID,
                        TagID = tag.TagID
                    };

                    dbContext.GameTags.Add(gameTag);
                    await dbContext.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index","Home");
        }

        [HttpGet]
        public IActionResult GameDescription(int id)
        {
            var game = dbContext.Games
                                .Include(g => g.GameTags)
                                .ThenInclude(gt => gt.Tag)
                                .FirstOrDefault(g => g.GameID == id);

            if (game == null)
            {
                return NotFound();
            }

            List<Review> reviews = dbContext.Reviews
                           .Include(r => r.User)
                           .Where(r => r.GameID == id)
                           .ToList();

            GameDescriptionViewModel gameView = new()
            {
                Id = id,
                CurrentUser = userManager.GetUserId(User),
                Title = game.Title,
                Description = game.Description,
                ExistingImagePath = game.ImagePath,
                ExistingFilePath = game.FilePath,
                SelectedTags = game.GameTags.Select(gt => gt.Tag.TagName).ToList() is null ? new List<string>() : game.GameTags.Select(gt => gt.Tag.TagName).ToList(),
                Reviews = reviews is not null ? reviews : new List<Review>()
            };

            return View(gameView);
        }

        public async Task<IActionResult> DownloadGame(int id)
        {
            
            var game = await dbContext.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(environment.WebRootPath, game.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            return PhysicalFile(filePath, "application/octet-stream", Path.GetFileName(filePath));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteGame(int id)
        {
            Game game = await dbContext.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            if (userManager.GetUserId(User) != game.UserID)
            {
                return Forbid();
            }

            var gameTags = dbContext.GameTags.Where(gt => gt.GameID == id);            

            await RemoveGameTags(gameTags.ToList());

            var imagePath = Path.Combine(environment.WebRootPath, Path.GetDirectoryName(game.ImagePath.TrimStart('/')));
            var filePath = Path.Combine(environment.WebRootPath, Path.GetDirectoryName(game.FilePath.TrimStart('/')));

            // Delete the image file if it exists
            if (Directory.Exists(imagePath) && game.ImagePath != "/images/nothing.png")
            {
                Directory.Delete(imagePath,true);
            }

            // Delete the game file if it exists
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath,true);
            }

            dbContext.Games.Remove(game);

            List<Review> reviews = dbContext.Reviews
                           .Where(r => r.GameID == id)
                           .ToList();
            foreach (var review in reviews)
            { 
                dbContext.Reviews.Remove(review);
            }
            await dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult EditGame(int id)
        {
            if (userManager.GetUserId(User) is null)
            {
                var returnUrl = Request.Path.ToString();
                return Redirect($"/Identity/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }
            var game = dbContext.Games
                                .Include(g => g.GameTags)
                                .ThenInclude(gt => gt.Tag)
                                .FirstOrDefault(g => g.GameID == id);

            if (game == null)
            {
                return NotFound();
            }

            if (userManager.GetUserId(User) != game.UserID)
            {
                TempData["ErrorMessage"] = "You dont have permission to access this game.";
                return Redirect("/");
            }
            var tags = dbContext.Tags.ToList();

            var viewModel = new EditGameViewModel
            {  
                Title = game.Title,
                Description = game.Description,
                ExistingImagePath = game.ImagePath, 
                ExistingFilePath = game.FilePath,  
                AvailableTags = tags.IsNullOrEmpty() ? tags : new List<Tag>(),
                SelectedTags = game.GameTags.Select(gt => gt.Tag.TagName).ToList() is null ? new List<string>() : game.GameTags.Select(gt => gt.Tag.TagName).ToList()
            };
                        
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditGame(int id, EditGameViewModel model)
        {
            var game = await dbContext.Games
                                      .Include(g => g.GameTags)
                                      .FirstOrDefaultAsync(g => g.GameID == id);

            if (model is null)
            {
                TempData["ErrorMessage"] = "Error in recieving model data : model is null. (probably sending more than 1GB of data)";
                return RedirectToAction("Error", "Home");
            }
            if (game == null)
            {
                TempData["ErrorMessage"] = $"not game with id{id} found";
                return NotFound();
            }
            model.SelectedTags = model.SelectedTags is null ? new() : model.SelectedTags;
            if (model.AvailableTags is null)
            {
                var tags = dbContext.Tags.ToList();
                model.AvailableTags = tags is null ? new() : tags;
            }
            if (String.IsNullOrEmpty(model.Title))
            {
                return View(model);
            }

            model.SelectedTags = model.SelectedTags is null ? new() : model.SelectedTags;
            if (model.AvailableTags is null)
            {
                var tags = dbContext.Tags.ToList();
                model.AvailableTags = tags is null ? new() : tags;
            }

            bool imagePathTest = model.ImagePath != null && model.ImagePath.Length > 0;
            bool fileTest = model.FilePath != null && model.FilePath.Length > 0;
            if (model.Title == String.Empty)
            {
                TempData["ErrorMessage"] = "You need to write Title";
                return View(model);
            }
            
            if (imagePathTest)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".svg", ".ico" };
                var extension = Path.GetExtension(model.ImagePath.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "Only image files are allowed (JPG, JPEG, PNG, GIF, BMP, TIFF, WEBP, SVG, ICO).";
                    return View(model);
                }

                var imagePath = Path.Combine(environment.WebRootPath, game.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath) && game.ImagePath != "/images/nothing.png")
                {
                    System.IO.File.Delete(imagePath);
                }

                string uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", model.Title);
                imagePath = Path.Combine(uploads, model.ImagePath.FileName);

                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    await model.ImagePath.CopyToAsync(fileStream);
                }
            }
            if (fileTest)
            {
                if (model.FilePath.Length > 300 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "File sze is more than 300 MB";
                    return View(model);
                }
                var filePath = Path.Combine(environment.WebRootPath, game.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                var uploadsFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "games", model.Title);
                filePath = Path.Combine(uploadsFile, model.FilePath.FileName);

                if (!Directory.Exists(uploadsFile))
                {
                    Directory.CreateDirectory(uploadsFile);
                }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.FilePath.CopyToAsync(fileStream);
                }
            }

            game.Title = model.Title;
            game.Description = model.Description is null ? "" : model.Description;
            game.ImagePath = imagePathTest ? $"/images/{game.GameID}/{model.ImagePath.FileName}" : game.ImagePath;
            game.FilePath = fileTest ? $"/games/{game.GameID}/{model.FilePath.FileName}" : game.FilePath;
            game.UploadDate = DateTime.Now;

            await dbContext.SaveChangesAsync();

            await RemoveGameTags(game.GameTags.ToList());            

            if (model.SelectedTags is not null)
            {
                foreach (string tagName in model.SelectedTags)
                {
                    var tag = await dbContext.Tags
                            .Where(t => t.TagName.ToLower() == tagName.ToLower())
                            .FirstOrDefaultAsync();

                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            TagName = tagName,
                            UseCount = 1
                        };
                        dbContext.Tags.Add(tag);
                    }
                    else
                    {
                        tag.UseCount += 1;

                        dbContext.Tags.Update(tag);
                    }
                    await dbContext.SaveChangesAsync();
                    var gameTag = new GameTag
                    {
                        GameID = game.GameID,
                        TagID = tag.TagID
                    };

                    dbContext.GameTags.Add(gameTag);
                }
            }

            dbContext.Games.Update(game);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Library", "Home");
        }

        private async Task RemoveGameTags(List<GameTag>? gameTags)
        {
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
            await dbContext.SaveChangesAsync();
        }
    }
}
