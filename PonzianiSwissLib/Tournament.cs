using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiSwissLib
{
    public enum FideTitle { GM, WGM, IM, WIM, FM, WFM, CM, WCM, WH, NONE };

    public enum Result { Open = -2, Forfeited = -1, Loss = 0, Draw = 1, Win = 2, ForfeitWin = 3 }

    public enum Side { White, Black }

    public enum Sex { Male, Female }

    public class Tournament
    {

        public List<Participant> Participants { set; get; } = new List<Participant>();

        public List<Round> Rounds { set; get; } = new List<Round>();

        public int CountRounds { set; get; }

        public string? Name { set; get; }

        public string? City { set; get; }

        public string? Federation { set; get; }

        public string? StartDate { set; get; }

        public string? EndDate { set; get; }

        public string? Type { set; get; }

        public int CountPlayer { set; get; }

        public int CountRatedPlayer { set; get; }

        public string? ChiefArbiter { set; get; }

        public string? DeputyChiefArbiter { set; get; }

        public string? TimeControl { set; get; }

        public Dictionary<Participant, Scorecard> GetScorecards(int round = int.MaxValue)
        {
            round = Math.Min(round, Rounds.Count - 1);
            Dictionary<Participant, Scorecard> scorecards = new();
            for (int r = 0; r <= round; ++r)
            {
                foreach (Pairing pairing in Rounds[r].Pairings)
                {
                    if (!scorecards.ContainsKey(pairing.White)) scorecards.Add(pairing.White, new(pairing.White));
                    if (!scorecards.ContainsKey(pairing.Black)) scorecards.Add(pairing.Black, new(pairing.Black));
                    scorecards[pairing.White].Entries.Add(new(pairing.Black, Side.White, pairing.Result, r));
                    scorecards[pairing.Black].Entries.Add(new(pairing.White, Side.Black, pairing.InvertedResult, r));
                }
            }
            foreach (var p in scorecards.Keys) p.Attributes[Participant.AttributeKey.Scorecard] = scorecards[p];
            Participants.RemoveAll(p => !p.Attributes.ContainsKey(Participant.AttributeKey.Scorecard));
            return scorecards;
        }

        public Scorecard GetParticipantScorecard(Participant p)
        {
            var scorecards = GetScorecards();
            return scorecards.Keys.Where(x => x.ParticipantId == p.ParticipantId).Select(x => (Scorecard)x.Attributes[Participant.AttributeKey.Scorecard]).First();
        }

        public void OrderByRank(int round = int.MaxValue)
        {
            round = Math.Min(round, Rounds.Count);
            GetScorecards();
            Participants.Sort((p2, p1) =>
            {
                if (round == 0) return p1.TournamentRating.CompareTo(p2.TournamentRating);
                else
                {
                    Scorecard sc1 = (Scorecard)p1.Attributes[Participant.AttributeKey.Scorecard];
                    Scorecard sc2 = (Scorecard)p2.Attributes[Participant.AttributeKey.Scorecard];
                    float score1 = sc1.Score(round);
                    float score2 = sc2.Score(round);
                    if (score1 != score2) return score1.CompareTo(score2);
                    score1 = sc1.BuchholzCut1(round);
                    score2 = sc2.BuchholzCut1(round);
                    if (score1 != score2) return score1.CompareTo(score2);
                    score1 = sc1.Buchholz(round);
                    score2 = sc2.Buchholz(round);
                    if (score1 != score2) return score1.CompareTo(score2);
                    int cwin1 = sc1.CountWin(round);
                    int cwin2 = sc2.CountWin(round);
                    if (cwin1 != cwin2) return cwin1.CompareTo(cwin2);
                    cwin1 = sc1.CountBlackWin(round);
                    cwin2 = sc2.CountBlackWin(round);
                    return cwin1.CompareTo(cwin2);
                }
            });
        }

        public void AssignTournamentIds(int round = int.MaxValue)
        {
            OrderByRank(round);
            for (int i = 0; i < Participants.Count; i++)
            {
                Participants[i].ParticipantId = $"{i + 1}";
            }
        }

        /// <summary>
        /// Create a TRF report <see href="https://www.fide.com/FIDE/handbook/C04Annex2_TRF16.pdf"/> for the tournament
        /// <para>TRF reports are used as input for the <see cref="PairingTool"/></para>
        /// </summary>
        /// <param name="round">Index of the round for which the report shall be created (0 means before 1. round, before 2., ...)</param>
        /// <returns>TRF report</returns>
        public List<string> CreateTRF(int round)
        {
            AssignTournamentIds(round);
            List<string> trf = new();
            trf.Add($"012 {Name}");
            trf.Add($"022 {City}");
            trf.Add($"032 {Federation}");
            trf.Add($"102 {ChiefArbiter}");
            trf.Add($"XXR {CountRounds}");
            var random = new Random();
            if (random.Next(2) == 1) trf.Add($"XXC white1"); else trf.Add($"XXC black1");
            foreach (Participant p in Participants)
            {
                char s = p.Attributes.ContainsKey(Participant.AttributeKey.Sex) && (Sex)p.Attributes[Participant.AttributeKey.Sex] == Sex.Female ? 'f' : 'm';
                string birthdate = p.Attributes.ContainsKey(Participant.AttributeKey.Birthdate) ? ((DateTime)p.Attributes[Participant.AttributeKey.Birthdate]).ToString("yyyy/MM/dd")
                                  : p.Attributes.ContainsKey(Participant.AttributeKey.Birthyear) ? ((DateTime)p.Attributes[Participant.AttributeKey.Birthyear]).ToString() : string.Empty;
                StringBuilder pline = new($"001 {p.ParticipantId,4} {s} {title_string[(int)p.Title],2} {p.Name,-33} {p.FideRating,4} {p.Federation,3 } {p.FideId,11} {birthdate,-10} { p.Scorecard?.Score(round),4} { p.ParticipantId,4}");
                for (int r = 0; r < round; ++r)
                {
                    var entry = p.Scorecard?.Entries.Where(e => e.Round == r - 1).First();
                    if (entry != null)
                        pline.Append($"  {entry?.Opponent.ParticipantId,4} {"wb"[(int)(entry?.Side)]} {result_char[(int)entry.Result - 2]} ");
                }
                trf.Add(pline.ToString());
            }
            return trf;
        }


        private static readonly PairingTool pairingTool = new();

        public async Task<bool> DrawAsync(int round = int.MaxValue)
        {
            var trf = CreateTRF(round);
            var file = Path.GetTempFileName();
            await File.WriteAllLinesAsync(file, trf, Encoding.UTF8);
            string pairings = pairingTool.Pair(file);
            string[] lines = pairings.Split('\n');
            for (int i = Rounds.Count; i < round; ++i) Rounds.Add(new Round(i));
            Rounds[round].Pairings.Clear();
            for (int i = round + 1; i < Rounds.Count; ++i) Rounds[i].Pairings.Clear();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] n = lines[i].Trim().Split(' ');
                if (n.Length != 2) continue;
                Participant p1 = Participants.Where(p => p.ParticipantId == n[0]).First();
                Participant p2 = n[1] != "0" ? Participants.Where(p => p.ParticipantId == n[1]).First() : Participant.BYE;
                Rounds[round].Pairings.Add(new(p1, p2));
            }
            return true;
        }

        private static readonly string[] title_string = { "g", "wg", "m", "wm", "f", "wf", "c", "wc", "h", "" };
        private static readonly string result_char = "*-0=1+";
    }
}
