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

        public float Buchholz() => Entries.Select(e => ((Scorecard)e.Opponent.Attributes[Participant.AttributeKey.Scorecard]).Score()).Sum();
        public float Buchholz(int round) => Entries.Where(e => e.Round < round).Select(e => ((Scorecard)e.Opponent.Attributes[Participant.AttributeKey.Scorecard]).Score(round)).Sum();

        public float BuchholzCut1() => Entries.Select(e => ((Scorecard)e.Opponent.Attributes[Participant.AttributeKey.Scorecard]).Score()).ToList().Skip(1).Sum();
        public float BuchholzCut1(int round) => Entries.Where(e => e.Round < round).Select(e => ((Scorecard)e.Opponent.Attributes[Participant.AttributeKey.Scorecard]).Score(round)).ToList().Skip(1).Sum();

        public float RefinedBuchholz() => Entries.Select(e => ((Scorecard)e.Opponent.Attributes[Participant.AttributeKey.Scorecard]).Buchholz()).Sum();
        public float RefinedBuchholz(int round) => Entries.Where(e => e.Round < round).Select(e => ((Scorecard)e.Opponent.Attributes[Participant.AttributeKey.Scorecard]).Buchholz(round)).Sum();

        public float SonnebornBergerScore() => Entries.Select(e => e.Score * ((Scorecard)e.Opponent.Attributes[Participant.AttributeKey.Scorecard]).Score()).Sum();
        public float SonnebornBergerScore(int round) => Entries.Where(e => e.Round < round).Select(e => e.Score * ((Scorecard)e.Opponent.Attributes[Participant.AttributeKey.Scorecard]).Score(round)).Sum();

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
        public float PointsForWin { 
            set { _scores[(int)Result.Win] = value; _scores[(int)Result.UnratedWin] = value; _scores[(int)Result.ForfeitWin] = value; _scores[(int)Result.FullPointBye] = value; } 
            get { return _scores[(int)Result.Win]; } }
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
            set { _scores[(int)Result.PairingAllocatedBye] = value; }
            get { return _scores[(int)Result.PairingAllocatedBye]; }
        }

        internal float Score(Result r) {  return _scores[(int)r]; }

        //public enum Result { Open, Forfeited, Loss, UnratedLoss, ZeroPointBye, Draw, UnratedDraw, HalfPointBye, Win, UnratedWin, ForfeitWin, FullPointBye, PairingAllocatedBye }
        private readonly float[] _scores = new float[13] {0f, 0f, 0f, 0f, 0f, .5f, .5f, .5f, 1f, 1f, 1f, 1f, 1f };

        internal List<string> TRFStrings()
        {
            List<string> result = new();
            if (this != Default)
            {
                if (PointsForWin != Default.PointsForWin) result.Add(FormattableString.Invariant($"BBW {PointsForWin:F1}"));
                if (PointsForDraw != Default.PointsForDraw) result.Add(FormattableString.Invariant($"BBD {PointsForDraw:F1}"));
                if (PointsForPlayedLoss != Default.PointsForPlayedLoss) result.Add(FormattableString.Invariant($"BBL {PointsForPlayedLoss:F1}"));
                if (PointsForZeroPointBye != Default.PointsForZeroPointBye) result.Add(FormattableString.Invariant($"BBZ {PointsForZeroPointBye:F1}"));
                if (PointsForForfeitedLoss != Default.PointsForForfeitedLoss) result.Add(FormattableString.Invariant($"BBF {PointsForForfeitedLoss:F1}"));
                if (PointsForPairingAllocatedBye != Default.PointsForPairingAllocatedBye) result.Add(FormattableString.Invariant($"BBU {PointsForPairingAllocatedBye:F1}"));
            }
            return result;
        }

        public readonly static ScoringScheme Default = new();
    }
}