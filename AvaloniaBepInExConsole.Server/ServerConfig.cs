using System.Text.Json.Serialization;

namespace Sigurd.AvaloniaBepInExConsole;

public class ServerConfig
{
    [JsonPropertyName("aeron_directory_name")]
    public string? AeronDirectoryName { get; init; }
}
