using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;
using VAII.Data;
using VAII.Models;
using VAII.Models.DTO;
using VAII.Models.Entities;

namespace VAII.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext dbContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            _logger = logger;
        }        
        
        public IActionResult Index(string search, GamesPlusTagsViewModel sendModel)
        {
            var gamesQuery = dbContext.Games.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                gamesQuery = gamesQuery.Where(game => game.Title.ToLower().Contains(search.ToLower()));
            }
            if (sendModel.SelectedTags is not null)
            {
                gamesQuery = gamesQuery.Where(game => sendModel.SelectedTags.All(selectedTag => game.GameTags.Any(gameTag => gameTag.Tag.TagName == selectedTag)));
            }
            var tags = dbContext.Tags.ToList();
            var games = gamesQuery.ToList();

            var selected = new List<string>();
            foreach (var tag in tags)
            { 
                selected.Add(tag.TagName);
            }

            var model = new GamesPlusTagsViewModel()
            {
                Games = games,
                Tags = tags,
                Search = search,
                //SelectedTags = sendModel.SelectedTags is not null ? sendModel.SelectedTags : new List<Tag>()
                SelectedTags = sendModel.SelectedTags is not null ? sendModel.SelectedTags : selected
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_GamesList", model);
            }

            return View(model);
        }

        public IActionResult Library() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var games = dbContext.Games.Where(game => game.UserID == userId).ToList();

            return View(games);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
