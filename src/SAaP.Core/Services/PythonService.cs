using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SAaP.Core.Services;

public static class PythonService
{
    public const string PyFolder = "py";
    // tdx script location
    public const string TdxReader = "tdx_reader.py";

    public const string PyName = "python.exe";

    public static Task RunPythonScript(string pythonExecFullPath, params string[] args)
    {
        //args generate
        var sb = new StringBuilder();
        const string blank = " ";
        const string d = "\"";
        foreach (var arg in args)
        {
            var trimmed = arg.Trim();
            // if blank exist in arg add ['] in ^$ is very necessary
            if (trimmed.Contains(blank))
            {
                sb.Append(blank).Append(d).Append(trimmed).Append(d).Append(blank);
            }
            else
            {
                sb.Append(blank).Append(trimmed).Append(blank);
            }
        }

        // process start info
        var startInfo = new ProcessStartInfo
        {
            FileName = pythonExecFullPath,
            Arguments = sb.ToString(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
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