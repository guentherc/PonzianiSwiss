using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiSwissLib
{
    public class PairingTool
    {
        public PairingTool()
        {
            executable = "bbpPairings";
            if (RuntimeInformation.OSArchitecture == Architecture.X86) executable += "32";
            else if (RuntimeInformation.OSArchitecture == Architecture.X64) executable += "64";
            else throw new Exception($"OS Architecture { RuntimeInformation.OSArchitecture } not supported! Only x86 and x64 architecture supported!");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) executable += ".exe";
            else if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) throw new Exception($"OS {RuntimeInformation.OSDescription} not supported");
            executable = Path.Combine("bbpPairings", executable);
            Trace.WriteLine($"Archicture {RuntimeInformation.OSArchitecture }, OS {RuntimeInformation.OSDescription} detected! => {executable} will be used!");
        }

        public bool CheckExecutable()
        {
            var psi = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using var process = Process.Start(psi);
            if (process == null) return false;
            using StreamReader reader = process.StandardOutput;
            string data = reader.ReadToEnd();

            return data.Contains("BBP Pairings", StringComparison.CurrentCulture);
        }

        public string Pair(string input)
        {
            var psi = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = $"--dutch {input} -p"
            };

            using var process = Process.Start(psi);
            if (process == null) return string.Empty;
            using StreamReader reader = process.StandardOutput;
            string data = reader.ReadToEnd();

            return data;

        }

        private readonly string executable;
    }
}
