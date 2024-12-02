namespace VAII.Models.Entities
{
    public class Tag
    {
        public int TagID { get; set; }
        public string TagName { get; set; }
        public int UseCount { get; set; }
        public ICollection<GameTag> GameTags { get; set; } // Navigation Property
    }
}
