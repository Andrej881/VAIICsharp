using VAII.Models.Entities;

namespace VAII.Models.DTO
{
    public class GamesPlusTagsViewModel
    {
        public IEnumerable<Game> Games { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
    }
}
