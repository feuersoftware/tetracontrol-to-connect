namespace FeuerSoftware.TetraControl2Connect.Models.Connect
{
    public record UserStatusModel
    {
        public string PagerIssi { get; set; } = string.Empty;

        public UserOperationStatus Status { get; set; }
    }
}
