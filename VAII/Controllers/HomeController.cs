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

        public IActionResult Index()
        {
            var games = dbContext.Games.ToList();
            var tags = dbContext.Tags.ToList();

            var model = new GamesPlusTagsViewModel()
            {
                Games = games,
                Tags = tags
            };

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
