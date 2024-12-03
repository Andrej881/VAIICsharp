using Microsoft.AspNetCore.Mvc;
using VAII.Models.DTO;
using VAII.Data;
using VAII.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Azure;

namespace VAII.Controllers
{
    public class GameController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IWebHostEnvironment environment;

        public GameController(ApplicationDbContext dbContext, IWebHostEnvironment environment)
        {
            this.dbContext = dbContext;
            this.environment = environment;
        }

        public IActionResult Index()
        {
            var games = dbContext.Games.ToList();

            return View(games);
        }

        [HttpGet]
        public IActionResult UploadGame()
        {
            // Fetch all tags from the database
            var tags = dbContext.Tags.ToList();
            var viewModel = new GameViewModel
            {
                AvailableTags = tags.IsNullOrEmpty() ? tags : new List<Tag>(),
                SelectedTags = new List<string>(),
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> UploadGame(GameViewModel model)
        {
            bool imagePathTest = model.ImagePath != null && model.ImagePath.Length > 0;
            bool fileTest = model.FilePath != null && model.FilePath.Length > 0;
            bool modelTest = model.Title != String.Empty;
            if (modelTest && fileTest)
            {
                string uploads, imagePath;
                if (imagePathTest)
                {
                    uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", model.Title);
                    imagePath = Path.Combine(uploads, model.ImagePath.FileName);
                }
                else
                {
                    uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    imagePath = Path.Combine(uploads, "nothing.png");
                }
                

                var uploadsFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "games", model.Title);
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

                var game = new Game
                {
                    Title = model.Title,
                    Description = model.Description is null ? "" : model.Description,
                    ImagePath = imagePathTest ? $"/images/{model.Title}/{model.ImagePath.FileName}" : "/images/nothing.png",
                    FilePath = $"/games/{model.Title}/{model.FilePath.FileName}",
                    UploadDate = DateTime.Now
                };

                if (model.SelectedTags is not null)
                {
                    foreach (string tagName in model.SelectedTags)
                    {
                        var tag = await dbContext.Tags
                                .FirstOrDefaultAsync(t => t.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase));

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
                        var gameTag = new GameTag
                        {
                            GameID = game.GameID,
                            TagID = tag.TagID
                        };

                        dbContext.GameTags.Add(gameTag);
                    }
                }                              

                await dbContext.Games.AddAsync(game);
                await dbContext.SaveChangesAsync();
            }           

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GameDescription(int id)
        {
            var game = dbContext.Games
                           .Include(g => g.GameTags) 
                           .FirstOrDefault(g => g.GameID == id);

            if (game == null)
            {
                return NotFound();
            }

            return View(game);
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

            var imagePath = Path.Combine(environment.WebRootPath, game.ImagePath.TrimStart('/'));
            var filePath = Path.Combine(environment.WebRootPath, game.FilePath.TrimStart('/'));

            // Delete the image file if it exists
            if (System.IO.File.Exists(imagePath) && game.ImagePath != "/images/nothing.png")
            {
                System.IO.File.Delete(imagePath);
            }

            // Delete the game file if it exists
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            dbContext.Games.Remove(game);
            await dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
