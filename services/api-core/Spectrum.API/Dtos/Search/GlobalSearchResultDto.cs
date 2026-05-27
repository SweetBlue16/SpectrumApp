namespace Spectrum.API.Dtos.Search
{
    public class GlobalSearchResultDto
    {
        public IReadOnlyList<GlobalSearchItemDto> Games { get; set; } = [];
        public IReadOnlyList<GlobalSearchItemDto> Users { get; set; } = [];
    }
}
