using System.Diagnostics;

namespace FaunaFinder.AppHost.Extensions;

public static class AzureCliHelper
{
    public static async Task<(int ExitCode, string Output, string Error)> RunCommandAsync(
        string arguments,
        CancellationToken ct = default
    )
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "az",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            },
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(ct);
        var error = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        return (process.ExitCode, output, error);
    }

    public static string? GetEnvironmentVariable(string name) =>
        Environment.GetEnvironmentVariable(name);

    public static string? ResourceGroup => GetEnvironmentVariable("AZURE_RESOURCE_GROUP");
    public static string? CustomDomain => GetEnvironmentVariable("CUSTOM_DOMAIN");
}
