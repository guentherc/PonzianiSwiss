using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiSwissTest
{
    [TestClass]
    public class TestUtils
    {
        [TestMethod]
        public void TestWinExpectatiom()
        {
            double r1 = 2400.0;
            double r2 = 2200.0;
            double winExp = Utils.WinProbability(r1, r2);
            Assert.IsTrue(winExp > 0.5 && winExp < 1);
            double rwinExp = 1 - Utils.WinProbability(r2, r1);
            Assert.IsTrue(rwinExp < winExp);
            r1 += 50;
            double iwinExp = Utils.WinProbability(r1, r2);
            Assert.IsTrue(iwinExp > winExp);
            r1 -= 100;
            iwinExp = Utils.WinProbability(r1, r2);
            Assert.IsTrue(iwinExp < winExp);
        }

        [TestMethod]
        public void TestSimulate()
        {
            Utils.Seed(42);
            int elo1 = 2800;
            int elo2 = 2700;
            double winExpectation = Utils.WinProbability(elo1, elo2, 0);
            int MATCH_COUNT = 1000000;
            double points = 0;
            for (int i = 0; i < MATCH_COUNT; ++i)
            {
                Result r1 = Utils.Simulate(elo1, elo2, 0.6, 0);
                Result r2 = Utils.Simulate(elo2, elo1, 0.6, 0);
                points += ResultToPoints(r1);
                points += (1 - ResultToPoints(r2));
            }
            double winExpExp = points / (2 * MATCH_COUNT);
            Assert.IsTrue(Math.Abs(winExpectation - winExpExp) < 0.003);
        }

        private static double ResultToPoints(Result r)
        {
            switch (r)
            {
                case Result.Win: case Result.ForfeitWin: return 1.0;
                case Result.Loss: case Result.Forfeited: return 0;
                case Result.Draw: return 0.5;
            }
            Assert.Fail();
            return 0;
        }

        [TestMethod]
        public void TestErf()
        {
            var sb = new System.Text.StringBuilder(1120);
            sb.AppendLine(@"0;0;1");
            sb.AppendLine(@"0.05;0.056372;0.943628");
            sb.AppendLine(@"0.1;0.1124629;0.8875371");
            sb.AppendLine(@"0.15;0.167996;0.832004");
            sb.AppendLine(@"0.2;0.2227026;0.7772974");
            sb.AppendLine(@"0.25;0.2763264;0.7236736");
            sb.AppendLine(@"0.3;0.3286268;0.6713732");
            sb.AppendLine(@"0.35;0.3793821;0.6206179");
            sb.AppendLine(@"0.4;0.4283924;0.5716076");
            sb.AppendLine(@"0.45;0.4754817;0.5245183");
            sb.AppendLine(@"0.5;0.5204999;0.4795001");
            sb.AppendLine(@"0.55;0.5633234;0.4366766");
            sb.AppendLine(@"0.6;0.6038561;0.3961439");
            sb.AppendLine(@"0.65;0.6420293;0.3579707");
            sb.AppendLine(@"0.7;0.6778012;0.3221988");
            sb.AppendLine(@"0.75;0.7111556;0.2888444");
            sb.AppendLine(@"0.8;0.742101;0.257899");
            sb.AppendLine(@"0.85;0.7706681;0.2293319");
            sb.AppendLine(@"0.9;0.7969082;0.2030918");
            sb.AppendLine(@"0.95;0.8208908;0.1791092");
            sb.AppendLine(@"1;0.8427008;0.1572992");
            sb.AppendLine(@"1.1;0.8802051;0.1197949");
            sb.AppendLine(@"1.2;0.910314;0.089686");
            sb.AppendLine(@"1.3;0.9340079;0.0659921");
            sb.AppendLine(@"1.4;0.9522851;0.0477149");
            sb.AppendLine(@"1.5;0.9661051;0.0338949");
            sb.AppendLine(@"1.6;0.9763484;0.0236516");
            sb.AppendLine(@"1.7;0.9837905;0.0162095");
            sb.AppendLine(@"1.8;0.9890905;0.0109095");
            sb.AppendLine(@"1.9;0.9927904;0.0072096");
            sb.AppendLine(@"2;0.9953223;0.0046777");
            sb.AppendLine(@"2.1;0.9970205;0.0029795");
            sb.AppendLine(@"2.2;0.9981372;0.0018628");
            sb.AppendLine(@"2.3;0.9988568;0.0011432");
            sb.AppendLine(@"2.4;0.9993115;0.0006885");
            sb.AppendLine(@"2.5;0.999593;0.000407");
            sb.AppendLine(@"2.6;0.999764;0.000236");
            sb.AppendLine(@"2.7;0.9998657;0.0001343");
            sb.AppendLine(@"2.8;0.999925;0.000075");
            sb.AppendLine(@"2.9;0.9999589;0.0000411");
            sb.AppendLine(@"3;0.9999779;0.0000221");
            sb.AppendLine(@"3.1;0.9999884;0.0000116");
            sb.AppendLine(@"3.2;0.999994;0.000006");
            sb.AppendLine(@"3.3;0.9999969;0.0000031");
            sb.AppendLine(@"3.4;0.9999985;0.0000015");
            sb.AppendLine(@"3.5;0.9999993;0.0000007");

            string data = sb.ToString();
            string[] lines = data.Split(new String[] { "\r\n", "\n", "\r" }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split(";");
                Assert.AreEqual(3, parts.Length);
                double x = double.Parse(parts[0], CultureInfo.InvariantCulture);
                double erf = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double erfc = double.Parse(parts[2], CultureInfo.InvariantCulture);
                Assert.IsTrue(Math.Abs(Utils.Erf(x) - erf) < 0.000001);
                Assert.IsTrue(Math.Abs(Utils.Erfc(x) - erfc) < 0.000001);
            }
        }
    }
}
