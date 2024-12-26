using VAII.Models.Entities;

namespace VAII.Models.DTO
{
    public class EditGameViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile ImagePath { get; set; }
        public IFormFile FilePath { get; set; }
        public string ExistingImagePath { get; set; }
        public string ExistingFilePath { get; set; }
        public List<string> SelectedTags { get; set; }
        public List<Tag> AvailableTags { get; set; }
    }
}
