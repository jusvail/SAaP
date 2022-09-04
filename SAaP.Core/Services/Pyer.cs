using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAaP.Core.Services
{
    public class Pyer
    {
        private const string PyFolder = "py/";
        public const string TdxReader = "tdx_reader.py";

        public void run_cmd(string cmd, string args)
        {
            var start = new ProcessStartInfo
            {
                FileName = "C:/devEnv/Python/Python310/python.exe",
                Arguments = $"{cmd} {args}",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using var process = Process.Start(start);
            if (process != null)
            {
                using StreamReader reader = process.StandardOutput;
                string result = reader.ReadToEnd();
                Console.Write(result);
            }
        }

        public static Task RunPythonScript(string pyScriptName, params string[] args)
        {
            var path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + PyFolder + pyScriptName;
            
            var sb = new StringBuilder(path);
            foreach (var arg in args)
                sb.Append(" ").Append(arg);

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

           return Task.Run(() =>{

                    using var process = Process.Start(startInfo);

                    if (process == null) return;

                    using var reader = process.StandardOutput;
                    var result = reader.ReadToEnd();

                    //TODO error catch await
                    Console.Write(result);
                });
        }
    }
}
