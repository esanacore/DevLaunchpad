using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DevLaunchpad;

/// <summary>
/// JSON serialization context for the Dev Launchpad extension using source-generated serializers.
/// 
/// This class leverages System.Text.Json source generation to provide compile-time JSON 
/// serialization and deserialization for configuration objects without runtime reflection.
/// This approach improves performance and reduces bundle size compared to reflection-based serialization.
/// </summary>
/// <remarks>
/// Configured for pretty-printed JSON output (WriteIndented = true) to make configuration files 
/// human-readable when stored or viewed.
/// </remarks>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DevLaunchpadConfig))]
[JsonSerializable(typeof(NamedUrl))]
[JsonSerializable(typeof(CustomCommandConfig))]
[JsonSerializable(typeof(List<NamedUrl>))]
[JsonSerializable(typeof(List<CustomCommandConfig>))]
[JsonSerializable(typeof(List<string>))]
internal sealed partial class DevLaunchpadJsonContext : JsonSerializerContext
{
}