using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiSwissLib;
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
            Tournament tournament = PairingTool.GenerateAsync().Result;
            Assert.IsNotNull(tournament);
        }
    }
}