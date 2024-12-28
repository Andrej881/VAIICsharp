using VAII.Models.Entities;

namespace VAII.Models.DTO
{
    public class GameDescriptionViewModel
    {
        public int Id { get; set; }
        public string CurrentUser { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ExistingImagePath { get; set; }
        public string ExistingFilePath { get; set; }
        public List<string> SelectedTags { get; set; }
        public List<Review> Reviews { get; set; }
    }
}
