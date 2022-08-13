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
            List<string> columnNames = new() { Strings.ParticpantListRank, Strings.Participant, Strings.ParticipantListFideRating,
                                                  Strings.ParticipantListNationalRating };
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

            List<string> columnNames = new() { Strings.BoardNumber, Strings.ParticipantIdShort, Strings.Participant, Strings.Title, Strings.Score, "-",
                                                          Strings.ParticipantIdShort, Strings.Participant, Strings.Title, Strings.Score, Strings.Result };
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
                sb.AppendLine(@"<table border=""2"" cellpadding=""2"" cellspacing=""2"" style=""border-collapse: collapse"" bordercolor=""#111111"" >");
                sb.AppendLine(@"<thead>");
                sb.AppendLine(@"<tr>");
                if (additionalRanking == null)
                    sb.AppendLine($"<td colspan=\"8\">{HttpUtility.HtmlEncode(Strings.CrossTableForRound.Replace("&", round.ToString()))} </td>");
                else
                    sb.AppendLine($"<td colspan=\"8\">{HttpUtility.HtmlEncode(Strings.CrossTableForRound.Replace("&", round.ToString()))} ({additionalRanking.Title})</td>");
                sb.AppendLine(@"</tr>");
                List<string> columnNames = new() { Strings.ParticpantListRank, Strings.Participant, Strings.ParticipantListFideRating,
                                                  Strings.ParticipantListNationalRating };
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
                        sb.AppendLine($"<td>{p.Name}.</td>");
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
                            sb.AppendLine($"<td>{p.Scorecard?.GetTieBreak(tb)}</td>");
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
                    participants = tournament.Participants.OrderByDescending(e => tournament.Rating(e)).ToList();
                else
                    participants = tournament.Participants.OrderBy(e => tournament.Rating(e)).ToList();
            }
            else
            {
                PropertyInfo? propertyInfo = tournament.Participants.First().GetType().GetProperty(sortPropertyName);
                if (propertyInfo != null)
                {
                    if (descending)
                        participants = tournament.Participants.OrderByDescending(e => propertyInfo.GetValue(e, null)).ToList();
                    else
                        participants = tournament.Participants.OrderBy(e => propertyInfo.GetValue(e, null)).ToList();
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
                string[] columnNames = new string[] { Strings.ParticpantListRank, Strings.Participant, Strings.Title, Strings.ParticipantListRating, Strings.ParticipantListFideRating,
                                                  Strings.ParticipantListNationalRating, Strings.Club, Strings.Federation };
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
    }
}
