namespace PonzianiSwissLib
{

    public class Pairing
    {
        public Pairing(Participant white, Participant black)
        {
            White = white ?? throw new ArgumentNullException(nameof(white));
            Black = black ?? throw new ArgumentNullException(nameof(black));
        }

        public Participant White { set; get; }

        public Participant Black { set; get; }

        public Result Result { set; get; } = Result.Open;

        /// <summary>
        /// Result from Black POV
        /// </summary>
        internal Result InvertedResult
        {
            get
            {
                return Result switch
                {
                    Result.Loss => Result.Win,
                    Result.Win => Result.Loss,
                    Result.Forfeited => Result.ForfeitWin,
                    Result.ForfeitWin => Result.Forfeited,
                    Result.UnratedLoss => Result.UnratedWin,
                    Result.UnratedWin => Result.UnratedLoss,
                    _ => Result,
                };
            }
        }
    }

    public class Round
    {
        public Round(int number)
        {
            Number = number;
        }

        public int Number { set; get; }

        public List<Pairing> Pairings { set; get; } = new List<Pairing>();
    }

    public class Scorecard
    {
        public Participant Participant { set; get; }

        public Tournament Tournament { set; get; }

        public Scorecard(Participant participant, Tournament tournament)
        {
            Participant = participant;
            Tournament = tournament;
        }

        public List<Entry> Entries { set; get; } = new List<Entry>();

        public float Score() => Entries.Sum((e) => e.Score);
        public float Score(int round) => Entries.Where(e => e.Round < round).Sum((e) => e.Score);

        public float Buchholz() => Entries.Select(e => e.Opponent.Scorecard?.Score() ?? 0).Sum();
        public float Buchholz(int round) => Entries.Where(e => e.Round < round).Select(e => e.Opponent.Scorecard?.Score(round) ?? 0).Sum();

        public float BuchholzMedian()
        {
            var list = Entries.Select(e => e.Opponent.Scorecard?.Score() ?? 0).ToList();
            list.Sort();
            list.RemoveAt(0);
            list.RemoveAt(list.Count - 1);
            return list.Sum();
        }

        public float BuchholzMedian(int round)
        {
            var list = Entries.Where(e => e.Round < round).Select(e => e.Opponent.Scorecard?.Score(round) ?? 0).ToList();
            list.Sort();
            list.RemoveAt(0);
            list.RemoveAt(list.Count - 1);
            return list.Sum();
        }

        public float BuchholzCut1()
        {
            var list = Entries.Select(e => e.Opponent.Scorecard?.Score() ?? 0).ToList();
            list.Sort();
            list.RemoveAt(0);
            return list.Sum();
        }

        public float BuchholzCut1(int round)
        {
            var list = Entries.Where(e => e.Round < round).Select(e => e.Opponent.Scorecard?.Score(round) ?? 0).ToList();
            list.Sort();
            list.RemoveAt(0);
            return list.Sum();
        }

        public float RefinedBuchholz() => Entries.Select(e => e.Opponent.Scorecard?.Buchholz() ?? 0).Sum();
        public float RefinedBuchholz(int round) => Entries.Where(e => e.Round < round).Select(e => e.Opponent.Scorecard?.Buchholz(round) ?? 0).Sum();

        public float SonnebornBergerScore() => Entries.Select(e => e.Score * (e.Opponent.Scorecard?.Score() ?? 0)).Sum();
        public float SonnebornBergerScore(int round) => Entries.Where(e => e.Round < round).Select(e => e.Score * (e.Opponent.Scorecard?.Score(round) ?? 0)).Sum();

        public int CountWin() => Entries.Where(e => (int)e.Result >= (int)Result.Win).Count();
        public int CountWin(int round) => Entries.Where(e => e.Round < round && (int)e.Result >= (int)Result.Win).Count();

        public int CountBlackWin() => Entries.Where(e => e.Side == Side.Black && (int)e.Result == (int)Result.Win).Count();
        public int CountBlackWin(int round) => Entries.Where(e => e.Side == Side.Black && e.Round < round && (int)e.Result == (int)Result.Win).Count();

        public float CumulativeScore(int round = int.MaxValue)
        {
            float sum = 0;
            float runningScore = 0;
            int r = 0;
            foreach (var entry in Entries)
            {
                if (r >= round) break;
                runningScore += entry.Score;
                sum += runningScore;
                ++r;
            }
            return sum;
        }

        public float GetTieBreak(TieBreak tieBreak)
        {
            return tieBreak switch
            {
                TieBreak.Score => Score(),
                TieBreak.Buchholz => Buchholz(),
                TieBreak.BuchholzMedian => BuchholzMedian(),
                TieBreak.BuchholzCut1 => BuchholzCut1(),
                TieBreak.RefinedBuchholz => RefinedBuchholz(),
                TieBreak.CountWin => CountWin(),
                TieBreak.CountWinWithBlack => CountBlackWin(),
                TieBreak.CumulativeScore => CumulativeScore(),
                _ => 0,
            };
        }

        public class Entry
        {
            internal Entry(Participant opponent, Side side, Result result, int round, Tournament tournament)
            {
                Opponent = opponent;
                Side = side;
                Result = result;
                Round = round;
                Tournament = tournament;
            }

            private Tournament Tournament { get; set; }

            public int Round { set; get; }

            public Participant Opponent { set; get; }

            public Side Side { set; get; }

            public Result Result { set; get; }

            public float Score => Tournament.ScoringScheme.Score(Result);
        }
    }

    public class ScoringScheme
    {
        public float PointsForWin
        {
            set { _scores[(int)Result.Win] = value; _scores[(int)Result.UnratedWin] = value; _scores[(int)Result.ForfeitWin] = value; _scores[(int)Result.FullPointBye] = value; }
            get { return _scores[(int)Result.Win]; }
        }
        public float PointsForDraw
        {
            set { _scores[(int)Result.Draw] = value; _scores[(int)Result.HalfPointBye] = value; _scores[(int)Result.UnratedDraw] = value; }
            get { return _scores[(int)Result.Draw]; }
        }
        public float PointsForPlayedLoss
        {
            set { _scores[(int)Result.Loss] = value; _scores[(int)Result.UnratedLoss] = value; }
            get { return _scores[(int)Result.Loss]; }
        }
        public float PointsForZeroPointBye
        {
            set { _scores[(int)Result.ZeroPointBye] = value; }
            get { return _scores[(int)Result.ZeroPointBye]; }
        }
        public float PointsForForfeitedLoss
        {
            set { _scores[(int)Result.Forfeited] = value; }
            get { return _scores[(int)Result.Forfeited]; }
        }
        public float PointsForPairingAllocatedBye
        {
            set
            {
                _scores[(int)Result.PairingAllocatedBye] = value;
            }
            get { return _scores[(int)Result.PairingAllocatedBye]; }
        }

        internal float Score(Result r)
        {
            return _scores[(int)r];
        }

        public bool IsDefault => _scores.SequenceEqual(default_scores);

        private static readonly float[] default_scores = new float[14] { 0f, 0f, 0f, 0f, 0f, 1f, .5f, .5f, .5f, 1f, 1f, 1f, 1f, 0f };

        //public enum Result { Open, Forfeited, Loss, UnratedLoss, ZeroPointBye, PairingAllocatedBye, Draw, UnratedDraw, HalfPointBye, Win, UnratedWin, ForfeitWin, FullPointBye, Double Forfeit }
        private readonly float[] _scores = (float[])default_scores.Clone();

        internal List<string> TRFStrings()
        {
            List<string> result = new();
            if (!IsDefault)
            {
                result.Add(FormattableString.Invariant($"BBW {PointsForWin,4:F1}"));
                result.Add(FormattableString.Invariant($"BBD {PointsForDraw,4:F1}"));
                result.Add(FormattableString.Invariant($"BBL {PointsForPlayedLoss,4:F1}"));
                result.Add(FormattableString.Invariant($"BBZ {PointsForZeroPointBye,4:F1}"));
                result.Add(FormattableString.Invariant($"BBF {PointsForForfeitedLoss,4:F1}"));
                result.Add(FormattableString.Invariant($"BBU {PointsForPairingAllocatedBye,4:F1}"));
            }
            return result;
        }

    }

    public enum TieBreak { Score, Buchholz, BuchholzMedian, BuchholzCut1, RefinedBuchholz, SonnebornBerger, CountWin, CountWinWithBlack, CumulativeScore }

    internal class ScoreCardComparer : IComparer<Scorecard>, IComparer<Participant>
    {
        public List<TieBreak>? Tiebreaks { get; set; }

        public int Compare(Scorecard? x, Scorecard? y)
        {
            if (Tiebreaks == null || x == null || y == null) return 0;
            foreach (var tieBreak in Tiebreaks)
            {
                int cv = y.GetTieBreak(tieBreak).CompareTo(x.GetTieBreak(tieBreak));
                if (cv != 0) return cv;
            }
            return 0;
        }

        public int Compare(Participant? x, Participant? y)
        {
            return Compare(x?.Scorecard, y?.Scorecard);
        }
    }
}