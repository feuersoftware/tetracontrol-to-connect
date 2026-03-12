namespace FeuerSoftware.TetraControl2Connect.Models.Connect
{
    public record UserModel
    {
        public string Id { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string PagerIssi { get; set; } = string.Empty;

        // Not supported by API
        public int OrganizationId { get; set; }
    }
}
