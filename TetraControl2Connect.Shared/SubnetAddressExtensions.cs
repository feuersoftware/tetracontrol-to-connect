using FeuerSoftware.TetraControl2Connect.Shared.Options.Models;

namespace FeuerSoftware.TetraControl2Connect.Shared
{
    public static class SubnetAddressExtensions
    {
        public static string ToStringForConnect(this SubnetAddress sna, bool useFullyQualifiedSubnetAddressForConnect)
        {
            return useFullyQualifiedSubnetAddressForConnect ?
                $"T2C({sna.GSSI}_{sna.SNA} - {sna.Name})" :
                $"SNA({sna.SNA})";
        }
    }
}
