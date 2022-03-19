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
        static PairingTool()
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

        public static async Task<bool> CheckExecutableAsync()
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
            string data = await reader.ReadToEndAsync();

            return data.Contains("BBP Pairings", StringComparison.CurrentCulture);
        }

        public static async Task<string> PairAsync(string input, PairingSystem pairingSystem = PairingSystem.Dutch)
        {
            Debug.Assert(File.Exists(executable));
            var psi = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = pairingSystem == PairingSystem.Dutch ? $"--dutch {input} -p" : $"--burstein {input} -p"
            };

            string cmd = $"\"{Path.GetFullPath(executable)}\" {psi.Arguments}";
            Trace.WriteLine(cmd);

            using var process = Process.Start(psi);
            if (process == null) return string.Empty;
            using StreamReader reader = process.StandardOutput;
            string data = await reader.ReadToEndAsync();

            return data;

        }

        public static async Task<string?> GenerateTRFAsync(int seed = 0, GeneratorConfig? config = null)
        {
            string tmpFile = string.Empty;
            string pairing_system = "dutch";
            if (config != null)
            {
                tmpFile = Path.GetTempFileName();
                await File.WriteAllLinesAsync(tmpFile, config.CreateConfigFileContent(), Encoding.UTF8);
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
                Arguments = $"--{ pairing_system }  -g {tmpFile} -o {trfname}"
            };
            if (seed != 0) psi.Arguments += $" -s {seed}";
            using var process = Process.Start(psi);
            if (process == null) return null;
            await process.WaitForExitAsync();
            return trfname;
        }

        public static async Task<Tournament?> GenerateAsync(int seed = 0, GeneratorConfig? config = null)
        {
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
            Debug.Assert(File.Exists(executable));
            var psi = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = pairingSystem == PairingSystem.Dutch ? $"--dutch {input} -c" : $"--burstein {input} -c"
            };

            string cmd = $"\"{Path.GetFullPath(executable)}\" {psi.Arguments}";
            Trace.WriteLine(cmd);

            using var process = Process.Start(psi);
            if (process == null) return string.Empty;
            using StreamReader reader = process.StandardOutput;
            string data = await reader.ReadToEndAsync();

            return data;
        }    

        private static readonly string executable;

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
            /// Rate for games ended in less than one full move in per thousand
            /// </summary>
            public int QuickgameRate { get; set; } = 5;

            /// <summary>
            /// Rate for players announcing their absence in a particular round in per thousand
            /// </summary>
            public int ZeroPointByeRate { get; set; } = 5;

            /// <summary>
            /// Rate for players asking for a half-point-bye in per thousand
            /// </summary>
            public int HalfPointByeRate { get; set; } = 0;

            /// <summary>
            /// Rate for players given a full-point-bye in per thousand
            /// </summary>
            public int FullPointByeRate { get; set; } = 0;

            /// <summary>
            /// Highest possible rating of a player in the tournament
            /// </summary>
            public int HighestRating { get; set; } = 2700;

            /// <summary>
            /// Lowest possible rating of a player in the tournament
            /// </summary>
            public int LowestRating { get; set; } = 700;

            /// <summary>
            ///  <see cref="Groups"/> and Separator work in obscure ways 
            ///  <para>Indicatively, Separator defines how many points the rating of the median
            /// player is below the medium point between the highest and the lowest ratings.</para>
            /// </summary>
            public int Separator { get; set; } = 100;

            /// <summary>
            /// Players rating are distributed in a gaussian way around the rating of the median player (in the rating limits expressed above).
            /// The sigma is indicatively given by "(HighestRating - LowestRating) / (Groups + 1)" However, Groups is automatically 
            /// incremented when too many players would pass the HighestRating
            /// </summary>
            public int Groups { get; set; } = 2;

            public ScoringScheme ScoringScheme { get; set; } = ScoringScheme.Default;

            internal List<string> CreateConfigFileContent()
            {
                List<string> content = new();
                content.Add($"PlayersNumber={CountParticipants}");
                content.Add($"RoundsNumber={CountRounds}");
                content.Add($"ForfeitRate={ForfeitRate}");
                content.Add($"QuickgameRate={QuickgameRate}");
                content.Add($"ZPBRate={ZeroPointByeRate}");
                content.Add($"HPBRate={HalfPointByeRate}");
                content.Add($"HighestRating={HighestRating}");
                content.Add($"LowestRating={LowestRating}");
                content.Add($"Groups={Groups}");
                content.Add($"Separator={Separator}");
                if (ScoringScheme != ScoringScheme.Default)
                {
                    content.Add(FormattableString.Invariant($"WWPoints={ScoringScheme.PointsForWin:F1}"));
                    content.Add(FormattableString.Invariant($"BWPoints={ScoringScheme.PointsForWin:F1}"));
                    content.Add(FormattableString.Invariant($"WDPoints={ScoringScheme.PointsForDraw:F1}"));
                    content.Add(FormattableString.Invariant($"BDPoints={ScoringScheme.PointsForDraw:F1}"));
                    content.Add(FormattableString.Invariant($"WLPoints={ScoringScheme.PointsForPlayedLoss:F1}"));
                    content.Add(FormattableString.Invariant($"BLPoints={ScoringScheme.PointsForPlayedLoss:F1}"));
                    content.Add(FormattableString.Invariant($"ZPBPoints={ScoringScheme.PointsForZeroPointBye:F1}"));
                    content.Add(FormattableString.Invariant($"HPBPoints={ScoringScheme.PointsForDraw:F1}"));
                    content.Add(FormattableString.Invariant($"FPBPoints={ScoringScheme.PointsForWin:F1}"));
                    content.Add(FormattableString.Invariant($"PABPoints={ScoringScheme.PointsForPairingAllocatedBye:F1}"));
                    content.Add(FormattableString.Invariant($"FWPoints={ScoringScheme.PointsForWin:F1}"));
                    content.Add(FormattableString.Invariant($"FLPoints={ScoringScheme.PointsForForfeitedLoss:F1}"));
                }

                return content;
            }
        }
    }
}

