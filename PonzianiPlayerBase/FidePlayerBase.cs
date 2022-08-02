using Microsoft.Extensions.Logging;
using PonzianiSwissLib;
using System.IO.Compression;

namespace PonzianiPlayerBase
{

    public class FidePlayerBase : PlayerBase
    {
        public override string Description => Strings.BaseDescription_FIDE;

        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.FIDE;

        public override bool Initialize(ILogger? logger)
        {
            this.logger = logger;
            if (!base.Initialize(logger)) return false;
            if (connection == null) return false;
            var command = connection.CreateCommand();
            command.CommandText = "SELECT LastUpdate FROM AdminData WHERE Id = 0";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lastUpdate = new(reader.GetInt64(0));
                return true;
            }
            return false;
        }

        public override List<Player> Find(string searchstring, int max = 0)
        {
            List<Player> result = new();
            using var cmd = connection?.CreateCommand();
            if (cmd == null) return result;
            cmd.CommandText = max > 0 ? $"SELECT * FROM FidePlayer WHERE Name LIKE @ss LIMIT {max}" : "SELECT * FROM FidePlayer WHERE Name LIKE @ss";
            cmd.Parameters.AddWithValue("@ss", searchstring + '%');
            cmd.Prepare();
            logger?.LogDebug("{sql} {param}", cmd.CommandText, cmd.Parameters[0].Value);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Player player = new(reader.GetInt64(0).ToString());
                player.FideId = ulong.Parse(player.Id);
                player.Name = reader.GetString(1);
                player.Federation = reader.GetString(2);
                player.Title = (FideTitle)reader.GetInt32(3);
                player.Sex = (Sex)reader.GetInt16(4);
                player.Rating = reader.GetInt32(5);
                player.Inactive = reader.GetInt16(6) == 1;
                player.YearOfBirth = reader.GetInt16(7);
                result.Add(player);
            }
            logger?.LogDebug("{count} Records read", result.Count);
            return result;
        }

        public override Player? GetById(string id)
        {
            using var cmd = connection?.CreateCommand();
            if (cmd == null) return null;
            cmd.CommandText = $"SELECT * FROM FidePlayer WHERE Id = \"{id}\"";
            logger?.LogDebug("{}", cmd.CommandText);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Player player = new(reader.GetInt64(0).ToString());
                player.FideId = ulong.Parse(player.Id);
                player.Name = reader.GetString(1);
                player.Federation = reader.GetString(2);
                player.Title = (FideTitle)reader.GetInt32(3);
                player.Sex = (Sex)reader.GetInt16(4);
                player.Rating = reader.GetInt32(5);
                player.Inactive = reader.GetInt16(6) == 1;
                player.YearOfBirth = reader.GetInt16(7);
                logger?.LogDebug("Found {name}", player.Name);
                return player;
            }
            logger?.LogDebug("No player found!");
            return null;
        }

        public override async Task<bool> UpdateAsync()
        {
            var httpClient = new HttpClient();
            ProgressUpdate(1, PonzianiPlayerBase.Strings.PreparingDownload);
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "ratings");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "ratings.zip");
            try
            {
                if (!File.Exists(tmpFile))
                {
                    ProgressUpdate(2, string.Format(PonzianiPlayerBase.Strings.DownloadingData, "FIDE"));
                    using var stream = await httpClient.GetStreamAsync("http://ratings.fide.com/download/players_list.zip");
                    using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                    await stream.CopyToAsync(fileStream);
                    ProgressUpdate(20, PonzianiPlayerBase.Strings.DownloadCompletedStartingDataProcessing);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            if (!Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".txt").Any())
            {
                ZipFile.ExtractToDirectory(tmpFile, tmpDir);
            }
            //Find txt-file
            string file = Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".txt").First();
            logger?.LogDebug("Data saved to {file}", file);

            if (connection == null) return false;

            ProgressUpdate(25, PonzianiPlayerBase.Strings.StartingDatabaseUpdate);
            using (var transaction = connection.BeginTransaction())
            {
                using (var del = connection.CreateCommand())
                {
                    del.CommandText = "DELETE FROM FidePlayer";
                    await del.ExecuteNonQueryAsync();
                }
                ProgressUpdate(30, PonzianiPlayerBase.Strings.OldDataRemoved);
                var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT INTO FidePlayer VALUES(@Id, @Name, @Federation, @Title, @Sex, @Rating, @Inactive, @Birthyear)";
                string[] parameters = new[] { "@Id", "@Name", "@Federation", "@Title", "@Sex", "@Rating", "@Inactive", "@Birthyear" };
                foreach (var p in parameters)
                {
                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = p;
                    cmd.Parameters.Add(parameter);
                }

                int count = 0;
                using (StreamReader reader = new(file))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            ++count;
                            if (count == 1) continue;
                            int id = int.Parse(line[..15].Trim());
                            cmd.Parameters["@Id"].Value = id;
                            cmd.Parameters["@Name"].Value = line.Substring(15, 61).Trim();
                            cmd.Parameters["@Federation"].Value = line.Substring(76, 4).Trim();
                            cmd.Parameters["@Sex"].Value = line[80] == 'F' ? (int)Sex.Female : (int)Sex.Male;
                            string title = line.Substring(84, 3).Trim();
                            cmd.Parameters["@Title"].Value = title.Length > 0 ? (int)Enum.Parse<FideTitle>(title) : (int)FideTitle.NONE;
                            string rating = line.Substring(113, 4).Trim();
                            cmd.Parameters["@Rating"].Value = rating.Length > 0 ? int.Parse(rating) : 0;
                            //rating = line.Substring(126, 4).Trim();
                            //if (rating.Length > 0) player.RatingRapid = int.Parse(rating);
                            //rating = line.Substring(139, 4).Trim();
                            //if (rating.Length > 0) player.RatingBlitz = int.Parse(rating);
                            string year = line.Substring(152, 4).Trim();
                            if (year.Length > 0 && int.TryParse(year, out int _))
                                cmd.Parameters["@Birthyear"].Value = year;
                            string flags = line.Substring(158, 4).Trim();
                            cmd.Parameters["@Inactive"].Value = flags.Contains('i') ? 1 : 0;
                            if (count == 2) await cmd.PrepareAsync();
                            await cmd.ExecuteNonQueryAsync();
                            if (count % 100 == 0)
                            {
                                ProgressUpdate((int)(30 + count * 65.0 / 1200000), string.Format(PonzianiPlayerBase.Strings.CountPlayersProcessed, count));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                ProgressUpdate(95, string.Format(PonzianiPlayerBase.Strings.CountPlayersProcessedCommitStarted, count));
                cmd = connection.CreateCommand();
                cmd.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"0\"";
                lastUpdate = DateTime.UtcNow;
                await cmd.ExecuteNonQueryAsync();
                transaction.Commit();
                cmd = connection.CreateCommand();
                cmd.CommandText = $"VACUUM";
                await cmd.ExecuteNonQueryAsync();
            }

            ProgressUpdate(99, PonzianiPlayerBase.Strings.Cleanup);
            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            ProgressUpdate(100, PonzianiPlayerBase.Strings.Done);
            return true;
        }

        public async Task<List<Player>?> GetRandomPlayers(int count = 100, int minRating = 500, int maxRating = 3000, List<string>? federations = null)
        {
            List<Player> result = new();
            using var cmd = connection?.CreateCommand();
            if (cmd == null) return result;
            cmd.CommandText = $"SELECT * FROM FidePlayer WHERE RATING >= @min AND RATING <= @max AND INACTIVE = 0 ORDER BY RANDOM() LIMIT {count}";
            cmd.Parameters.AddWithValue("@min", minRating);
            cmd.Parameters.AddWithValue("@max", maxRating);
            if (federations != null)
            {
                cmd.CommandText += $" AND Federation IN ({string.Join(',', federations)}";
            }
            cmd.Prepare();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Player player = new(reader.GetInt64(0).ToString());
                player.FideId = ulong.Parse(player.Id);
                player.Name = reader.GetString(1);
                player.Federation = reader.GetString(2);
                player.Title = (FideTitle)reader.GetInt32(3);
                player.Sex = (Sex)reader.GetInt16(4);
                player.Rating = reader.GetInt32(5);
                player.Inactive = reader.GetInt16(6) == 1;
                player.YearOfBirth = reader.GetInt16(7);
                result.Add(player);
            }
            return result;
        }
    }


}
