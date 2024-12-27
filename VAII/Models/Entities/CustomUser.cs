using Microsoft.AspNetCore.Identity;

namespace VAII.Models.Entities
{
    public class CustomUser : IdentityUser
    {
        public string ImagePath { get; set; }
    }
}
