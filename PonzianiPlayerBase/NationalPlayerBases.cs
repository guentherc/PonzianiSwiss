﻿using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using Microsoft.Extensions.Logging;
using PonzianiSwissLib;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace PonzianiPlayerBase
{
    public abstract class NationalPlayerBase : PlayerBase
    {
        public override List<Player> Find(string searchstring, int max = 0)
        {
            string federation = Key.ToString();
            List<Player> result = [];
            using var cmd = connection?.CreateCommand();
            if (cmd == null) return result;
            cmd.CommandText = max > 0 ? $"SELECT * FROM Player WHERE Federation = @fed AND Name LIKE @ss LIMIT {max}" : "SELECT * FROM Player WHERE Federation = @fed AND  Name LIKE @ss";
            cmd.Parameters.AddWithValue("@fed", federation);
            cmd.Parameters.AddWithValue("@ss", searchstring + '%');
            cmd.Prepare();
            logger?.LogDebug("{sql} {p1} {p2}", cmd.CommandText, cmd.Parameters[0].Value, cmd.Parameters[1].Value);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Player player = new($"{reader.GetString(0)}_{reader.GetString(1)}", reader.GetString(3))
                    {
                        Federation = reader.GetString(0),
                        Club = reader.GetString(2),
                        Title = FideTitle.NONE,
                        Sex = (Sex)reader.GetInt16(4),
                        Rating = reader.GetInt32(5),
                        Inactive = reader.GetInt16(6) == 1,
                        YearOfBirth = reader.GetInt16(7),
                        FideId = (ulong)reader.GetInt64(8)
                    };
                    result.Add(player);
                }
                logger?.LogDebug("{count} Records read", result.Count);
            }
            if (result.Count == 1 && result[0].Club?.Length > 0)
            {
                using var ccmd = connection?.CreateCommand();
                if (ccmd != null)
                {
                    ccmd.CommandText = $"SELECT Name FROM Club WHERE Federation = \"{federation}\" and Id = \"{result[0].Club}\"";
                    using var creader = ccmd.ExecuteReader();
                    logger?.LogDebug("{}", ccmd.CommandText);
                    while (creader.Read())
                    {
                        result[0].Club = creader.GetString(0);
                        logger?.LogDebug("Found {name}", result[0].Club);
                    }
                }
            }
            return result;
        }

        public override Player? GetById(string id)
        {
            string idField = "Id";
            return SelectById(id, idField);
        }

        public override Player? GetByFideId(ulong id)
        {
            string idField = "FideId";
            return SelectById($"{id}", idField);
        }

        private Player? SelectById(string id, string idField)
        {
            string federation = Key.ToString();
            using var cmd = connection?.CreateCommand();
            if (cmd == null) return null;
            cmd.CommandText = $"SELECT * FROM Player WHERE Federation = \"{federation}\" AND {idField} = \"{id}\"";
            logger?.LogDebug("{}", cmd.CommandText);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Player player = new($"{reader.GetString(0)}_{reader.GetString(1)}", reader.GetString(3))
                {
                    Federation = reader.GetString(0),
                    Club = reader.GetString(2),
                    Sex = (Sex)reader.GetInt16(4),
                    Rating = reader.GetInt32(5),
                    Inactive = reader.GetInt16(6) == 1,
                    YearOfBirth = reader.GetInt16(7),
                    FideId = (ulong)reader.GetInt64(8),
                    Title = FideTitle.NONE
                };
                logger?.LogDebug("Found {name}", player.Name);
                if (player.Club != null && player.Club != string.Empty)
                {
                    using var ccmd = connection?.CreateCommand();
                    if (ccmd != null)
                    {
                        ccmd.CommandText = $"SELECT Name FROM Club WHERE Federation = \"{federation}\" and Id = \"{player.Club ?? String.Empty}\"";
                        logger?.LogDebug("{}", cmd.CommandText);
                        using var creader = ccmd.ExecuteReader();
                        while (creader.Read())
                        {
                            player.Club = creader.GetString(0);
                            logger?.LogDebug("Found {name}", player.Club);
                        }
                    }
                }

                return player;
            }
            return null;
        }
    }

    public class NetherlandsPlayerBase : NationalPlayerBase
    {
        public override string Description => Strings.BaseDescription_NED;
        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.NED;

        public override async Task<bool> UpdateAsync()
        {
            if (connection == null) return false;
            ProgressUpdate(1, string.Format(PonzianiPlayerBase.Strings.DownloadingData, "https://schaakbond.nl"));
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var httpClient = new HttpClient();
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "ned");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "ned.zip");
            if (!File.Exists(tmpFile))
            {
                using var stream = await httpClient.GetStreamAsync("https://schaakbond.nl/sites/default/files/userfiles/schaken/rating/KNSB.zip");
                using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                await stream.CopyToAsync(fileStream);
            }
            ProgressUpdate(20, PonzianiPlayerBase.Strings.DownloadCompletedStartingDataProcessing);
            if (!Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".csv").Any())
            {
                ZipFile.ExtractToDirectory(tmpFile, tmpDir);
            }
            string file = Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".csv").First();
            if (file == null || file.Trim().Length == 0) return false;

            ProgressUpdate(30, PonzianiPlayerBase.Strings.StartingDatabaseUpdate);
            using (var transaction = connection.BeginTransaction())
            {
                using (var del = connection.CreateCommand())
                {
                    del.CommandText = "DELETE FROM Player WHERE Federation = \"NED\"";
                    await del.ExecuteNonQueryAsync();
                    del.CommandText = "DELETE FROM Club WHERE Federation = \"NED\"";
                    await del.ExecuteNonQueryAsync();
                }
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Player VALUES(@Federation, @Id, @Club, @Name, @Sex, @Rating, @Inactive, @Birthyear, @FideId)";
                    string[] parameters = ["@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId"];
                    foreach (var p in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = p;
                        cmd.Parameters.Add(parameter);
                    }

                    int count = 0;
                    using StreamReader reader = new(file, Encoding.UTF8);
                    CsvConfiguration config = new(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";"
                    };
                    using var csv = new CsvReader(reader, config);
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        try
                        {
                            int id = csv.GetField<int>(0);
                            if (id == 0) continue;
                            ++count;
                            cmd.Parameters["@Id"].Value = $"{id}";
                            cmd.Parameters["@Inactive"].Value = "0";
                            cmd.Parameters["@Name"].Value = csv.GetField<string>(1);
                            cmd.Parameters["@Sex"].Value = csv.GetField<string>(7)?.Trim() == "w" ? "1" : "0";
                            cmd.Parameters["@Federation"].Value = "NED";
                            cmd.Parameters["@Club"].Value = $"";
                            cmd.Parameters["@Rating"].Value = $"{(csv.GetField<string>(4)?.Length > 0 ? csv.GetField<int>(4) : 0)}";
                            cmd.Parameters["@Birthyear"].Value = $"{(csv.GetField<string>(6)?.Length > 0 ? csv.GetField<int>(6) : 0)}";
                            cmd.Parameters["@FideId"].Value = 0;
                            if (count == 1) await cmd.PrepareAsync();
                            await cmd.ExecuteNonQueryAsync();
                            if ((count % 100) == 0) ProgressUpdate((int)(30 + 65.0 * count / 20000), string.Format(PonzianiPlayerBase.Strings.CountPlayersWrittenToDatabase, count));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    ProgressUpdate((int)(30 + 65.0 * count / 20000), string.Format(PonzianiPlayerBase.Strings.CountPlayersProcessedCommitStarted, count));
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.NED}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                ProgressUpdate(98, PonzianiPlayerBase.Strings.Cleanup);
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            ProgressUpdate(100, PonzianiPlayerBase.Strings.Done);
            return true;
        }

    }

    public class CroatiaPlayerBase : NationalPlayerBase
    {
        public override string Description => Strings.BaseDescription_CRO;
        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.CRO;

        public override async Task<bool> UpdateAsync()
        {
            if (connection == null) return false;
            ProgressUpdate(1, string.Format(PonzianiPlayerBase.Strings.DownloadingData, "http://hrvatski-sahovski-savez.hr"));
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var httpClient = new HttpClient();
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "cro");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "cro.zip");
            if (!File.Exists(tmpFile))
            {
                using var stream = await httpClient.GetStreamAsync("http://hrvatski-sahovski-savez.hr/hr-rejting/cro_sm_nrl.zip");
                using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                await stream.CopyToAsync(fileStream);
            }
            ProgressUpdate(20, PonzianiPlayerBase.Strings.DownloadCompletedStartingDataProcessing);
            if (!Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".xls").Any())
            {
                ZipFile.ExtractToDirectory(tmpFile, tmpDir);
            }
            string file = Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".xls").First();
            if (file == null || file.Trim().Length == 0) return false;
            ProgressUpdate(30, PonzianiPlayerBase.Strings.StartingDatabaseUpdate);
            using (var transaction = connection.BeginTransaction())
            {
                using (var del = connection.CreateCommand())
                {
                    del.CommandText = "DELETE FROM Player WHERE Federation = \"CRO\"";
                    await del.ExecuteNonQueryAsync();
                    del.CommandText = "DELETE FROM Club WHERE Federation = \"CRO\"";
                    await del.ExecuteNonQueryAsync();
                }
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Player VALUES(@Federation, @Id, @Club, @Name, @Sex, @Rating, @Inactive, @Birthyear, @FideId)";
                    string[] parameters = ["@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId"];
                    foreach (var p in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = p;
                        cmd.Parameters.Add(parameter);
                    }

                    int count = 0;
                    using var stream = File.Open(file, FileMode.Open, FileAccess.Read);
                    using var reader = ExcelReaderFactory.CreateReader(stream);
                    //skip header rows
                    for (int i = 0; i < 1; i++) reader.Read();
                    do
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                ++count;
                                cmd.Parameters["@Id"].Value = $"{reader.GetString(1)}";
                                cmd.Parameters["@Inactive"].Value = "0";
                                cmd.Parameters["@Name"].Value = $"{reader.GetString(4).Trim()}";
                                cmd.Parameters["@Sex"].Value = reader.GetString(9) == "F" ? "1" : "0";
                                cmd.Parameters["@Federation"].Value = "CRO";
                                cmd.Parameters["@Club"].Value = reader.GetString(3) ?? string.Empty;
                                cmd.Parameters["@Rating"].Value = (int)reader.GetDouble(8);
                                cmd.Parameters["@Birthyear"].Value = int.Parse(reader.GetString(10)[6..].Trim());
                                cmd.Parameters["@FideId"].Value = reader.GetValue(0) != null ? (ulong)reader.GetDouble(0) : 0;
                                if (count == 1) await cmd.PrepareAsync();
                                await cmd.ExecuteNonQueryAsync();
                                if ((count % 100) == 0) ProgressUpdate((int)(30 + 65.0 * count / 20000), string.Format(PonzianiPlayerBase.Strings.CountPlayersWrittenToDatabase, count));
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    } while (reader.NextResult());
                    ProgressUpdate((int)(30 + 65.0 * count / 20000), string.Format(PonzianiPlayerBase.Strings.CountPlayersProcessedCommitStarted, count));
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.CRO}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                ProgressUpdate(98, PonzianiPlayerBase.Strings.Cleanup);
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            ProgressUpdate(100, PonzianiPlayerBase.Strings.Done);
            return true;
        }

    }


    public class ItalianPlayerBase : NationalPlayerBase
    {
        public override string Description => Strings.BaseDescription_ITA;
        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.ITA;

        public override async Task<bool> UpdateAsync()
        {
            if (connection == null) return false;
            ProgressUpdate(1, PonzianiPlayerBase.Strings.PreparingDownload);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var httpClient = new HttpClient();
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "ita");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "ita.csv");
            if (!File.Exists(tmpFile))
            {
                ProgressUpdate(2, string.Format(PonzianiPlayerBase.Strings.DownloadingData, "http://www.torneionline.com"));
                using var stream = await httpClient.GetStreamAsync("http://www.torneionline.com/dwn/allin.csv");
                using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                await stream.CopyToAsync(fileStream);
            }
            ProgressUpdate(20, PonzianiPlayerBase.Strings.DownloadCompletedStartingDataProcessing);
            string file = Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".csv").First();
            if (file == null || file.Trim().Length == 0) return false;

            ProgressUpdate(30, PonzianiPlayerBase.Strings.StartingDatabaseUpdate);
            using (var transaction = connection.BeginTransaction())
            {
                using (var del = connection.CreateCommand())
                {
                    del.CommandText = "DELETE FROM Player WHERE Federation = \"ITA\"";
                    await del.ExecuteNonQueryAsync();
                    del.CommandText = "DELETE FROM Club WHERE Federation = \"ITA\"";
                    await del.ExecuteNonQueryAsync();
                }
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Player VALUES(@Federation, @Id, @Club, @Name, @Sex, @Rating, @Inactive, @Birthyear, @FideId)";
                    string[] parameters = ["@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId"];
                    foreach (var p in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = p;
                        cmd.Parameters.Add(parameter);
                    }

                    int count = 0;
                    using StreamReader reader = new(file, Encoding.UTF8);
                    CsvConfiguration config = new(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";"

                    };
                    using var csv = new CsvReader(reader, config);
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        try
                        {
                            if (!csv.TryGetField<int>(6, out int id) || id == 0) continue;
                            ++count;
                            cmd.Parameters["@Id"].Value = $"{id}";
                            cmd.Parameters["@Inactive"].Value = csv.GetField<string>(18) == "N" ? "1" : "0";
                            cmd.Parameters["@Name"].Value = csv.GetField<string>(0);
                            cmd.Parameters["@Sex"].Value = csv.GetField<string>(7)?.Trim() == "F" ? "1" : "0";
                            cmd.Parameters["@Federation"].Value = "ITA";
                            cmd.Parameters["@Club"].Value = $"";
                            cmd.Parameters["@Rating"].Value = $"{csv.GetField<int>(1)}";
                            cmd.Parameters["@Birthyear"].Value = $"{csv.GetField<int>(5)}";
                            string fideid = csv.GetField<string>(8) ?? string.Empty;
                            if (fideid != null && ulong.TryParse(fideid, out ulong f))
                                cmd.Parameters["@FideId"].Value = f;
                            else
                                cmd.Parameters["@FideId"].Value = 0;
                            if (count == 1) await cmd.PrepareAsync();
                            await cmd.ExecuteNonQueryAsync();
                            if ((count % 100) == 0) ProgressUpdate((int)(30 + 65.0 * count / 100000), string.Format(PonzianiPlayerBase.Strings.CountPlayersWrittenToDatabase, count));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    ProgressUpdate((int)(30 + 65.0 * count / 100000), string.Format(PonzianiPlayerBase.Strings.CountPlayersProcessedCommitStarted, count));
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.ITA}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                ProgressUpdate(98, PonzianiPlayerBase.Strings.Cleanup);
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            ProgressUpdate(100, PonzianiPlayerBase.Strings.Done);
            return true;
        }

    }


    public class CzechPlayerBase : NationalPlayerBase
    {
        public override string Description => Strings.BaseDescription_CZE;
        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.CZE;

        public override async Task<bool> UpdateAsync()
        {
            if (connection == null) return false;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            ProgressUpdate(1, string.Format(PonzianiPlayerBase.Strings.DownloadingData, "http://elo.miramal.com"));
            var httpClient = new HttpClient();
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "cze");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "cze.xls");
            if (!File.Exists(tmpFile))
            {
                using var stream = await httpClient.GetStreamAsync("http://elo.miramal.com/download/lok_sm_cz.xls");
                using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                await stream.CopyToAsync(fileStream);
            }
            ProgressUpdate(20, PonzianiPlayerBase.Strings.DownloadCompletedStartingDataProcessing);
            string file = Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".xls").First();
            if (file == null || file.Trim().Length == 0) return false;

            ProgressUpdate(30, PonzianiPlayerBase.Strings.StartingDatabaseUpdate);
            using (var transaction = connection.BeginTransaction())
            {
                using (var del = connection.CreateCommand())
                {
                    del.CommandText = "DELETE FROM Player WHERE Federation = \"CZE\"";
                    await del.ExecuteNonQueryAsync();
                    del.CommandText = "DELETE FROM Club WHERE Federation = \"CZE\"";
                    await del.ExecuteNonQueryAsync();
                }
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Player VALUES(@Federation, @Id, @Club, @Name, @Sex, @Rating, @Inactive, @Birthyear, @FideId)";
                    string[] parameters = ["@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId"];
                    foreach (var p in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = p;
                        cmd.Parameters.Add(parameter);
                    }

                    int count = 0;
                    using var stream = File.Open(file, FileMode.Open, FileAccess.Read);
                    using var reader = ExcelReaderFactory.CreateReader(stream);
                    //skip header rows
                    for (int i = 0; i < 1; i++) reader.Read();
                    do
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                ++count;
                                cmd.Parameters["@Id"].Value = $"{reader.GetDouble(0)}";
                                cmd.Parameters["@Inactive"].Value = "0";
                                cmd.Parameters["@Name"].Value = $"{reader.GetString(1).Trim()}";
                                cmd.Parameters["@Sex"].Value = reader.GetString(4) == "f" ? "1" : "0";
                                cmd.Parameters["@Federation"].Value = "CZE";
                                cmd.Parameters["@Club"].Value = reader.GetString(2) ?? string.Empty;
                                cmd.Parameters["@Rating"].Value = (int)reader.GetDouble(5);
                                cmd.Parameters["@Birthyear"].Value = int.Parse(reader.GetString(3)[6..].Trim());
                                cmd.Parameters["@FideId"].Value = reader.GetValue(8) != null ? (ulong)reader.GetDouble(8) : 0;
                                if (count == 1) await cmd.PrepareAsync();
                                await cmd.ExecuteNonQueryAsync();
                                if ((count % 100) == 0) ProgressUpdate((int)(30 + 65.0 * count / 20000), string.Format(PonzianiPlayerBase.Strings.CountPlayersWrittenToDatabase, count));
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    } while (reader.NextResult());
                    ProgressUpdate((int)(30 + 65.0 * count / 20000), string.Format(PonzianiPlayerBase.Strings.CountPlayersProcessedCommitStarted, count));
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.CZE}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                ProgressUpdate(98, PonzianiPlayerBase.Strings.Cleanup);
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            ProgressUpdate(100, PonzianiPlayerBase.Strings.Done);
            return true;
        }

    }


    public class AustriaPlayerBase : NationalPlayerBase
    {
        public override string Description => Strings.BaseDescription_AUT;
        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.AUS;

        public override async Task<bool> UpdateAsync()
        {
            if (connection == null) return false;
            ProgressUpdate(1, string.Format(PonzianiPlayerBase.Strings.DownloadingData, "http://chess-results.com"));
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var httpClient = new HttpClient();
            string page = await httpClient.GetStringAsync("http://chess-results.com/OesbEloSuche.aspx");
            if (page == null || page.Trim().Length == 0) return false;
            var link = $"http://chess-results.com/{LinkFinder.Find(page).Find(l => l.Text.Contains("Download der Eloliste", StringComparison.CurrentCulture)).Href}";
            if (link == null || link.Trim().Length == 0) return false;
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "aut");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "aut.zip");

            if (!File.Exists(tmpFile))
            {
                using var stream = await httpClient.GetStreamAsync(link);
                using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                await stream.CopyToAsync(fileStream);
            }
            ProgressUpdate(20, PonzianiPlayerBase.Strings.DownloadCompletedStartingDataProcessing);
            if (!Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".xls").Any())
            {
                ZipFile.ExtractToDirectory(tmpFile, tmpDir);
            }
            string file = Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".xls").First();
            if (file == null || file.Trim().Length == 0) return false;

            ProgressUpdate(30, PonzianiPlayerBase.Strings.StartingDatabaseUpdate);
            using (var transaction = connection.BeginTransaction())
            {
                using (var del = connection.CreateCommand())
                {
                    del.CommandText = "DELETE FROM Player WHERE Federation = \"AUT\"";
                    await del.ExecuteNonQueryAsync();
                    del.CommandText = "DELETE FROM Club WHERE Federation = \"AUT\"";
                    await del.ExecuteNonQueryAsync();
                }
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Player VALUES(@Federation, @Id, @Club, @Name, @Sex, @Rating, @Inactive, @Birthyear, @FideId)";
                    string[] parameters = ["@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId"];
                    foreach (var p in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = p;
                        cmd.Parameters.Add(parameter);
                    }

                    int count = 0;
                    using var stream = File.Open(file, FileMode.Open, FileAccess.Read);
                    using var reader = ExcelReaderFactory.CreateReader(stream);
                    //skip header rows
                    for (int i = 0; i < 5; i++) reader.Read();
                    do
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                ++count;
                                int daysSince2000 = (reader.GetDateTime(11) - new DateTime(2000, 1, 1)).Days;
                                cmd.Parameters["@Id"].Value = $"{reader.GetDouble(0)}-{reader.GetDouble(1)}-{daysSince2000}";
                                cmd.Parameters["@Inactive"].Value = "0";
                                cmd.Parameters["@Name"].Value = $"{reader.GetString(2).Trim()}, {reader.GetString(3).Trim()}";
                                cmd.Parameters["@Sex"].Value = reader.GetString(5) == "w" ? "1" : "0";
                                cmd.Parameters["@Federation"].Value = "AUT";
                                cmd.Parameters["@Club"].Value = reader.GetString(22) ?? string.Empty;
                                cmd.Parameters["@Rating"].Value = (int)reader.GetDouble(7);
                                cmd.Parameters["@Birthyear"].Value = (int)reader.GetDouble(25);
                                cmd.Parameters["@FideId"].Value = (ulong)reader.GetDouble(16);
                                if (count == 1) await cmd.PrepareAsync();
                                await cmd.ExecuteNonQueryAsync();
                                if ((count % 100) == 0) ProgressUpdate((int)(30 + 65.0 * count / 20000), string.Format(PonzianiPlayerBase.Strings.CountPlayersWrittenToDatabase, count));
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    } while (reader.NextResult());
                    ProgressUpdate((int)(30 + 65.0 * count / 20000), string.Format(PonzianiPlayerBase.Strings.CountPlayersProcessedCommitStarted, count));
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.AUT}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                ProgressUpdate(98, PonzianiPlayerBase.Strings.Cleanup);
                transaction.Commit();
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            ProgressUpdate(100, PonzianiPlayerBase.Strings.Done);
            return true;
        }

    }

    public class AustraliaPlayerBase : NationalPlayerBase
    {
        public override string Description => Strings.BaseDescription_AUS;

        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.AUS;


        public override async Task<bool> UpdateAsync()
        {
            if (connection == null) return false;
            try
            {
                ProgressUpdate(1, string.Format(PonzianiPlayerBase.Strings.DownloadingData, "https://auschess.org.au"));
                var httpClient = new HttpClient();
                string page = await httpClient.GetStringAsync("https://auschess.org.au/rating-lists/");
                var links = LinkFinder.Find(page);
                int indx = page.IndexOf("Classic ACF Master File");
                var rl = links.Find(l => l.ToIndex < indx && l.ToIndex + 20 > indx);
                if (rl.Href == null || rl.Href.Trim().Length == 0) return false;
                string rating_data = await httpClient.GetStringAsync($"https://auschess.org.au{rl.Href}");
                ProgressUpdate(20, PonzianiPlayerBase.Strings.DownloadCompletedStartingDataProcessing);
                if (rating_data == null || rating_data.Length == 0) return false;
                string[] rating_data_lines = rating_data.Split(["\r\n", "\n", "\r"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                ProgressUpdate(30, PonzianiPlayerBase.Strings.StartingDatabaseUpdate);
                using var transaction = connection.BeginTransaction();
                using (var del = connection.CreateCommand())
                {
                    del.CommandText = "DELETE FROM Player WHERE Federation = \"AUS\"";
                    await del.ExecuteNonQueryAsync();
                    del.CommandText = "DELETE FROM Club WHERE Federation = \"AUS\"";
                    await del.ExecuteNonQueryAsync();
                }


                //Process player
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Player VALUES(@Federation, @Id, @Club, @Name, @Sex, @Rating, @Inactive, @Birthyear, @FideId)";
                    string[] parameters = ["@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId"];
                    foreach (var p in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = p;
                        cmd.Parameters.Add(parameter);
                    }

                    int count = 0;
                    for (int i = 1; i < rating_data_lines.Length; ++i)
                    {
                        string l = rating_data_lines[i].Trim();
                        try
                        {
                            ++count;
                            cmd.Parameters["@Id"].Value = l[..7];
                            cmd.Parameters["@Inactive"].Value = "0";
                            cmd.Parameters["@Name"].Value = l[24..];
                            cmd.Parameters["@Sex"].Value = "0";
                            cmd.Parameters["@Federation"].Value = "AUS";
                            cmd.Parameters["@Club"].Value = string.Empty;
                            string rs = l.Substring(9, 4);
                            if (int.TryParse(rs, out int rating))
                            {
                                cmd.Parameters["@Rating"].Value = rating;
                            }
                            else
                            {
                                cmd.Parameters["@Rating"].Value = 0;
                            }
                            cmd.Parameters["@Birthyear"].Value = 0;
                            cmd.Parameters["@FideId"].Value = 0;
                            if (count == 1) await cmd.PrepareAsync();
                            await cmd.ExecuteNonQueryAsync();
                            if ((count % 100) == 0) ProgressUpdate((int)(30 + 65.0 * count / 50000), string.Format(PonzianiPlayerBase.Strings.CountPlayersWrittenToDatabase, count));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    ProgressUpdate((int)(30 + 65.0 * count / 50000), string.Format(PonzianiPlayerBase.Strings.CountPlayersProcessedCommitStarted, count));
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.AUS}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                ProgressUpdate(98, PonzianiPlayerBase.Strings.Cleanup);
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
                ProgressUpdate(100, PonzianiPlayerBase.Strings.Done);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }
    }

    public class SuissePlayerBase : NationalPlayerBase
    {
        public override string Description => Strings.BaseDescription_SUI;

        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.SUI;


        public override async Task<bool> UpdateAsync()
        {
            ProgressUpdate(1, string.Format(PonzianiPlayerBase.Strings.DownloadingData, "http://adapter.swisschess.ch"));
            var httpClient = new HttpClient();
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "sui");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "sui.csv");
            try
            {
                if (!File.Exists(tmpFile))
                {
                    using var stream = await httpClient.GetStreamAsync("http://adapter.swisschess.ch/schachsport/fl/export.php?profile=swissmanager&output=csv");
                    using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                    await stream.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            ProgressUpdate(20, PonzianiPlayerBase.Strings.DownloadCompletedStartingDataProcessing);
            if (connection == null || !File.Exists(tmpFile)) return false;

            using (var transaction = connection.BeginTransaction())
            {
                using (var del = connection.CreateCommand())
                {
                    del.CommandText = "DELETE FROM Player WHERE Federation = \"SUI\"";
                    await del.ExecuteNonQueryAsync();
                    del.CommandText = "DELETE FROM Club WHERE Federation = \"SUI\"";
                    await del.ExecuteNonQueryAsync();
                }


                //Process player
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Player VALUES(@Federation, @Id, @Club, @Name, @Sex, @Rating, @Inactive, @Birthyear, @FideId)";
                    string[] parameters = ["@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId"];
                    foreach (var p in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = p;
                        cmd.Parameters.Add(parameter);
                    }

                    int count = 0;
                    using StreamReader reader = new(tmpFile, Encoding.Latin1);
                    CsvConfiguration config = new(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";"
                    };
                    using var csv = new CsvReader(reader, config);
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        try
                        {
                            ++count;
                            cmd.Parameters["@Id"].Value = $"{csv.GetField<string>(0)}";
                            cmd.Parameters["@Inactive"].Value = "0";
                            cmd.Parameters["@Name"].Value = csv.GetField<string>(1);
                            cmd.Parameters["@Sex"].Value = csv.GetField<string>(2)?.Length > 0 && csv.GetField<char>(2) == 'f' ? "1" : "0";
                            cmd.Parameters["@Federation"].Value = "SUI";
                            cmd.Parameters["@Club"].Value = $"{csv.GetField<string>(5)}";
                            cmd.Parameters["@Rating"].Value = $"{(csv.GetField<string>(7)?.Length > 0 ? csv.GetField<int>(7) : 0)}";
                            cmd.Parameters["@Birthyear"].Value = $"{(csv.GetField<string>(6)?.Length > 0 ? csv.GetField<int>(6) : 0)}";
                            string fideid = csv.GetField<string>(8) ?? string.Empty;
                            if (fideid != null && ulong.TryParse(fideid, out ulong f))
                                cmd.Parameters["@FideId"].Value = f;
                            else
                                cmd.Parameters["@FideId"].Value = 0;
                            if (count == 1) await cmd.PrepareAsync();
                            await cmd.ExecuteNonQueryAsync();
                            if ((count % 100) == 0) ProgressUpdate((int)(30 + 65.0 * count / 6000), string.Format(PonzianiPlayerBase.Strings.CountPlayersWrittenToDatabase, count));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    ProgressUpdate((int)(30 + 65.0 * count / 6000), string.Format(PonzianiPlayerBase.Strings.CountPlayersProcessedCommitStarted, count));
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.SUI}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                ProgressUpdate(98, PonzianiPlayerBase.Strings.Cleanup);
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            ProgressUpdate(100, PonzianiPlayerBase.Strings.Done);
            return true;
        }
    }

    public class EnglishPlayerBase : NationalPlayerBase
    {
        public override string Description => Strings.BaseDescription_ENG;

        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.ENG;


        public override async Task<bool> UpdateAsync()
        {
            ProgressUpdate(1, string.Format(PonzianiPlayerBase.Strings.DownloadingData, "https://www.ecfrating.org.uk"));
            var httpClient = new HttpClient();
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "ecf");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "ecf.csv");

            try
            {
                if (!File.Exists(tmpFile))
                {
                    using var stream = await httpClient.GetStreamAsync("https://www.ecfrating.org.uk/v2/new/api.php?v2/rating_list_csv");
                    using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                    await stream.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            ProgressUpdate(20, PonzianiPlayerBase.Strings.DownloadCompletedStartingDataProcessing);

            if (connection == null || !File.Exists(tmpFile)) return false;

            using (var transaction = connection.BeginTransaction())
            {
                using (var del = connection.CreateCommand())
                {
                    del.CommandText = "DELETE FROM Player WHERE Federation = \"ENG\"";
                    await del.ExecuteNonQueryAsync();
                    del.CommandText = "DELETE FROM Club WHERE Federation = \"ENG\"";
                    await del.ExecuteNonQueryAsync();
                }


                //Process player
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Player VALUES(@Federation, @Id, @Club, @Name, @Sex, @Rating, @Inactive, @Birthyear, @FideId)";
                    string[] parameters = ["@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId"];
                    foreach (var p in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = p;
                        cmd.Parameters.Add(parameter);
                    }

                    int count = 0;
                    using StreamReader reader = new(tmpFile, Encoding.Latin1);
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        try
                        {
                            if (csv.GetField<string>(6)?.Length <= 0) continue;
                            ++count;
                            cmd.Parameters["@Id"].Value = $"{csv.GetField<string>(0)}";
                            cmd.Parameters["@Inactive"].Value = "0";
                            cmd.Parameters["@Name"].Value = csv.GetField<string>(1);
                            cmd.Parameters["@Sex"].Value = csv.GetField<string>(4)?.Length > 0 && csv.GetField<char>(4) == 'F' ? "1" : "0";
                            cmd.Parameters["@Federation"].Value = "ENG";
                            cmd.Parameters["@Club"].Value = $"{csv.GetField<string>(19)}";
                            cmd.Parameters["@Rating"].Value = $"{csv.GetField<int>(6)}";
                            cmd.Parameters["@Birthyear"].Value = $"0000";
                            string fideid = csv.GetField<string>(3) ?? string.Empty;
                            if (fideid != null && ulong.TryParse(fideid, out ulong f))
                                cmd.Parameters["@FideId"].Value = f;
                            else
                                cmd.Parameters["@FideId"].Value = 0;
                            if (count == 1) await cmd.PrepareAsync();
                            await cmd.ExecuteNonQueryAsync();
                            if ((count % 100) == 0) ProgressUpdate((int)(30 + 65.0 * count / 25000), string.Format(PonzianiPlayerBase.Strings.CountPlayersWrittenToDatabase, count));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    ProgressUpdate((int)(30 + 65.0 * count / 25000), string.Format(PonzianiPlayerBase.Strings.CountPlayersProcessedCommitStarted, count));
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.ENG}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                ProgressUpdate(98, PonzianiPlayerBase.Strings.Cleanup);
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            ProgressUpdate(100, PonzianiPlayerBase.Strings.Done);
            return true;
        }
    }

    public class GermanPlayerBase : NationalPlayerBase
    {
        public override string Description => Strings.BaseDescription_GER;

        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.GER;

        public async override Task<bool> UpdateAsync()
        {
            ProgressUpdate(1, string.Format(PonzianiPlayerBase.Strings.DownloadingData, "https://dwz.svw.info"));
            var httpClient = new HttpClient();
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "dwz");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "dwz.zip");

            try
            {
                if (!File.Exists(tmpFile))
                {
                    using var stream = await httpClient.GetStreamAsync("https://dwz.svw.info/services/files/export/csv/LV-0-csv.zip");
                    using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                    await stream.CopyToAsync(fileStream);
                }
                ProgressUpdate(20, PonzianiPlayerBase.Strings.DownloadCompletedStartingDataProcessing);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            if (!Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".csv").Any())
            {
                ZipFile.ExtractToDirectory(tmpFile, tmpDir);
            }
            //Find txt-file
            string clubfile = Directory.GetFiles(tmpDir).Where(f => Path.GetFileName(f) == "vereine.csv").First();
            string playerfile = Directory.GetFiles(tmpDir).Where(f => Path.GetFileName(f) == "spieler.csv").First();

            if (connection == null || !File.Exists(clubfile) || !File.Exists(playerfile)) return false;

            ProgressUpdate(30, PonzianiPlayerBase.Strings.StartingDatabaseUpdate);
            using (var transaction = connection.BeginTransaction())
            {
                using (var del = connection.CreateCommand())
                {
                    del.CommandText = "DELETE FROM Player WHERE Federation = \"GER\"";
                    await del.ExecuteNonQueryAsync();
                    del.CommandText = "DELETE FROM Club WHERE Federation = \"GER\"";
                    await del.ExecuteNonQueryAsync();
                }

                //Process clubs
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Club VALUES(@Federation, @Id, @Name)";
                    string[] parameters = ["@Federation", "@Id", "@Name"];
                    foreach (var p in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = p;
                        cmd.Parameters.Add(parameter);
                    }

                    int count = 0;
                    using StreamReader reader = new(clubfile, Encoding.Latin1);
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        try
                        {
                            ++count;
                            cmd.Parameters["@Id"].Value = csv.GetField<string>(0);
                            cmd.Parameters["@Name"].Value = csv.GetField<string>(3);
                            cmd.Parameters["@Federation"].Value = "GER";
                            if (count == 1) await cmd.PrepareAsync();
                            await cmd.ExecuteNonQueryAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }

                //Process clubs
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Player VALUES(@Federation, @Id, @Club, @Name, @Sex, @Rating, @Inactive, @Birthyear, @FideId)";
                    string[] parameters = ["@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId"];
                    foreach (var p in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = p;
                        cmd.Parameters.Add(parameter);
                    }

                    int count = 0;
                    using StreamReader reader = new(playerfile, Encoding.Latin1);
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        try
                        {
                            ++count;
                            cmd.Parameters["@Id"].Value = $"{csv.GetField<string>(0)}-{csv.GetField<string>(1)}";
                            cmd.Parameters["@Inactive"].Value = csv.GetField<char>(2) == 'P' ? "1" : "0";
                            cmd.Parameters["@Name"].Value = csv.GetField<string>(3);
                            cmd.Parameters["@Sex"].Value = csv.GetField<char>(4) == 'W' ? "1" : "0";
                            cmd.Parameters["@Federation"].Value = "GER";
                            cmd.Parameters["@Club"].Value = $"{csv.GetField<string>(0)}";
                            cmd.Parameters["@Rating"].Value = $"{csv.GetField<int>(8)}";
                            if (csv.GetField<string>(6)?.Trim().Length > 0) cmd.Parameters["@Birthyear"].Value = $"{csv.GetField<int>(6)}";
                            string fideid = csv.GetField<string>(12) ?? string.Empty;
                            if (fideid != null && ulong.TryParse(fideid, out ulong f))
                                cmd.Parameters["@FideId"].Value = f;
                            else
                                cmd.Parameters["@FideId"].Value = 0;
                            if (count == 1) await cmd.PrepareAsync();
                            await cmd.ExecuteNonQueryAsync();
                            if ((count % 100) == 0) ProgressUpdate((int)(30 + 65.0 * count / 100000), string.Format(PonzianiPlayerBase.Strings.CountPlayersWrittenToDatabase, count));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    ProgressUpdate((int)(30 + 65.0 * count / 100000), string.Format(PonzianiPlayerBase.Strings.CountPlayersProcessedCommitStarted, count));
                }

                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.GER}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                ProgressUpdate(98, PonzianiPlayerBase.Strings.Cleanup);
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            ProgressUpdate(100, PonzianiPlayerBase.Strings.Done);
            return true;
        }
    }
}
