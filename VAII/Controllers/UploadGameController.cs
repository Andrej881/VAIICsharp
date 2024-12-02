using Microsoft.AspNetCore.Mvc;
using VAII.Models.DTO;

namespace VAII.Controllers
{
    public class UploadGameController : Controller
    {
        [HttpGet]
        public IActionResult UploadGame()
        {
            return View();
        }
        [HttpPost]
        public IActionResult UploadGame(GameViewModel model)
        {
            return View();
        }
    }
}
