using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PonzianiSwissLib
{
    /// <summary>
    /// Chess title 
    /// </summary>
    public enum FideTitle
    {
        /// <summary>
        /// Grandmaster
        /// </summary>
        GM,
        /// <summary>
        /// Woman Grandmaster
        /// </summary>
        WGM,
        /// <summary>
        /// International Master
        /// </summary>
        IM,
        /// <summary>
        /// Woman International Master
        /// </summary>
        WIM,
        /// <summary>
        /// Fide Master
        /// </summary>
        FM,
        /// <summary>
        /// Woman Fide Master
        /// </summary>
        WFM,
        /// <summary>
        /// Candidate master
        /// </summary>
        CM,
        /// <summary>
        /// Woman Candidate master
        /// </summary>
        WCM,
        /// <summary>
        /// Untitled
        /// </summary>
        NONE,
        /// <summary>
        /// Woman Honarary Grandmaster
        /// </summary>
        WH
    };

    /// <summary>
    /// Result of a single game within a swiss tournament
    /// </summary>
    public enum Result { Open, Forfeited, Loss, UnratedLoss, ZeroPointBye, PairingAllocatedBye, Draw, UnratedDraw, HalfPointBye, Win, UnratedWin, ForfeitWin, FullPointBye, DoubleForfeit }

    /// <summary>
    /// Color assigned to a player
    /// </summary>
    public enum Side { White, Black }

    public enum Sex { Male, Female }

    public enum PairingSystem { Dutch, Burstein }

    /// <summary>
    /// Swiss chess tournament
    /// </summary>
    public class Tournament : ICloneable
    {
        /// <summary>
        /// List of Paricipants of the tournament
        /// </summary>
        public List<Participant> Participants { set; get; } = new List<Participant>();

        /// <summary>
        /// Rounds already played resp. ongoing round
        /// </summary>
        public List<Round> Rounds { set; get; } = new List<Round>();

        /// <summary>
        /// Number of rounds to be played
        /// </summary>
        public int CountRounds { set; get; }

        /// <summary>
        /// Tournament name
        /// </summary>
        public string? Name { set; get; }

        /// <summary>
        /// City where the tournament takes place
        /// </summary>
        public string? City { set; get; }

        /// <summary>
        /// Federation hosting the tournament
        /// </summary>
        public string? Federation { set; get; }

        /// <summary>
        /// Tournament's start date
        /// </summary>
        public string? StartDate { set; get; }

        /// <summary>
        /// Tournament's end date
        /// </summary>
        public string? EndDate { set; get; }

        /// <summary>
        /// Tournament type (free text), only relevant for information purposes
        /// </summary>
        public string? Type { set; get; }

        /// <summary>
        /// Number of Prticipants
        /// </summary>
        public int CountPlayer { set; get; }

        /// <summary>
        /// Number of rated players
        /// </summary>
        public int CountRatedPlayer { set; get; }

        /// <summary>
        /// Chief Arbiter
        /// </summary>
        public string? ChiefArbiter { set; get; }

        /// <summary>
        /// Other Arbiter(s)
        /// </summary>
        public string? DeputyChiefArbiter { set; get; }

        /// <summary>
        /// Time Control (only for information purposes)
        /// </summary>
        public string? TimeControl { set; get; }

        /// <summary>
        /// Pairing system, which shall be used
        /// </summary>
        public PairingSystem PairingSystem { set; get; } = PairingSystem.Dutch;

        public ScoringScheme ScoringScheme { set; get; } = new();

        public bool BakuAcceleration { set; get; } = false;

        /// <summary>
        /// Calculates the scorecards for all participants
        /// </summary>
        /// <param name="round">The (0-based) round number up to which the scorecards will be calculated</param>
        /// <returns>A dictionary containing a scorecard for each player</returns>
        public Dictionary<Participant, Scorecard> GetScorecards(int round = int.MaxValue)
        {
            round = Math.Min(round, Rounds.Count - 1);
            Dictionary<Participant, Scorecard> scorecards = new();
            for (int r = 0; r <= round; ++r)
            {
                foreach (Pairing pairing in Rounds[r].Pairings)
                {
                    if (!scorecards.ContainsKey(pairing.White)) scorecards.Add(pairing.White, new(pairing.White, this));
                    if (!scorecards.ContainsKey(pairing.Black)) scorecards.Add(pairing.Black, new(pairing.Black, this));
                    scorecards[pairing.White].Entries.Add(new(pairing.Black, Side.White, pairing.Result, r, this));
                    scorecards[pairing.Black].Entries.Add(new(pairing.White, Side.Black, pairing.InvertedResult, r, this));
                }
            }
            foreach (var p in scorecards.Keys)
                p.Scorecard = scorecards[p];
            return scorecards;
        }

        /// <summary>
        /// Orders the list of participants
        /// <para>This provides at the same time the necessary input for the draw of round <paramref name="round"/> 
        /// as well as the standing after round <paramref name="round"/> -1"/></para>
        /// </summary>
        /// <param name="round">Round for which the standing is calculated (0-based: 0 is before first round, 1: after first round,..)</param>
        public void OrderByRank(int round = int.MaxValue)
        {
            round = Math.Min(round, Rounds.Count);
            GetScorecards();
            Participants.Sort((p2, p1) =>
            {
                if (round == 0) return p1.TournamentRating.CompareTo(p2.TournamentRating);
                else
                {
                    Scorecard? sc1 = p1.Scorecard;
                    Scorecard? sc2 = p2.Scorecard;
                    if (sc1 != null && sc2 != null)
                    {
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
                    else return 0;
                }
            });
        }


        /// <summary>
        /// Orders the list of participants
        /// <para>This provides at the same time the necessary input for the draw of round <paramref name="round"/> 
        /// as well as the standing after round <paramref name="round"/> -1"/></para>
        /// </summary>
        /// <param name="round">Round for which the standing is calculated (0-based: 0 is before first round, 1: after first round,..)</param>
        public void OrderByScoreAndInitialRank(int round = int.MaxValue)
        {
            round = Math.Min(round, Rounds.Count);
            if (round > 0) GetScorecards();
            Participants.Sort((p2, p1) =>
            {
                if (round == 0)
                {
                    if (p1.TournamentRating != p2.TournamentRating) return p1.TournamentRating.CompareTo(p2.TournamentRating);
                    else if (p1.ParticipantId != null && p2.ParticipantId != null) return p2.ParticipantId.CompareTo(p1.ParticipantId);
                    else return 0;
                }
                else
                {
                    float score1 = p1.Scorecard?.Score(round) ?? 0;
                    float score2 = p2.Scorecard?.Score(round) ?? 0;
                    if (score1 != score2) return score1.CompareTo(score2);
                    int sint1 = int.Parse(p1.ParticipantId ?? "10000");
                    int sint2 = int.Parse(p2.ParticipantId ?? "10000");
                    return sint2.CompareTo(sint1);
                }
            });
        }

        /// <summary>
        /// Assigns the participant ids according to rank
        /// </summary>
        /// <param name="round">Round for which the rank is calculated (0-based: 0 is before first round, 1: after first round,..)</param>
        public void AssignTournamentIds(int round = int.MaxValue)
        {
            OrderByRank(round);
            for (int i = 0; i < Participants.Count; i++)
            {
                Participants[i].ParticipantId = $"{i + 1}";
                Participants[i].Rank = i + 1;
            }
        }

        /// <summary>
        /// Assigns the participant's rank
        /// </summary>
        /// <param name="round">Round for which the rank is calculated (0-based: 0 is before first round, 1: after first round,..)</param>
        public void AssignRank(int round = int.MaxValue)
        {
            OrderByScoreAndInitialRank(round);
            for (int i = 0; i < Participants.Count; i++)
            {
                Participants[i].Rank = i + 1;
            }
            Participants.Sort((p1, p2) =>
            {
                return int.Parse(p1.ParticipantId ?? "0").CompareTo(int.Parse(p2.ParticipantId ?? "0"));
            });
        }

        /// <summary>
        /// Create a TRF report <see href="https://www.fide.com/FIDE/handbook/C04Annex2_TRF16.pdf"/> for the tournament
        /// <para>TRF reports are used as input for the <see cref="PairingTool"/></para>
        /// </summary>
        /// <param name="round">Index of the round for which the report shall be created (0 means before 1. round, 1 means before 2., ...)</param>
        /// <param name="xxc">Side, which top ranked played should have in round 1 (if null use random value)</param>
        /// <returns>TRF report</returns>
        public List<string> CreateTRF(int round, Side? xxc = null, Dictionary<string, Result>? byes = null)
        {
            round = Math.Min(round, Rounds.Count);
            if (round == 0 && Participants.Any(p => p.ParticipantId == null))
                AssignTournamentIds(round);
            AssignRank(round);
            List<string> trf = new();
            trf.Add($"012 {Name}");
            trf.Add($"022 {City}");
            trf.Add($"032 {Federation}");
            trf.Add($"102 {ChiefArbiter}");
            trf.Add($"XXR {CountRounds}");
            trf.AddRange(ScoringScheme.TRFStrings());
            if (Rounds.Count == 0)
            {
                if (xxc.HasValue)
                {
                    if (xxc.Value == Side.White) trf.Add($"XXC white1"); else trf.Add($"XXC black1");
                }
                else
                {
                    var random = new Random();
                    if (random.Next(2) == 1) trf.Add($"XXC white1"); else trf.Add($"XXC black1");
                }
            }
            else
            {
                for (int i = 1; i <= Participants.Count; i++)
                {
                    string pid = i.ToString();
                    if (byes != null && byes.ContainsKey(pid)) continue;
                    var p = Rounds[0].Pairings.Where(p => p.White.ParticipantId == pid || p.Black.ParticipantId == pid);
                    if (p.Any())
                    {
                        Pairing pp = p.ToList()[0];
                        if (pp.White.ParticipantId == pid) trf.Add($"XXC white1"); else trf.Add($"XXC black1");
                        break;
                    }
                }
            }
            foreach (Participant p in Participants)
            {
                if (p == Participant.BYE || p.ParticipantId == Participant.BYE.ParticipantId) continue;
                char s = p.Attributes.ContainsKey(Participant.AttributeKey.Sex) && (Sex)p.Attributes[Participant.AttributeKey.Sex] == Sex.Female ? 'f' : 'm';
                string birthdate = p.Attributes.ContainsKey(Participant.AttributeKey.Birthdate) ? ((DateTime)p.Attributes[Participant.AttributeKey.Birthdate]).ToString("yyyy/MM/dd")
                                  : p.Attributes.ContainsKey(Participant.AttributeKey.Birthyear) ? ((int)p.Attributes[Participant.AttributeKey.Birthyear]).ToString() : string.Empty;
                StringBuilder pline = new(FormattableString.Invariant($"001 {p.ParticipantId,4} {s} {title_string[(int)p.Title],2} {p.Name,-33} {p.FideRating,4} {p.Federation,3 } {(p.FideId != 0 ? p.FideId : string.Empty),11} {birthdate,-10} { p.Scorecard?.Score(round) ?? 0,4} { p.Rank,4}"));
                for (int r = 1; r <= round; ++r)
                {
                    var entries = p.Scorecard?.Entries.Where(e => e.Round == r - 1);
                    if (entries != null && entries.Any())
                    {
                        var entry = entries.First();
                        if (entry != null)
                        {
                            if (entry?.Opponent.ParticipantId == "0000")
                                pline.Append($"  0000 - {result_char[(int)(entry?.Result ?? Result.Open)]}");
                            else
                                pline.Append($"  {entry?.Opponent.ParticipantId,4} {"wb"[(int)(entry?.Side ?? Side.White)]} {result_char[(int)(entry?.Result ?? Result.Open)]}");
                        }
                    }
                    else pline.Append("          ");
                }
                if (byes != null && p.ParticipantId != null && byes.ContainsKey(p.ParticipantId))
                    pline.Append($"  0000 - {result_char[(int)byes[p.ParticipantId]]}");
                trf.Add(pline.ToString().Trim());
            }
            if (BakuAcceleration)
            {
                int G1 = 2 * (int)Math.Ceiling(Participants.Count / 4.0);
                int CountAccRounds = (CountRounds + 1) / 2;
                int CountFullAccRounds = (CountAccRounds + 1) / 2;
                for (int i = 0; i<G1; ++i)
                {
                    string pline = $"XXA {Participants[i].ParticipantId,4}";
                    for (int j = 0; j < Math.Min(Rounds.Count + 1, CountFullAccRounds); j++)
                    {
                        pline += "  1.0";
                    }
                    for (int j = CountFullAccRounds; j < Math.Min(Rounds.Count + 1, CountAccRounds); j++)
                    {
                        pline += "  0.5";
                    }
                    trf.Add(pline);
                }
            }
            return trf;
        }

        /// <summary>
        /// Executes the pairing of the next rouns
        /// </summary>
        /// <param name="round">0-based round index</param>
        /// <param name="SideForTopRanked">Side for top-ranked player in 1. round (if null, then random)</param>
        /// <returns>true, if successful</returns>
        public async Task<bool> DrawAsync(int round = int.MaxValue, Side? SideForTopRanked = null, Dictionary<string, Result>? byes = null)
        {
            var trf = CreateTRF(round, SideForTopRanked, byes);
            var file = Path.GetTempFileName();
            await File.WriteAllLinesAsync(file, trf, Encoding.UTF8);
            string pairings = await PairingTool.PairAsync(file, PairingSystem);
            string[] lines = pairings.Split('\n');
            for (int i = Rounds.Count; i <= round; ++i) Rounds.Add(new Round(i));
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
            //Byes
            if (byes != null)
            {
                foreach (var entry in byes)
                {
                    Participant? p = Participants.Find(p => p.ParticipantId == entry.Key);
                    if (p != null)
                    {
                        Pairing pairing = new(p, Participant.BYE);
                        pairing.Result = entry.Value;
                        Rounds[round].Pairings.Add(pairing);
                    }
                }
            }
            Rounds[round].Pairings.SortByJointScore(round);
            return true;
        }

        object ICloneable.Clone()
        {
            string json = this.Serialize();
            var t = Extensions.Deserialize(json) ?? new Tournament();
            //Adjust references
            Dictionary<string, Participant> participants = new();
            var byes = t.Participants.Where(p => p.ParticipantId == "0000");
            if (byes.Any())
            {
                t.Participants.RemoveAll(p => p.ParticipantId == "0000");
            }
            t.Participants.Add(Participant.BYE);
            foreach (var p in t.Participants)
            {
                string? id = p.ParticipantId ?? p.FideId.ToString();
                if (id != null) participants.Add(id, p);
            }
            foreach (Round r in t.Rounds)
            {
                foreach (Pairing pairing in r.Pairings)
                {
                    string? idwhite = pairing.White.ParticipantId ?? pairing.White.FideId.ToString();
                    if (idwhite != null)
                    {
                        if (participants.ContainsKey(idwhite)) pairing.White = participants[idwhite];
                        else
                        {
                            participants.Add(idwhite, pairing.White);
                            t.Participants.Add(pairing.White);
                        }
                    }
                    string? idblack = pairing.Black.ParticipantId ?? pairing.Black.FideId.ToString();
                    if (idblack != null)
                    {
                        if (participants.ContainsKey(idblack)) pairing.Black = participants[idblack];
                        else
                        {
                            participants.Add(idblack, pairing.Black);
                            t.Participants.Add(pairing.Black);
                        }
                    }
                }
            }
            t.Participants.Remove(Participant.BYE);
            return t;
        }

        internal static readonly string[] title_string = { "g", "wg", "m", "wm", "f", "wf", "c", "wc", "", "h" };
        internal static readonly string result_char = "*-0LZU=DH1W+F-";

    }
}
