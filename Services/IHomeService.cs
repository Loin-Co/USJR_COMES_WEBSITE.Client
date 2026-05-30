namespace USJR_COMES_WEBSITE.Services;
using USJR_COMES_WEBSITE.Models;

public interface IHomeService
{
    Task<List<Headline>?> GetHeadlinesAsync();
    Task<List<SlideItem>?> GetSlideItemsAsync();
    /// <summary>Bust the in-memory and browser HTTP caches for headlines and slide items.</summary>
    void InvalidateHomeCache();
}
