using FeuerSoftware.TetraControl2Connect.Models.TetraControl;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public interface ISDSService
    {
        Task HandleSds(TetraControlDto sds);
    }
}
