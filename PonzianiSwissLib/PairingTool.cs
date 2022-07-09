using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PonzianiSwissLib
{
    public class PairingTool
    {

        public static void Initialize(ILogger? logger = null)
        {
            if (logger != null) Logger = logger;
            if (executable != null) return;
            executable = "bbpPairings";
            if (RuntimeInformation.OSArchitecture == Architecture.X86) executable += "32";
            else if (RuntimeInformation.OSArchitecture == Architecture.X64) executable += "64";
            else throw new Exception($"OS Architecture {RuntimeInformation.OSArchitecture} not supported! Only x86 and x64 architecture supported!");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) executable += ".exe";
            else if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) throw new Exception($"OS {RuntimeInformation.OSDescription} not supported");
            if (!File.Exists(Path.Combine("bbpPairings", executable)))
            {
                executable = LoadFromOneDrive();
            }
            else executable = Path.Combine("bbpPairings", executable);
            Logger?.LogInformation("Architecture {architecture}, OS {os} detected! => {executable} will be used!", RuntimeInformation.OSArchitecture, RuntimeInformation.OSDescription, executable);
        }

        private static ILogger? Logger;

        private static string LoadFromOneDrive()
        {
            if (executable == null)
            {
                Logger?.LogCritical("Inconsistency - Pairing Tool Executable is initial");
                throw new Exception("Inconsistency - Pairing Tool Executable is initial");
            }
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bbpPairings", executable);
            if (File.Exists(file))
            {
                executable = file;
                return executable;
            }
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bbpPairings");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Logger?.LogDebug("Directory {dir} created!", directory);
            }
            //Download from One Drive
            string url = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (RuntimeInformation.OSArchitecture == Architecture.X86) url = "https://onedrive.live.com/download?cid=88BDB7962BB5DA40&resid=88BDB7962BB5DA40%21855896&authkey=AAGA8xogdvOvoYc";
                else if (RuntimeInformation.OSArchitecture == Architecture.X64) url = "https://onedrive.live.com/download?cid=88BDB7962BB5DA40&resid=88BDB7962BB5DA40%21855895&authkey=AGd-uVZOaW4wgKM";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (RuntimeInformation.OSArchitecture == Architecture.X86) url = "https://onedrive.live.com/download?cid=88BDB7962BB5DA40&resid=88BDB7962BB5DA40%21855815&authkey=AHx9MQedkE5Vxk8";
                else if (RuntimeInformation.OSArchitecture == Architecture.X64) url = "https://onedrive.live.com/download?cid=88BDB7962BB5DA40&resid=88BDB7962BB5DA40%21855813&authkey=AH1TocDLxKao7eg";

            }
            else throw new Exception($"OS {RuntimeInformation.OSDescription} not supported");
            Logger?.LogInformation("Downloading executables from {url}", url);
            HttpClient client = new();
            using var stream = client.GetStreamAsync(url).Result;
            using var fileStream = new FileStream(file, FileMode.CreateNew);
            stream.CopyToAsync(fileStream).Wait();
            executable = file;
            return executable;
        }

        public static async Task<bool> CheckExecutableAsync()
        {
            Initialize();
            var psi = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            using var process = Process.Start(psi);
            if (process == null) return false;
            using StreamReader reader = process.StandardOutput;
            string data = await reader.ReadToEndAsync();

            return data.Contains("BBP Pairings", StringComparison.CurrentCulture);
        }

        public static async Task<string> PairAsync(string input, PairingSystem pairingSystem = PairingSystem.Dutch)
        {
            Initialize();
            Debug.Assert(File.Exists(executable));
            var psi = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                Arguments = pairingSystem == PairingSystem.Dutch ? $"--dutch {input} -p" : $"--burstein {input} -p"
            };

            Logger?.LogInformation("Start Draw {exe} {args}", Path.GetFullPath(executable), psi.Arguments);
            Stopwatch sw = new();
            sw.Start();
            using var process = Process.Start(psi);
            if (process == null) return string.Empty;
            using StreamReader reader = process.StandardOutput;
            string data = await reader.ReadToEndAsync();
            sw.Stop();
            Logger?.LogInformation("Draw completed ({runtime} ms)", sw.Elapsed.TotalMilliseconds);
            return data;

        }

        public static async Task<string?> GenerateTRFAsync(int seed = 0, GeneratorConfig? config = null)
        {
            Initialize();
            string tmpFile = string.Empty;
            string pairing_system = "dutch";
            if (config != null)
            {
                tmpFile = Path.GetTempFileName();
                await File.WriteAllLinesAsync(tmpFile, config.CreateConfigFileContent(), Encoding.ASCII);
                if (config.PairingSystem == PairingSystem.Burstein) pairing_system = "burstein";
            }
            string oFile = Path.GetTempFileName();
            string trfname = Path.ChangeExtension(oFile, "trf");
            var psi = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                Arguments = $"--{pairing_system}  -g {tmpFile} -o {trfname}"
            };
            if (seed != 0) psi.Arguments += $" -s {seed}";

            Logger?.LogInformation("Generate TRF {exe} {args}", executable != null ? Path.GetFullPath(executable) : string.Empty, psi.Arguments);

            using var process = Process.Start(psi);
            if (process == null) return null;
            await process.WaitForExitAsync();
            return trfname;
        }

        public static async Task<Tournament?> GenerateAsync(int seed = 0, GeneratorConfig? config = null)
        {
            Initialize();
            string? trfname = await GenerateTRFAsync(seed, config);
            if (trfname != null)
            {
                Tournament tournament = new();
                tournament.LoadFromTRF(File.ReadAllText(trfname));
                return tournament;
            }
            else return null;
        }

        public static async Task<string> CheckAsync(string input, PairingSystem pairingSystem = PairingSystem.Dutch)
        {
            Initialize();
            Debug.Assert(File.Exists(executable));
            var psi = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = pairingSystem == PairingSystem.Dutch ? $"--dutch {input} -c" : $"--burstein {input} -c"
            };

            string cmd = $"{DateTime.Now} \"{Path.GetFullPath(executable)}\" {psi.Arguments}";
            Trace.WriteLine(cmd);

            using var process = Process.Start(psi);
            if (process == null) return string.Empty;
            using StreamReader reader = process.StandardOutput;
            string data = await reader.ReadToEndAsync();

            return data;
        }

        private static string? executable;

        /// <summary>
        /// Configuration for Tournament Generator
        /// </summary>
        public class GeneratorConfig
        {
            /// <summary>
            /// Pairing system to be used
            /// </summary>
            public PairingSystem PairingSystem { get; set; } = PairingSystem.Dutch;
            /// <summary>
            /// Number of Participants
            /// </summary>
            public int CountParticipants { get; set; } = 100;
            /// <summary>
            /// Number of Rounds
            /// </summary>
            public int CountRounds { get; set; } = 9;

            /// <summary>
            /// Rate for forfeited games (scheduled but not played) in per thousand
            /// </summary>
            public int ForfeitRate { get; set; } = 20;

            /// <summary>
            /// Highest possible rating of a player in the tournament
            /// </summary>
            public int HighestRating { get; set; } = 2700;

            /// <summary>
            /// Lowest possible rating of a player in the tournament
            /// </summary>
            public int LowestRating { get; set; } = 700;

            public ScoringScheme ScoringScheme { get; set; } = new();

            internal List<string> CreateConfigFileContent()
            {
                List<string> content = new()
                {
                    $"PlayersNumber={CountParticipants}",
                    $"RoundsNumber={CountRounds}",
                    $"ForfeitRate={ForfeitRate}",
                    $"HighestRating={HighestRating}",
                    $"LowestRating={LowestRating}"
                };
                if (!ScoringScheme.IsDefault)
                {
                    content.Add(FormattableString.Invariant($"PointsForWin={ScoringScheme.PointsForWin:F1}"));
                    content.Add(FormattableString.Invariant($"PointsForDraw={ScoringScheme.PointsForDraw:F1}"));
                    content.Add(FormattableString.Invariant($"PointsForLoss={ScoringScheme.PointsForPlayedLoss:F1}"));
                    content.Add(FormattableString.Invariant($"PointsForZPB={ScoringScheme.PointsForZeroPointBye:F1}"));
                    content.Add(FormattableString.Invariant($"PointsForPAB={ScoringScheme.PointsForPairingAllocatedBye:F1}"));
                    content.Add(FormattableString.Invariant($"PointsForForfeitLoss={ScoringScheme.PointsForForfeitedLoss:F1}"));
                }

                return content;
            }
        }
    }
}

