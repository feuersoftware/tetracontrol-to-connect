using FeuerSoftware.TetraControl2Connect.Models.TetraControl;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public interface ISDSService : IDisposable
    {
        Task HandleSds(TetraControlDto sds);
    }
}
