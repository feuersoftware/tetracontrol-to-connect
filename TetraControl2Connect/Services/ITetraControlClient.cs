using FeuerSoftware.TetraControl2Connect.Models.TetraControl;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public interface ITetraControlClient
    {
        IObservable<TetraControlDto> PositionReceived { get; }

        IObservable<TetraControlDto> SDSReceived { get; }

        IObservable<TetraControlDto> StatusReceived { get; }

        IObservable<bool> IsConnected { get; }

        void Init();

        Task Start();

        Task Stop();
    }
}