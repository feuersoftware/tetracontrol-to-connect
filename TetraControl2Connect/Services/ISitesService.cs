using FeuerSoftware.TetraControl2Connect.Models.Connect;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public interface ISitesService
    {
        SiteModel GetSiteInfo(string accessToken);
        Task Initialize();
    }
}