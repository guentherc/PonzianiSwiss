using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PonzianiSwissLib
{
    public static class Export
    {
        public static string CrosstableHTML(this Tournament tournament, int round = int.MaxValue)
        {
            round = Math.Min(round, tournament.Rounds.Count);
            tournament.OrderByRank(round);
            StringBuilder sb = new();
            sb.AppendLine($"<h2 align=\"center\">{HttpUtility.HtmlEncode(tournament.Name)}</h2>");
            if (tournament.Participants != null)
            {
                sb.AppendLine(@"<div align=""center"">");
                sb.AppendLine(@"<center>");
                sb.AppendLine(@"<table border=""2"" cellpadding=""2"" cellspacing=""2"" style=""border-collapse: collapse"" bordercolor=""#111111"" >");
                sb.AppendLine(@"<thead>");
                sb.AppendLine(@"<tr>");
                sb.AppendLine($"<td colspan=\"8\">{HttpUtility.HtmlEncode(Strings.CrossTableForRound).Replace("&", round.ToString())} </td>");
                sb.AppendLine(@"</tr>");
                sb.AppendLine(@"<tr>");

                List<string> columnNames = new List<string>() { Strings.ParticpantListRank, Strings.Participant, Strings.ParticipantListFideRating,
                                                  Strings.ParticipantListNationalRating };
                for (int i = 1; i<= round; i++) columnNames.Add(i.ToString());
                foreach(var tb in tournament.TieBreak)
                {
                    columnNames.Add(tb.ToString());
                }
                foreach (string columnName in columnNames) sb.AppendLine($"<th>{HttpUtility.HtmlEncode(columnName)}</th>");
                sb.AppendLine(@"</tr>");
                int rank = 1;
                foreach (Participant p in tournament.Participants)
                {
                    sb.AppendLine(@"<tr>");
                    sb.AppendLine($"<td>{rank}.</td>");
                    sb.AppendLine($"<td>{p.Name}.</td>");
                    sb.AppendLine($"<td>{p.FideRating}</td>");
                    sb.AppendLine($"<td>{p.AlternativeRating}</td>");
                    for (int i = 0;i<round; ++i)
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
