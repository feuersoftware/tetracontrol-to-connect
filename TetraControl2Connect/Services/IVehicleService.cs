using FeuerSoftware.TetraControl2Connect.Models.TetraControl;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public interface IVehicleService : IDisposable
    {
        Task HandleVehiclePosition(TetraControlDto dto);

        Task HandleVehicleStatus(TetraControlDto dto);

        Task Initialize();
    }
}