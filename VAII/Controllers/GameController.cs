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

        public GameController(ApplicationDbContext dbContext)
        { 
            this.dbContext = dbContext;
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
            bool pathsTest = model.ImagePath != null && model.ImagePath.Length > 0 && model.FilePath != null && model.FilePath.Length > 0;
            bool modelTest = model.Title != String.Empty;
            if (modelTest && pathsTest)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                var imagePath = Path.Combine(uploads, model.ImagePath.FileName);

                var uploadsFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "games");
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
                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    await model.ImagePath.CopyToAsync(fileStream);
                }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImagePath.CopyToAsync(fileStream);
                }

                var game = new Game
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImagePath = "/images/" + model.ImagePath.FileName,
                    FilePath = "/games/" + model.FilePath.FileName,
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

            return RedirectToAction("Index", "Home");
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
    }
}
