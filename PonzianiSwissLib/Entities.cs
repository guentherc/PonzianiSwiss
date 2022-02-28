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
                    _ => Result,
                };
            }
        }

        public float ScoreWhite => Result == Result.Draw ? 0.5f : (Result < Result.Draw ? 0f : 1.0f);
        public float ScoreBlack => 1 - ScoreWhite;
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

        public Scorecard(Participant participant)
        {
            Participant = participant;
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

        public int CountBlackWin() => Entries.Where(e => e.Side == Side.Black && (int)e.Result >= (int)Result.Win).Count();
        public int CountBlackWin(int round) => Entries.Where(e => e.Side == Side.Black && e.Round < round && (int)e.Result >= (int)Result.Win).Count();

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
            public Entry(Participant opponent, Side side, Result result, int round)
            {
                Opponent = opponent;
                Side = side;
                Result = result;
                Round = round;
            }

            public int Round { set; get; }

            public Participant Opponent { set; get; }

            public Side Side { set; get; }

            public Result Result { set; get; }

            public float Score => Result == Result.Draw ? 0.5f : (Result < Result.Draw ? 0f : 1.0f);
        }
    }
}