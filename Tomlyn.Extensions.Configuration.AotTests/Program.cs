using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Tomlyn.Extensions.Configuration.AotTests;

internal static class Program
{
    private static int Main()
    {
        var failures = new List<string>();

        var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.toml");
        var appSettingsWithUnderscoresPath = Path.Combine(AppContext.BaseDirectory, "appsettings_with_underscores.toml");

        var configuration = new ConfigurationBuilder()
            .AddTomlFile(appSettingsPath, optional: false, reloadOnChange: false)
            .Build();

        Expect(configuration["AllowedHosts"] == "*", "AllowedHosts", failures);
        Expect(configuration["Server:Port"] == "8080", "Server:Port", failures);
        Expect(configuration["Logging:LogLevel:Default"] == "Information", "Logging:LogLevel:Default", failures);
        Expect(configuration["Logging:LogLevel:Microsoft"] == "Warning", "Logging:LogLevel:Microsoft", failures);
        Expect(configuration["Logging:LogLevel:Microsoft.Hosting.Lifetime"] == "Information", "Logging:LogLevel:Microsoft.Hosting.Lifetime", failures);

        using (var stream = File.OpenRead(appSettingsPath))
        {
            var streamConfiguration = new ConfigurationBuilder()
                .AddTomlStream(stream)
                .Build();
            Expect(streamConfiguration["Server:Port"] == "8080", "AddTomlStream Server:Port", failures);
        }

        var underscoreConfiguration = new ConfigurationBuilder()
            .AddTomlFile(appSettingsWithUnderscoresPath, optional: false, reloadOnChange: false)
            .Build()
            .RemoveUnderscores();
        Expect(underscoreConfiguration["Server:MaxRequestBodyBytes"] == "10485760", "RemoveUnderscores", failures);

        if (failures.Count > 0)
        {
            foreach (var failure in failures)
            {
                Console.Error.WriteLine($"FAILED: {failure}");
            }

            return 1;
        }

        Console.WriteLine("NativeAOT configuration smoke test passed.");
        return 0;
    }

    private static void Expect(bool condition, string name, List<string> failures)
    {
        if (!condition)
        {
            failures.Add(name);
        }
    }
}
