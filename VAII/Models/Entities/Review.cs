using Microsoft.AspNetCore.Identity;
namespace VAII.Models.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public int GameID { get; set; }
        public string Content { get; set; }
    }
}
