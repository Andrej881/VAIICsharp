using VAII.Models.Entities;

namespace VAII.Models.DTO
{
    public class GamesPlusTagsViewModel
    {
        public IEnumerable<Game> Games { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public List<string> SelectedTags { get; set; }
        public string Search { get; set; }

    }
}
