using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string[] lines = content.Split('\n');
            List<string> plist = new();
            foreach (string line in lines)
            {
                if (line.Trim().Length < 4 || line[3] != ' ') continue;
                if (!int.TryParse(line.AsSpan(0, 3), out int code)) continue;

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
                if (c == 'f' || ((int)p.Title & 1) == 1) p.Attributes.Add(Participant.AttributeKey.Sex, Sex.Female);
                else if (c == 'm') p.Attributes.Add(Participant.AttributeKey.Sex, Sex.Male);
                tournament.Participants.Add(p);
            }
            //second pass to get pairings 
            for (int i = 0; i < playerList.Count; ++i)
            {
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
                        if (opponent != null && opponent.ParticipantId?.CompareTo(tournament.Participants[i].ParticipantId) < 0)
                        {
                            Pairing p = color == 'b' ? new(opponent, tournament.Participants[i]) : new(tournament.Participants[i], opponent);
                            switch (result)
                            {
                                case '1':
                                case 'W':
                                    p.Result = color == 'b' ? Result.Loss : Result.Win; break;
                                case '0':
                                case 'L':
                                    p.Result = color == 'b' ? Result.Win : Result.Loss; break;
                                case '=':
                                case 'D':
                                    p.Result = Result.Draw; break;
                                case '+':
                                    p.Result = color == 'b' ? Result.Forfeited : Result.ForfeitWin; break;
                                case '-':
                                    p.Result = color == 'b' ? Result.ForfeitWin : Result.Forfeited; break;
                            }

                            tournament.Rounds[round].Pairings.Add(p);
                        }
                    }
                    indx += 10;
                    ++round;
                }
            }
        }
    }
}
