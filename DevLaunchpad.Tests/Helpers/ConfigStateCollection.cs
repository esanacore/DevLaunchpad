using Xunit;

namespace DevLaunchpad.Tests.Helpers;

/// <summary>
/// xUnit collection for tests that mutate the process-wide
/// <see cref="DevLaunchpadConfig.ConfigDirectoryOverride"/> static via <see cref="TempConfigDir"/>.
///
/// xUnit runs separate test classes in parallel by default. Because the config directory override
/// is global mutable state, any two such classes running concurrently would clobber each other's
/// temp directory. Assigning them all to one collection forces them to run sequentially.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ConfigStateCollection
{
    public const string Name = "ConfigState";
}
