namespace VAII.Models.Entities
{
    public class Game
    {
        public int GameID { get; set; } // Primary Key
        public string Title { get; set; } 
        public string Description { get; set; }
        public string ImagePath { get; set; } 
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }         
        public ICollection<GameTag> GameTags { get; set; } // Navigation Property
    }
}
