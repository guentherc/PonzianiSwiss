using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace PonzianiSwissTest
{
    [TestClass]
    public class TestTournament
    {

        private static string? TRF_FIDE = null;
        private static string? TRF_LICHESS = null;
        private static string? TRF_GITHUB = null;

        public static void InitTRF()
        {
            HttpClient client = new();
            TRF_FIDE = client.GetStringAsync("http://ratings.fide.com/download/example1.txt").Result;
            TRF_LICHESS = client.GetStringAsync("https://lichess.org/swiss/j8rtJ5GL.trf").Result;
            TRF_GITHUB = client.GetStringAsync("https://raw.githubusercontent.com/erral/fidetournament/master/tests/test_demo.trf").Result;
        }

        [TestMethod]
        public void TestLoadTRF()
        {
            if (TRF_FIDE == null) InitTRF();
            if (TRF_FIDE != null)
            {
                Tournament ft = new();
                ft.LoadFromTRF(TRF_FIDE);
                Assert.IsNotNull(ft.Name);
            }
            if (TRF_LICHESS != null)
            {
                Tournament lt = new();
                lt.LoadFromTRF(TRF_LICHESS);
                Assert.IsNotNull(lt.Name);
            }
            if (TRF_GITHUB != null)
            {
                Tournament gt = new();
                gt.LoadFromTRF(TRF_GITHUB);
                Assert.IsNotNull(gt.Name);
            }
        }

        [TestMethod]
        public void TestScorecard()
        {
            if (TRF_FIDE == null) InitTRF();
            if (TRF_FIDE != null)
            {
                Tournament ft = new();
                ft.LoadFromTRF(TRF_FIDE);
                var scorecards = ft.GetScorecards();
                var list = scorecards.Values.OrderBy((e) => -e.Score()).ToList();
                Assert.IsNotNull(list);
            }

        }

        [TestMethod]
        public void TestScores()
        {
            if (TRF_FIDE == null) InitTRF();
            if (TRF_FIDE != null)
            {
                Tournament ft = new();
                ft.LoadFromTRF(TRF_FIDE);
                var scorecards = ft.GetScorecards();
                var list = scorecards.Values.OrderBy((e) => -e.Score()).ToList();
                Assert.AreEqual(6.5f, list[0].Score());
                Assert.AreEqual(33f, list[0].Buchholz());
                Assert.AreEqual(29.5f, list[0].BuchholzCut1());
                Assert.AreEqual(30f, list[0].SonnebornBergerScore());
                Assert.AreEqual(27f, list[0].CumulativeScore());
                ft.OrderByRank();
                Assert.AreEqual(6.5f, ft.Participants[0].Scorecard?.Score());
                Assert.AreEqual(33f, ft.Participants[0].Scorecard?.Buchholz());
                Assert.AreEqual(29.5f, ft.Participants[0].Scorecard?.BuchholzCut1());
                Assert.AreEqual(30f, ft.Participants[0].Scorecard?.SonnebornBergerScore());
                Assert.AreEqual(27f, ft.Participants[0].Scorecard?.CumulativeScore());
            }
        }

        [TestMethod]
        public void TestDraw()
        {
            if (TRF_FIDE == null) InitTRF();
            Tournament ft = new();
            Assert.IsNotNull(TRF_GITHUB);
            ft.LoadFromTRF(TRF_GITHUB);
            bool drawn = ft.DrawAsync(0).Result;
            Assert.IsTrue(drawn);
        }

        [TestMethod]
        public void TestGenerateTournament()
        {
            Tournament? tournament = PairingTool.GenerateAsync().Result;
            Assert.IsNotNull(tournament);
            string json = tournament.Serialize();
            Tournament? t2 = Extensions.Deserialize(json); ;
            Assert.IsNotNull(t2);
        }
    }

    [TestClass]
    public class TestDraw
    {
        [TestMethod]
        public void CheckDutchDefaultScorescheme()
        {
            for (int seed = 30; seed < 40; ++seed)
            {
                TestDrawSimulation(seed);
            }
        }

        [TestMethod]
        public void CheckDutchOtherScorescheme()
        {
            PairingTool.GeneratorConfig config = new()
            {
                ScoringScheme = new()
            };
            config.ScoringScheme.PointsForWin = 3;
            config.ScoringScheme.PointsForDraw = 1;
            for (int seed = 30; seed < 40; ++seed)
            {
                TestDrawSimulation(seed, config);
            }
        }

        [TestMethod]
        public void CheckBursteinDefaultScorescheme()
        {
            PairingTool.GeneratorConfig config = new()
            {
                PairingSystem = PairingSystem.Burstein
            };
            for (int seed = 30; seed < 40; ++seed)
            {
                TestDrawSimulation(seed, config);
            }
        }

        [TestMethod]
        public void CheckBursteinOtherScorescheme()
        {
            PairingTool.GeneratorConfig config = new()
            {
                PairingSystem = PairingSystem.Burstein,
                ScoringScheme = new()
            };
            config.ScoringScheme.PointsForWin = 3;
            config.ScoringScheme.PointsForPairingAllocatedBye = 3;
            config.ScoringScheme.PointsForDraw = 1;
            for (int seed = 30; seed < 40; ++seed)
            {
                TestDrawSimulation(seed, config);
            }
        }

        [TestMethod]
        public void TestDutchAccelerated()
        {
            for (int seed = 30; seed < 40; ++seed)
            {
                TestDrawSimulationAccelerated(seed);
            }
        }

        private static void TestDrawSimulationAccelerated(int seed, PairingTool.GeneratorConfig? config = null)
        {
            if (config == null) config = new();
            config.CountRounds = 11;
            string? trfFile0 = PairingTool.GenerateTRFAsync(seed, config).Result;
            PairingSystem pairingSystem = config == null ? PairingSystem.Dutch : config.PairingSystem;
            Assert.IsNotNull(trfFile0);
            if (trfFile0 == null) return;
            Console.WriteLine($"Testing {trfFile0}");
            Tournament tournament = new();
            tournament.LoadFromTRF(File.ReadAllText(trfFile0));
            tournament.BakuAcceleration = true;
            tournament.PairingSystem = pairingSystem;
            Console.WriteLine($"{tournament.Participants.Count} Participants {tournament.Rounds.Count} Rounds");
            Tournament testTournament = (Tournament)((ICloneable)tournament).Clone();
            Assert.IsNotNull(testTournament);
            testTournament.Rounds.Clear();
            Side? xxc = null;
            int indx = 0;
            while (!xxc.HasValue)
            {
                var entry = tournament.Participants[indx].Scorecard?.Entries[0];
                if (entry != null && entry.Round == 0 && entry.Opponent != Participant.BYE)
                {
                    xxc = entry.Side;
                }
                ++indx;
            }
            //Now let's draw one round after another and check drawing
            for (int round = 0; round < tournament.Rounds.Count; round++)
            {
                Console.WriteLine($"Drawing Round {round}");
                Dictionary<string, Result> byes = new();
                bool drawIsOk = testTournament.DrawAsync(round, xxc, byes).Result;
                bool ok = drawIsOk && TestTournamentTRF(testTournament);
                if (!ok)
                {
                    PrintGeneratedTRF(trfFile0);
                    Assert.Fail();
                }
                foreach (var p in testTournament.Rounds[round].Pairings)
                {
                    p.Result = Utils.Simulate(testTournament.Rating(p.White), testTournament.Rating(p.Black));
                }
            }
        }

        private static void TestDrawSimulation(int seed, PairingTool.GeneratorConfig? config = null)
        {
            string? trfFile0 = PairingTool.GenerateTRFAsync(seed, config).Result;
            PairingSystem pairingSystem = config == null ? PairingSystem.Dutch : config.PairingSystem;
            Assert.IsNotNull(trfFile0);
            if (trfFile0 == null) return;
            Console.WriteLine($"Testing {trfFile0}");
            Tournament tournament = new();
            tournament.LoadFromTRF(File.ReadAllText(trfFile0));
            tournament.PairingSystem = pairingSystem;
            Console.WriteLine($"{tournament.Participants.Count} Participants {tournament.Rounds.Count} Rounds");
            Tournament testTournament = (Tournament)((ICloneable)tournament).Clone();
            Assert.IsNotNull(testTournament);
            TestTournamentTRF(testTournament);
            testTournament.Rounds.Clear();
            Side? xxc = null;
            int indx = 0;
            while (!xxc.HasValue)
            {
                var entry = tournament.Participants.Find(p => p.ParticipantId == "1")?.Scorecard?.Entries[0];
                if (entry != null && entry.Round == 0 && entry.Opponent != Participant.BYE)
                {
                    xxc = entry.Side;
                }
                ++indx;
            }
            //Now let's draw one round after another and check drawing
            for (int round = 0; round < tournament.Rounds.Count; round++)
            {
                Console.WriteLine($"Drawing Round {round}");
                Dictionary<string, Result> byes = new();
                foreach (var p in tournament.Rounds[round].Pairings)
                {
                    if (p.White?.ParticipantId != null && (p.Result == Result.ZeroPointBye || p.Result == Result.HalfPointBye || p.Result == Result.FullPointBye))
                        byes.Add(p.White.ParticipantId, p.Result);
                }
                bool drawIsOk = testTournament.DrawAsync(round, xxc, byes).Result;
                bool ok = drawIsOk && TestTournamentTRF(testTournament);
                if (!ok)
                {
                    PrintGeneratedTRF(trfFile0);
                }
                Assert.IsTrue(ok);
                foreach (Pairing pExpected in tournament.Rounds[round].Pairings)
                {

                    Pairing? pActual = testTournament.Rounds[round].Pairings.Find(e => e.White?.ParticipantId == pExpected.White?.ParticipantId);
                    if (pActual != null && pActual.Black == Participant.BYE) pActual.Black = Participant.BYE;
                    if (pActual == null || pExpected.Black?.ParticipantId != pActual.Black?.ParticipantId)
                    {
                        Console.WriteLine($"Issue with {pExpected.White?.ParticipantId} - {pExpected.Black?.ParticipantId} Round {round}");
                        PrintGeneratedTRF(trfFile0);
                    }
                    Assert.IsNotNull(pActual);
                    Assert.AreEqual(pExpected.Black?.ParticipantId, pActual.Black?.ParticipantId);
                    pActual.Result = pExpected.Result;
                }
            }
        }

        private static void PrintGeneratedTRF(string trfFile0)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("- Generated TRF                   -");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine(File.ReadAllText(trfFile0));
            Console.WriteLine();
        }

        private static bool TestTournamentTRF(Tournament tournament)
        {
            var trf = tournament.CreateTRF(tournament.Rounds.Count);
            string tmpFile = Path.GetTempFileName();
            File.WriteAllLines(tmpFile, trf);
            string[] checkResult = PairingTool.CheckAsync(tmpFile, tournament.PairingSystem).Result.Split(new String[] { "\r\n", "\n", "\r" }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            string fname = Path.GetFileNameWithoutExtension(tmpFile);
            foreach (string line in checkResult)
            {
                if (!line.StartsWith(fname))
                {
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("- Invalid TRF                     -");
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine(File.ReadAllText(tmpFile));
                    Console.WriteLine();
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("- Check Result                    -");
                    Console.WriteLine("-----------------------------------");
                    foreach (string l in checkResult) Console.WriteLine(l);
                    Console.WriteLine();
                    return false;
                }
            }
            return true;
        }
    }

}