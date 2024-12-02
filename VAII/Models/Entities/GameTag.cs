namespace VAII.Models.Entities
{
    public class GameTag
    {
        public int GameID { get; set; }
        public Game Game { get; set; } // Navigation Property

        public int TagID { get; set; }
        public Tag Tag { get; set; } // Navigation Property
    }
}
