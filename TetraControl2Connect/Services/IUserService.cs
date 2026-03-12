
using FeuerSoftware.TetraControl2Connect.Models.Connect;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public interface IUserService : IDisposable
    {
        List<string> GetAccessTokensForUser(string pagerIssi);

        IEnumerable<UserModel> GetUsers(string pagerIssi);

        Task Initialize();
    }
}