using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
        public IActionResult Reviews(int gameID, int reviewID)
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
            Review review = reviewID == 0 ? new Review() { GameID = gameID} : dbContext.Reviews.Find(reviewID);
            if (userManager.GetUserId(User) != review.UserID)
            {
                TempData["ErrorMessage"] = "You dont have permission to edit someonesElse review.";
                return Forbid();
            }
            return View(review);
        }
        [HttpPost]
        public async Task<IActionResult> Reviews(Review review)
        {
            Review testReview = await dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == review.Id); 
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
            if (testReview != null)
            {
                if (review.UserID != testReview.UserID)
                {
                    TempData["ErrorMessage"] = "You dont have permission to edit someonesElse review.";
                    return Forbid();
                }
                testReview.Value = review.Value;
                testReview.Content = review.Content;
                dbContext.Reviews.Update(testReview);
            }
            else
            {
                await dbContext.Reviews.AddAsync(review);
            }
            await dbContext.SaveChangesAsync();

            return RedirectToAction("GameDescription", "Game", new { id = review.GameID });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            Review review = await dbContext.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            if (userManager.GetUserId(User) != review.UserID)
            {
                return Forbid();
            }

            dbContext.Reviews.Remove(review);
            
            await dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
