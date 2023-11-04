#nullable enable
namespace SAaP.Models;

public class LocalSettingsOptions
{
    public string? ApplicationDataFolder
    {
        get; set;
    }

    public string? LocalSettingsFile
    {
        get; set;
    }

    public string? TdxInstallationPath
    {
        get; set;
    }

    public string? PythonInstallationPathFull
    {
        get; set;
    }
}
