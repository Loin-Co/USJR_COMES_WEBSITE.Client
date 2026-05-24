namespace USJR_COMES_WEBSITE.Services;
using USJR_COMES_WEBSITE.Models;

public interface IHomeService
{
    Task<List<Headline>?> GetHeadlinesAsync();
    Task<List<SlideItem>?> GetSlideItemsAsync();
}
