using MathNet.Numerics.Distributions;
using System.Reflection;
using System.Text;
using System.Web;

namespace PonzianiSwissLib
{
    public static class Export
    {
        public static string TeamHTML(this Tournament tournament, int round = int.MaxValue)
        {
            round = Math.Min(round, tournament.Rounds.Count - 1);
            StringBuilder sb = new();
            var teamScoreCards = tournament.GetTeamScorecards(round);
            sb.AppendLine($"<h2 align=\"center\">{HttpUtility.HtmlEncode(tournament.Name)}</h2>");
            int rank = 1;
            sb.AppendLine(@"<div align=""center"">");
            sb.AppendLine(@"<center>");
            sb.AppendLine(@"<table border=""2"" cellpadding=""2"" cellspacing=""2"" style=""border-collapse: collapse"" bordercolor=""#111111"" >");
            sb.AppendLine(@"<thead>");
            sb.AppendLine(@"<tr>");
            sb.AppendLine($"<td colspan=\"{4 + tournament.TieBreak.Count}\">{HttpUtility.HtmlEncode(Strings.TeamRankingTable.Replace("&", (round + 1).ToString()))} </td>");
            sb.AppendLine(@"</tr>");
            List<string> columnNames = [ Strings.ParticpantListRank, Strings.Participant, Strings.ParticipantListFideRating,
                                                  Strings.ParticipantListNationalRating ];
            foreach (var tb in tournament.TieBreak)
            {
                columnNames.Add(tb.ToString());
            }
            sb.AppendLine(@"<tr>");
            foreach (string columnName in columnNames) sb.AppendLine($"<th>{HttpUtility.HtmlEncode(columnName)}</th>");
            sb.AppendLine(@"</tr>");

            foreach (var tsc in teamScoreCards)
            {
                sb.AppendLine($"<tr><td rowspan=\"{tsc.Scorecards.Count + 2}\"><strong>{rank}</strong></td><td colspan=\"{3 + tournament.TieBreak.Count}\"><strong>{tsc.Name}</strong></td></tr>");
                int indx = 0;
                foreach (Scorecard sc in tsc.Scorecards)
                {
                    string em = indx < tournament.TeamSize ? string.Empty : "<em>";
                    string emo = indx < tournament.TeamSize ? string.Empty : "</em>";
                    ++indx;
                    sb.AppendLine($"<em><tr><td style=\"padding-left:10px\">{em}{sc.Participant.Name}{emo}</td>");
                    sb.AppendLine($"<td>{em}{sc.Participant.FideRating}{emo}</td>");
                    sb.AppendLine($"<td>{em}{sc.Participant.AlternativeRating}{emo}</td>");
                    foreach (var tb in tournament.TieBreak)
                    {
                        sb.AppendLine($"<td>{em}{sc.Participant.Scorecard?.GetTieBreak(tb)}{emo}</td>");
                    }
                    sb.AppendLine(@"</tr></em>");
                }
                sb.AppendLine("<tr><td colspan=\"3\"></td>");
                foreach (var tb in tournament.TieBreak)
                {
                    sb.AppendLine($"<td><strong>{tsc.GetTieBreak(tb)}</strong></td>");
                }
                sb.AppendLine(@"</tr>");
                ++rank;
            }
            sb.AppendLine(@"</table>");
            return sb.ToString();
        }
        public static string RoundHTML(this Tournament tournament, int round = int.MaxValue)
        {
            round = Math.Min(round, tournament.Rounds.Count - 1);
            tournament.OrderByRank(round);
            StringBuilder sb = new();
            sb.AppendLine($"<h2 align=\"center\">{HttpUtility.HtmlEncode(tournament.Name)}</h2>");
            sb.AppendLine(@"<div align=""center"">");
            sb.AppendLine(@"<center>");
            sb.AppendLine(@"<table border=""2"" cellpadding=""2"" cellspacing=""2"" style=""border-collapse: collapse"" bordercolor=""#111111"" >");
            sb.AppendLine(@"<thead>");
            sb.AppendLine(@"<tr>");
            sb.AppendLine($"<td colspan=\"8\">{HttpUtility.HtmlEncode(Strings.RoundResults.Replace("&", (round + 1).ToString()))} </td>");
            sb.AppendLine(@"</tr>");

            List<string> columnNames = [ Strings.BoardNumber, Strings.ParticipantIdShort, Strings.Participant, Strings.Title, Strings.Score, "-",
                                                          Strings.ParticipantIdShort, Strings.Participant, Strings.Title, Strings.Score, Strings.Result ];
            sb.AppendLine(@"<tr>");
            foreach (string columnName in columnNames) sb.AppendLine($"<th>{HttpUtility.HtmlEncode(columnName)}</th>");
            sb.AppendLine(@"</tr>");

            int board = 1;
            foreach (var p in tournament.Rounds[round].Pairings)
            {
                sb.AppendLine(@"<tr>");
                sb.AppendLine($"<td>{board}</td>");
                sb.AppendLine($"<td>{p.White?.RankId}</td>");
                sb.AppendLine($"<td>{p.White?.Name}</td>");
                sb.AppendLine($"<td>{(p.White?.Title == FideTitle.NONE ? string.Empty : p.White?.Title.ToString())}</td>");
                sb.AppendLine($"<td>({p.White?.Scorecard?.Score(round):F1})</td>");
                sb.AppendLine($"<td>-</td>");
                sb.AppendLine($"<td>{p.Black?.RankId}</td>");
                sb.AppendLine($"<td>{p.Black?.Name}</td>");
                sb.AppendLine($"<td>{(p.Black?.Title == FideTitle.NONE ? string.Empty : p.Black?.Title.ToString())}</td>");
                sb.AppendLine($"<td>({p.Black?.Scorecard?.Score(round):F1})</td>");
                sb.AppendLine($"<td>{Tournament.result_strings[(int)p.Result]}</td>");
                sb.AppendLine(@"</tr>");
                ++board;
            }

            return sb.ToString();
        }

        public static string CrosstableHTML(this Tournament tournament, int round = int.MaxValue, AdditionalRanking? additionalRanking = null)
        {
            round = Math.Min(round, tournament.Rounds.Count);
            tournament.OrderByRank(round);
            StringBuilder sb = new();
            sb.AppendLine($"<h2 align=\"center\">{HttpUtility.HtmlEncode(tournament.Name)}</h2>");
            var plist = additionalRanking == null ? tournament.Participants : tournament.GetParticipants(additionalRanking);
            if (plist != null)
            {
                sb.AppendLine(@"<div align=""center"">");
                sb.AppendLine(@"<center>");
                sb.AppendLine(@"<table class=""ps_crosstable"" border=""2"" cellpadding=""2"" cellspacing=""2"" style=""border-collapse: collapse"" bordercolor=""#111111"" >");
                sb.AppendLine(@"<thead>");
                sb.AppendLine(@"<tr>");
                if (additionalRanking == null)
                    sb.AppendLine($"<td colspan=\"8\">{HttpUtility.HtmlEncode(Strings.CrossTableForRound.Replace("&", round.ToString()))} </td>");
                else
                    sb.AppendLine($"<td colspan=\"8\">{HttpUtility.HtmlEncode(Strings.CrossTableForRound.Replace("&", round.ToString()))} ({additionalRanking.Title})</td>");
                sb.AppendLine(@"</tr>");
                List<string> columnNames = [ Strings.ParticpantListRank, Strings.Participant, Strings.ParticipantListFideRating,
                                                  Strings.ParticipantListNationalRating ];
                for (int i = 1; i <= round; i++) columnNames.Add(i.ToString());
                foreach (var tb in tournament.TieBreak)
                {
                    columnNames.Add(tb.ToString());
                }
                sb.AppendLine(@"<tr>");
                foreach (string columnName in columnNames) sb.AppendLine($"<th>{HttpUtility.HtmlEncode(columnName)}</th>");
                sb.AppendLine(@"</tr>");
                int rank = 1;
                foreach (Participant p in tournament.Participants)
                {
                    if (plist.Contains(p))
                    {
                        sb.AppendLine(@"<tr>");
                        sb.AppendLine($"<td>{rank}.</td>");
                        sb.AppendLine($"<td>{p.Name}</td>");
                        sb.AppendLine($"<td>{p.FideRating}</td>");
                        sb.AppendLine($"<td>{p.AlternativeRating}</td>");
                        for (int i = 0; i < round; ++i)
                        {
                            var entry = p.Scorecard?.Entries.Where(e => e.Round == i).ToList();
                            if (entry == null || entry.Count == 0) sb.AppendLine($"<td></td>");
                            else
                            {
                                sb.AppendLine($"<td>{entry[0].Opponent.RankId ?? "-"}{"ws"[(int)entry[0].Side]}{Tournament.result_char[(int)(entry[0].Result)]}</td>");
                            }
                        }
                        foreach (var tb in tournament.TieBreak)
                        {
                            sb.AppendLine($"<td  style=\"text-align:right;\">{p.Scorecard?.GetTieBreak(tb)}</td>");
                        }
                        sb.AppendLine(@"</tr>");
                    }
                    rank++;
                }

            }
            return sb.ToString();
        }

        public static string ParticipantListHTML(this Tournament tournament, string sortPropertyName, bool descending = false)
        {
            List<Participant>? participants = null;
            if (sortPropertyName == "Rating")
            {
                if (descending)
                    participants = [.. tournament.Participants.OrderByDescending(e => tournament.Rating(e))];
                else
                    participants = [.. tournament.Participants.OrderBy(tournament.Rating)];
            }
            else
            {
                PropertyInfo? propertyInfo = tournament.Participants.First().GetType().GetProperty(sortPropertyName);
                if (propertyInfo != null)
                {
                    if (descending)
                        participants = [.. tournament.Participants.OrderByDescending(e => propertyInfo.GetValue(e, null))];
                    else
                        participants = [.. tournament.Participants.OrderBy(e => propertyInfo.GetValue(e, null))];
                }
            }
            StringBuilder sb = new();
            sb.AppendLine($"<h2 align=\"center\">{HttpUtility.HtmlEncode(tournament.Name)}</h2>");
            if (participants != null)
            {
                sb.AppendLine(@"<div align=""center"">");
                sb.AppendLine(@"<center>");
                sb.AppendLine(@"<table border=""2"" cellpadding=""2"" cellspacing=""2"" style=""border-collapse: collapse"" bordercolor=""#111111"" >");
                sb.AppendLine(@"<thead>");
                sb.AppendLine(@"<tr>");
                sb.AppendLine($"<td colspan=\"8\">{HttpUtility.HtmlEncode(Strings.ParticipantListByStartOrder)} </td>");
                sb.AppendLine(@"</tr>");
                sb.AppendLine(@"<tr>");
                string[] columnNames = [ Strings.ParticpantListRank, Strings.Participant, Strings.Title, Strings.ParticipantListRating, Strings.ParticipantListFideRating,
                                                  Strings.ParticipantListNationalRating, Strings.Club, Strings.Federation ];
                foreach (string columnName in columnNames) sb.AppendLine($"<th>{HttpUtility.HtmlEncode(columnName)}</th>");
                sb.AppendLine(@"</tr>");
                sb.AppendLine(@"</thead>");
                sb.AppendLine(@"<tbody>");
                foreach (Participant participant in participants)
                {
                    sb.AppendLine(@"<tr>");
                    sb.AppendLine($"<td>{HttpUtility.HtmlEncode(participant.RankId)}</td>");
                    sb.AppendLine($"<td>{HttpUtility.HtmlEncode(participant.Name)}</td>");
                    sb.AppendLine($"<td>{HttpUtility.HtmlEncode(participant.Title == FideTitle.NONE ? " " : participant.Title)}</td>");
                    sb.AppendLine($"<td>{HttpUtility.HtmlEncode(tournament.Rating(participant).ToString())}</td>");
                    sb.AppendLine($"<td>{HttpUtility.HtmlEncode(participant.FideRating.ToString())}</td>");
                    sb.AppendLine($"<td>{HttpUtility.HtmlEncode(participant.AlternativeRating.ToString())}</td>");
                    sb.AppendLine($"<td>{HttpUtility.HtmlEncode(participant.Club)}</td>");
                    sb.AppendLine($"<td>{HttpUtility.HtmlEncode(participant.Federation)}</td>");
                    sb.AppendLine(@"</tr>");
                }
                sb.AppendLine(@"</tbody>");
                sb.AppendLine(@"</table></center></div>");
            }
            return sb.ToString();
        }

        public static string PGN(this Tournament tournament)
        {
            StringBuilder sb = new();
            foreach (Round round in tournament.Rounds)
            {
                foreach (var pairing in round.Pairings)
                {
                    sb.AppendLine($"[Event \"{tournament.Name}\"]");
                    sb.AppendLine($"[Site \"{tournament.City}\"]");
                    sb.AppendLine($"[Date \"{tournament.StartDate:yyyy.MM.dd}\"]");
                    sb.AppendLine($"[Round \"{round.Number + 1}\"]");
                    sb.AppendLine($"[White \"{pairing.White?.Name}\"]");
                    if (pairing.White?.FideRating > 0) sb.AppendLine($"[WhiteElo \"{pairing.White?.FideRating}\"]");
                    if (pairing.White?.FideId > 0) sb.AppendLine($"[WhiteFideId \"{pairing.White?.FideId}\"]");
                    if (pairing.White?.AlternativeRating > 0) sb.AppendLine($"[WhiteNrv \"{pairing.White?.AlternativeRating}\"]");
                    sb.AppendLine($"[Black \"{pairing.Black?.Name}\"]");
                    if (pairing.Black?.FideRating > 0) sb.AppendLine($"[BlackElo \"{pairing.Black?.FideRating}\"]");
                    if (pairing.Black?.FideId > 0) sb.AppendLine($"[BlackFideId \"{pairing.Black?.FideId}\"]");
                    if (pairing.Black?.AlternativeRating > 0) sb.AppendLine($"[BlackNrv \"{pairing.White?.AlternativeRating}\"]");
                    sb.AppendLine($"[Result \"{Tournament.result_strings[(int)pairing.Result]}\"]");
                    sb.AppendLine();
                    sb.AppendLine($"{Tournament.result_strings[(int)pairing.Result]}");
                    sb.AppendLine();

                }
            }
            return sb.ToString();
        }
    }

    public static class RatingCalculator
    {
        public const double SIGMA = 282.84271247461900976033774484194;
        private static readonly Normal _normal = new(0, SIGMA);

        public static double WinExpectation(double dwz1, double dwz2) => _normal.CumulativeDistribution(dwz1 - dwz2);
        public static double RatingDiff(double score) => _normal.InverseCumulativeDistribution(score);

        public static DWZEvaluation? Evaluate(Participant p, DateTime? Birthday = null, int Index = 10)
        {
            if (Birthday == null && p.YearOfBirth != 0)
            {
                Birthday = new DateTime(p.YearOfBirth, 1, 1);
            }
            DWZEvaluation dwzEvaluation = new()
            {
                Name = p.Name ?? string.Empty,
                FideId = p.FideId,
                Age = Birthday == null ? 50 : CalculateAge(Birthday.Value, DateTime.Now),
                OldDWZ = p.AlternativeRating

            };
            if (p.Scorecard == null || p.Scorecard.Entries.Count == 0) return dwzEvaluation;
            var valGames = p.Scorecard.Entries.Where(e => e.Opponent.AlternativeRating > 0 && (e.Result == Result.Loss || e.Result == Result.Win || e.Result == Result.Draw));
            dwzEvaluation.Games = valGames.Count();
            dwzEvaluation.ExpectedScore = valGames.Sum(e => WinExpectation(p.AlternativeRating, e.Opponent.AlternativeRating));
            dwzEvaluation.OpponentAverageRating = valGames.Sum(e => e.Opponent.AlternativeRating) / dwzEvaluation.Games;
            dwzEvaluation.Score = valGames.Sum(e => e.Result == Result.Win ? 1.0 : e.Result == Result.Draw ? 0.5 : 0.0);
            double fb = 1;
            if (dwzEvaluation.Age < 20 && dwzEvaluation.Score > dwzEvaluation.ExpectedScore)
            {
                fb = Math.Clamp(dwzEvaluation.OldDWZ / 2000.0, 0.5, 1.0);
            }
            double sbr = 0;
            if (dwzEvaluation.OldDWZ < 1300 && dwzEvaluation.Score < dwzEvaluation.ExpectedScore)
            {
                sbr = Math.Exp((1300 - dwzEvaluation.OldDWZ) / 150.0) - 1;
            }
            dwzEvaluation.Coefficient = (Math.Pow(dwzEvaluation.OldDWZ / 1000.0, 4) + J(dwzEvaluation.Age)) * fb + sbr;
            if (dwzEvaluation.OldDWZ < 1300 && dwzEvaluation.Score < dwzEvaluation.ExpectedScore)
            {
                dwzEvaluation.Coefficient = Math.Clamp(dwzEvaluation.Coefficient, 5, 150);
            }
            else
            {
                if (Index < 5)
                    dwzEvaluation.Coefficient = Math.Clamp(dwzEvaluation.Coefficient, 5, 5 * Index);
                else
                    dwzEvaluation.Coefficient = Math.Clamp(dwzEvaluation.Coefficient, 5, 30);
            }
            dwzEvaluation.Coefficient = Math.Round(dwzEvaluation.Coefficient);
            dwzEvaluation.NewDWZ = Math.Round(dwzEvaluation.OldDWZ + 800 * (dwzEvaluation.Score - dwzEvaluation.ExpectedScore) / (dwzEvaluation.Coefficient + dwzEvaluation.Games));
            if (dwzEvaluation.Score == 0) dwzEvaluation.Performance = dwzEvaluation.OpponentAverageRating - 677;
            else if (dwzEvaluation.Score == dwzEvaluation.Games) dwzEvaluation.Performance = dwzEvaluation.OpponentAverageRating + 677;
            else
            {
                dwzEvaluation.Performance = dwzEvaluation.OpponentAverageRating + RatingDiff(dwzEvaluation.Score / dwzEvaluation.Games);
                while (true)
                {
                    var estimatedScore = valGames.Select(p => p.Opponent).Select(o => WinExpectation(dwzEvaluation.Performance, o.AlternativeRating)).Sum();
                    var pe = (dwzEvaluation.Score - estimatedScore) / dwzEvaluation.Games + 0.5;
                    var de = RatingDiff(pe);
                    var newPe = dwzEvaluation.Performance + de;
                    if (Math.Round(newPe) == Math.Round(dwzEvaluation.Performance)) break;
                    dwzEvaluation.Performance = newPe;
                }
            }
            return dwzEvaluation;
        }

        private static int J(int age)
        {
            if (age > 25) return 15;
            else if (age > 20) return 10;
            else return 5;
        }

        public static int CalculateAge(DateTime birthDate, DateTime now)
        {
            int age = now.Year - birthDate.Year;
            if (birthDate > now.AddYears(-age))
                age--;
            return age;
        }
    }

    public class DWZEvaluation
    {
        public string Name { get; set; } = string.Empty;
        public ulong FideId { get; set; } = 0;
        public double OldDWZ { get; set; } = 0;
        public double NewDWZ { get; set; } = 0;
        public double Score { get; set; } = 0.0;
        public int Games { get; set; } = 0;
        public double ExpectedScore { get; set; } = 0.0;
        public double Coefficient { get; set; } = 30;
        public double OpponentAverageRating { get; set; } = 0.0;
        public double Performance { get; set; } = 0.0;
        public int Age { get; set; } = 0;

    }
}
