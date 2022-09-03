using System;
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
        private static readonly string PyFolder = "py/";
        public const string TdxReader = "tdx_reader.py";

        public void run_cmd(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.FileName = "C:/devEnv/Python/Python310/python.exe";
            start.Arguments = $"{cmd} {args}";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            using Process process = Process.Start(start);
            if (process != null)
            {
                using StreamReader reader = process.StandardOutput;
                string result = reader.ReadToEnd();
                Console.Write(result);
            }
        }

        public static void RunPythonScript(string pyScriptName, params string[] args)
        {
            var p = new Process();
            var path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + PyFolder + pyScriptName;
            
            var sb = new StringBuilder(path);
            foreach (var arg in args)
                sb.Append(" ").Append(arg);

            p.StartInfo.FileName = "C:/devEnv/Python/Python310/python.exe";
            p.StartInfo.Arguments = sb.ToString();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            p.Start();   
            p.BeginOutputReadLine();
            Console.ReadLine();
            p.WaitForExit();
        }
        public delegate void AppendTextCallback(string text);
    }
}
