namespace VAII.Models.DTO
{
    public class GameViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile ImagePath { get; set; }
        public IFormFile FilePath { get; set; }
        public List<int> SelectedTagIds { get; set; }

    }
}
