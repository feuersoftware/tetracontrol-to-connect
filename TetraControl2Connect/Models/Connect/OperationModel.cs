namespace FeuerSoftware.TetraControl2Connect.Models.Connect
{
    public record OperationModel
    {
        public DateTime Start { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdateAt { get; set; }

        public string? Keyword { get; set; }

        public AddressModel Address { get; set; } = new();

        public PositionModel? Position { get; set; }

        public ReporterModel? Reporter { get; set; }

        public string? Facts { get; set; }

        public string? Ric { get; set; }

        public string? Number { get; set; }

        public string? Source { get; set; }

        public List<OperationPropertyModel> Properties { get; set; } = [];
    }
}
