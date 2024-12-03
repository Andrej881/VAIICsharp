using Microsoft.AspNetCore.Mvc;
using VAII.Models.DTO;
using VAII.Data;
using VAII.Models.Entities;
using Microsoft.EntityFrameworkCore;

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
                AvailableTags = tags
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> UploadGame(GameViewModel model)
        {
            if (model.ImagePath != null && model.ImagePath.Length > 0 && model.FilePath != null && model.FilePath.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                var imagePath = Path.Combine(uploads, model.ImagePath.FileName);

                var uploadsFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "games");
                var filePath = Path.Combine(uploads, model.FilePath.FileName);

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

                var Game = new Game
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImagePath = "/images/" + model.ImagePath.FileName,
                    FilePath = "/games/" + model.FilePath.FileName,
                    UploadDate = DateTime.Now
                };

                foreach (int tagID in model.SelectedTagIds)
                { 
                    
                }

                await dbContext.Games.AddAsync(Game);
                await dbContext.SaveChangesAsync();
            }           

            return RedirectToAction("Index", "Home");
        }
    }
}
