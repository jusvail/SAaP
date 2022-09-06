using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SAaP.Core.Services;

public static class PythonService
{
    private const string PyFolder = "py/";
    // tdx script location
    public const string TdxReader = "tdx_reader.py";

    public static Task RunPythonScript(string pyScriptName, params string[] args)
    {
        // py script location
        var path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + PyFolder + pyScriptName;

        //args generate
        var sb = new StringBuilder(path);
        foreach (var arg in args)
            sb.Append(" ").Append(arg);

        // process start info
        var startInfo = new ProcessStartInfo
        {
            FileName = "C:/devEnv/Python/Python310/python.exe", // TODO custom py env location await
            Arguments = sb.ToString(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            CreateNoWindow = false
        };

        // new task
        return Task.Run(() =>
        {
            // start process
            using var process = Process.Start(startInfo);

            if (process == null) return;

            using var reader = process.StandardOutput;
            var result = reader.ReadToEnd();

            //TODO error catch await
            Console.Write(result);
        });
    }
}