using System.Text.Json.Serialization;

namespace FeuerSoftware.TetraControl2Connect.Models
{
    internal sealed record GitHubReleaseModel(
        [property: JsonPropertyName("tag_name")] string TagName,
        [property: JsonPropertyName("html_url")] string HtmlUrl,
        [property: JsonPropertyName("prerelease")] bool Prerelease,
        [property: JsonPropertyName("draft")] bool Draft);
}
