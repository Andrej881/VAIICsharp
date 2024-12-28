using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAII.Data;
using VAII.Models.DTO;
using VAII.Models.Entities;

namespace VAII.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<CustomUser> userManager;
        public ReviewController(ApplicationDbContext dbContext, UserManager<CustomUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }
        [HttpGet]
        public IActionResult Reviews(int gameID)
        {
            if (userManager.GetUserId(User) is null)
            {
                var returnUrl = Request.Path + "?gameID=" + gameID;
                return Redirect($"/Identity/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }
            Game game = dbContext.Find<Game>(gameID);
            if (game == null) 
            {
                return NotFound();
            }
            Review review = new Review() { GameID = gameID};

            return View(review);
        }
        [HttpPost]
        public async Task<IActionResult> Reviews(Review review)
        {
            if (String.IsNullOrEmpty(review.Content) || review.Value < 1 || review.Value > 5)
            {
                TempData["ErrorMessage"] = "You didn't fill all required spaces or used incorrect values";
                return View(review);
            }
            if (userManager.GetUserId(User) is null)
            {
                TempData["ErrorMessage"] = "You are not logged in";
                return Redirect("/Identity/Account/Login");
            }
            review.UserID = userManager.GetUserId(User);

            await dbContext.Reviews.AddAsync(review);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("GameDescription", "Game", new { id = review.GameID });
        }
    }
}
