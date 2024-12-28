using Microsoft.AspNetCore.Identity;
namespace VAII.Models.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public string UserID { get; set; }
        public CustomUser User { get; set; }
        public int GameID { get; set; }
        public int Value { get; set; }
        public string Content { get; set; }
    }
}
