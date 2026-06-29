using FeuerSoftware.TetraControl2Connect.Models.Connect;

namespace FeuerSoftware.TetraControl2Connect.Extensions
{
    public static class OperationModelExtensions
    {
        private static readonly TimeSpan ActiveTimeSpan = TimeSpan.FromMinutes(10);

        public static bool IsRecentlyAlertedOrUpdated(this OperationModel operation)
        {
            var recentlyCreated = (DateTimeOffset.Now - operation.CreatedAt).Duration() < ActiveTimeSpan;

            if (recentlyCreated)
            {
                return true;
            }

            var recentlyUpdated = operation.LastUpdateAt.HasValue &&
                (DateTimeOffset.Now - operation.LastUpdateAt.Value).Duration() < ActiveTimeSpan;

            return recentlyUpdated;
        }
    }
}
