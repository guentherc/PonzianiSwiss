using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace PonzianiSwissLib
{
    public static class Extensions
    {
        public async static Task LoadFromTRFAsync(this Tournament tournament, string file)
        {
            string content = await File.ReadAllTextAsync(file);
            tournament.LoadFromTRF(content);
        }

        public static void LoadFromTRF(this Tournament tournament, string content)
        {
            string[] lines = content.Split(new String[] { "\r\n", "\n", "\r" }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            List<string> plist = new();
            bool bbuAvailable = false;
            foreach (string line in lines)
            {
                if (line.Trim().Length < 4 || line[3] != ' ') continue;
                if (!int.TryParse(line.AsSpan(0, 3), out int code))
                {
                    if (line[..2] == "BB")
                    {
                        char c = line[2];
                        float value = float.Parse(line[3..].Trim(), CultureInfo.InvariantCulture);
                        switch (c)
                        {
                            case 'W':
                                tournament.ScoringScheme.PointsForWin = value;
                                if (!bbuAvailable)
                                    tournament.ScoringScheme.PointsForPairingAllocatedBye = value;
                                break;
                            case 'D': tournament.ScoringScheme.PointsForDraw = value; break;
                            case 'L': tournament.ScoringScheme.PointsForPlayedLoss = value; break;
                            case 'Z': tournament.ScoringScheme.PointsForZeroPointBye = value; break;
                            case 'F': tournament.ScoringScheme.PointsForForfeitedLoss = value; break;
                            case 'U':
                                tournament.ScoringScheme.PointsForPairingAllocatedBye = value;
                                bbuAvailable = true;
                                break;
                        }

                    }
                    continue;
                }

                if (code == 1)
                {
                    plist.Add(line);
                }
                else
                {
                    switch (code)
                    {
                        case 12:
                            tournament.Name = line[4..].Trim(); break;
                        case 22:
                            tournament.City = line[4..].Trim(); break;
                        case 32:
                            tournament.Federation = line[4..].Trim(); break;
                        case 42:
                            tournament.StartDate = line[4..].Trim(); break;
                        case 52:
                            tournament.EndDate = line[4..].Trim(); break;
                        case 62:
                            tournament.CountPlayer = int.Parse(line[4..].Trim()); break;
                        case 72:
                            tournament.CountRatedPlayer = int.Parse(line[4..].Trim()); break;
                        case 92:
                            tournament.Type = line[4..].Trim(); break;
                        case 102:
                            tournament.ChiefArbiter = line[4..].Trim(); break;
                        case 112:
                            tournament.DeputyChiefArbiter = line[4..].Trim(); break;
                        case 122:
                            tournament.TimeControl = line[4..].Trim(); break;
                    }
                }
            }
            tournament.ProcessPlayerTRF(plist);
            tournament.CountRounds = tournament.Rounds.Count;
            tournament.OrderByRank();
        }

        private static readonly string[] ShortTitles = { "g", "wg", "m", "wm", "f", "wf", "c", "wc", string.Empty };
        private static void ProcessPlayerTRF(this Tournament tournament, List<string> playerList)
        {

            foreach (string line in playerList)
            {
                string pid = line.Substring(4, 4).Trim();
                string name = line.Substring(14, 33);
                _ = int.TryParse(line.AsSpan(48, 4), out int rating);
                string title = line.Substring(10, 3).Trim();
                string birthdate = line.Substring(69, 10).Trim();
                FideTitle ftitle = FideTitle.NONE;
                if (title.Trim().Length > 0 && !Enum.TryParse<FideTitle>(title, out ftitle))
                {
                    int indx = Array.IndexOf(ShortTitles, title);
                    if (indx >= 0) ftitle = (FideTitle)indx;
                }
                Participant p = new(name, rating, ftitle, pid);
                if (birthdate.Length == 10)
                {
                    p.Attributes.Add(Participant.AttributeKey.Birthdate, new DateTime(int.Parse(birthdate[..4]), int.Parse(birthdate.Substring(5, 2)), int.Parse(birthdate.Substring(8, 2))));
                }
                p.Federation = line.Substring(53, 3).Trim();
                string fideId = line.Substring(57, 11).Trim();
                if (fideId.Length > 0) p.FideId = ulong.Parse(fideId);
                char c = line[9];
                if (c == 'f' || ((int)p.Title & 1) == 1) p.Sex = Sex.Female;
                else if (c == 'm') p.Sex = Sex.Male;
                tournament.Participants.Add(p);
            }
            //Parse results
            Dictionary<Participant, List<PEntry>> plist = new();
            for (int i = 0; i < playerList.Count; ++i)
            {
                List<PEntry> pentry_list = new();
                plist.Add(tournament.Participants[i], pentry_list);
                string line = playerList[i].Trim();
                int indx = 91;
                int round = 0;
                while (indx < line.Length)
                {
                    if (tournament.Rounds.Count <= round) tournament.Rounds.Add(new(round));
                    string id = line.Substring(indx, 4).Trim();
                    if (id.Length > 0)
                    {
                        char color = line[indx + 5];
                        char result = line[indx + 7];
                        int opponentId = int.Parse(id);
                        Participant opponent = opponentId == 0 ? Participant.BYE : tournament.Participants[opponentId - 1];
                        pentry_list.Add(new(round, opponent, color == 'b' ? Side.Black : Side.White, (Result)Tournament.result_char.IndexOf(result)));
                    }
                    indx += 10;
                    ++round;
                }
            }
            foreach (var entry in plist)
            {
                foreach (var item in entry.Value.Where(p => p.Side == Side.White))
                {
                    Pairing pairing = new(entry.Key, item.Opponent);
                    pairing.Result = item.Result;
                    if (pairing.Result == Result.Forfeited)
                    {
                        //Check if double forfeit
                        if (plist.ContainsKey(item.Opponent))
                        {
                            var olist = plist[item.Opponent];
                            PEntry? oitem = olist.Find(o => o.RoundIndex == item.RoundIndex);
                            Debug.Assert(oitem != null && oitem.Opponent == entry.Key);
                            if (oitem != null && oitem.Result == Result.Forfeited) pairing.Result = Result.DoubleForfeit;
                        }
                    }
                    tournament.Rounds[item.RoundIndex].Pairings.Add(pairing);
                }
            }
            tournament.GetScorecards();
            //Sort Pairings
            for (int i = 0; i < tournament.Rounds.Count; i++)
            {
                tournament.Rounds[i].Pairings.SortByJointScore(i);
            }
        }

        internal class PEntry
        {
            public PEntry(int roundIndex, Participant opponent, Side side, Result result)
            {
                RoundIndex = roundIndex;
                Opponent = opponent;
                Side = side;
                Result = result;
            }

            public int RoundIndex { get; set; }
            public Participant Opponent { get; set; }
            public Side Side { set; get; }
            public Result Result { set; get; }

        }

        internal static int SortId(this Participant p)
        {
            return p == Participant.BYE ? int.MaxValue : int.Parse(p.ParticipantId ?? "9999");
        }

        public static void SortByJointScore(this List<Pairing> pairings, int roundIndex)
        {
            pairings.Sort((p1, p2) =>
            {
                float score1 = p1?.White?.Scorecard?.Score(roundIndex) ?? 0 + p1?.Black?.Scorecard?.Score(roundIndex) ?? 0;
                float score2 = p2?.White?.Scorecard?.Score(roundIndex) ?? 0 + p2?.Black?.Scorecard?.Score(roundIndex) ?? 0;
                if (score1 != score2) return score2.CompareTo(score1);
                return Math.Min(p1?.White?.SortId() ?? 9999, p1?.Black?.SortId() ?? 9999).CompareTo(
                    Math.Min(p2?.White?.SortId() ?? 9999, p2?.Black?.SortId() ?? 9999));
            });
        }

        public static T Deserialize<T>(string json)
        {
            throw new NotImplementedException();
        }

        public static string Serialize(this Tournament tournament)
        {
            JsonSerializerOptions options = new()
            {
#if DEBUG
                WriteIndented = true
#else
                WriteIndented = false
#endif
            };
            return JsonSerializer.Serialize(tournament, options);
        }

        public static Tournament? Deserialize(string json)
        {
            return JsonSerializer.Deserialize<Tournament>(json);
        }

        public static async Task<bool> TestTRFGeneration()
        {
            string? trfFile0 = await PairingTool.GenerateTRFAsync();
            if (trfFile0 == null) return false;
            Tournament tournament = new();
            tournament.LoadFromTRF(File.ReadAllText(trfFile0));
            List<string> lines = tournament.CreateTRF(tournament.Rounds.Count);
            string oFile = Path.GetTempFileName();
            string trfFile1 = Path.ChangeExtension(oFile, "trf");
            await File.WriteAllLinesAsync(trfFile1, lines);
            string[] clines = (await PairingTool.CheckAsync(trfFile1, tournament.PairingSystem)).Split(new String[] { "\r\n", "\n", "\r" }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (clines == null || clines.Length == 0) return false;
            string fname = Path.GetFileNameWithoutExtension(trfFile1);
            foreach (string line in clines)
            {
                if (!line.StartsWith(fname))
                {
                    oFile = Path.GetTempFileName();
                    string trfFile2 = Path.ChangeExtension(oFile, "txt");
                    await File.WriteAllLinesAsync(trfFile2, clines);
                    Process.Start("notepad.exe", trfFile0);
                    Process.Start("notepad.exe", trfFile1);
                    Process.Start("notepad.exe", trfFile2);
                    return false;
                }
            }
            return true;
        }

    }

    public static class Utils
    {
        private static Random? rnd;

        public static void Seed(int seed)
        {
            rnd = new Random(seed);
        }

        public static Result Simulate(int eloWhite, int eloBlack, double drawRate = 0.6, double forfeitRate = 0.02)
        {
            if (rnd == null) rnd = new Random();
            if (rnd.NextDouble() < forfeitRate)
            {
                return rnd.NextDouble() < 0.5 ? Result.ForfeitWin : Result.Forfeited;
            }
            double winExp = WinProbability(eloWhite, eloBlack);
            // Formulas:
            // Pw (win expectation of stronger side)
            // Pb (win expectation of weaker side)
            // Pd + Pw + Pb = 1
            // Pw + Pd/2 = winExp
            // => Pd = 1 - Pw - Pb
            // => 1 + Pw - Pb = 2 * winExp
            // => Pw = 2* WinExp + Pb - 1
            // minimal Pw (Pb = 0) = 2* WinExp - 1 (if WinExp > 0,5) else 0
            // maximal Pw (no draws) Pw = WinExp
            // draw rate min = 0
            // draw rate max Pd = 1 - Pw - Pb = 2 - 2 * WinExp = 2 * (1 - WinExp)
            bool blackIsStronger = winExp < .5;
            double pWin = blackIsStronger ? 1 - winExp : winExp;
            double pDraw = 2 * drawRate * (1 - pWin);
            double pStronger = pWin - pDraw / 2;
            double pWeaker = (1 - pWin) - pDraw / 2;
            Debug.Assert(pStronger >= 0 && pWeaker >= 0 && pStronger <= 1 && pWeaker <= 1 && pDraw <= 1);
            Debug.Assert(Math.Abs(1 - pStronger - pWeaker - pDraw) <= 0.00001);
            double r = rnd.NextDouble();
            if (r < pDraw) return Result.Draw;
            else if (r < pDraw + pStronger) return blackIsStronger ? Result.Loss : Result.Win;
            else return blackIsStronger ? Result.Win : Result.Loss;
        }

        public static double WinProbability(double eloWhite, double eloBlack, double bonusWhite = 20)
        {
            double ratingWhite = eloWhite + bonusWhite;
            double ratingBlack = eloBlack - bonusWhite;
            double ratingDiff = ratingWhite - ratingBlack;
            return 1 / (1 + Math.Pow(10, -ratingDiff / 400));
        }

        public static double Erf(double x)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }

        public static double Erfc(double x) => 1 - Erf(x);
    }
}
