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

                await dbContext.Games.AddAsync(game);
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

            }           

            return RedirectToAction("Index");
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

            

            EditGameViewModel gameView = new()
            {
                Id = id,
                Title = game.Title,
                Description = game.Description,
                ExistingImagePath = game.ImagePath,
                ExistingFilePath = game.FilePath,
                SelectedTags = game.GameTags.Select(gt => gt.Tag.TagName).ToList() is null ? new List<string>() : game.GameTags.Select(gt => gt.Tag.TagName).ToList()
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
            await dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult EditGame(int id)
        {
            var game = dbContext.Games
                                .Include(g => g.GameTags)
                                .ThenInclude(gt => gt.Tag)
                                .FirstOrDefault(g => g.GameID == id);

            if (game == null)
            {
                return NotFound();
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

            if (game == null)
            {
                return NotFound();
            }

            if (String.IsNullOrEmpty(model.Title))
            {
                return View(model);
            }

            bool imagePathTest = model.ImagePath != null && model.ImagePath.Length > 0;
            bool fileTest = model.FilePath != null && model.FilePath.Length > 0;

            if (imagePathTest)
            {
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
            game.ImagePath = imagePathTest ? $"/images/{model.Title}/{model.ImagePath.FileName}" : game.ImagePath;
            game.FilePath = fileTest ? $"/games/{model.Title}/{model.FilePath.FileName}" : game.FilePath;
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

            return RedirectToAction("Index");
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
