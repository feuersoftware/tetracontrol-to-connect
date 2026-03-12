using FeuerSoftware.TetraControl2Connect.Models.TetraControl;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public interface ISirenService : IDisposable
    {
        Task HandleSirenStatuscode(TetraControlDto dto);
        Task Initialize();
    }
}