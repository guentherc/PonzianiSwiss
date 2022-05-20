using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
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
            List<Player> result = new();
            using var cmd = connection?.CreateCommand();
            if (cmd == null) return result;
            cmd.CommandText = max > 0 ? $"SELECT * FROM Player WHERE Federation = @fed AND Name LIKE @ss LIMIT { max }" : "SELECT * FROM Player WHERE Federation = @fed AND  Name LIKE @ss";
            cmd.Parameters.AddWithValue("@fed", federation);
            cmd.Parameters.AddWithValue("@ss", searchstring + '%');
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Player player = new($"{reader.GetString(0)}_{reader.GetString(1)}", reader.GetString(3));
                    player.Federation = reader.GetString(0);
                    player.Club = reader.GetString(2);
                    player.Title = (FideTitle)reader.GetInt32(3);
                    player.Sex = (Sex)reader.GetInt16(4);
                    player.Rating = reader.GetInt32(5);
                    player.Inactive = reader.GetInt16(6) == 1;
                    player.YearOfBirth = reader.GetInt16(7);
                    player.FideId = (ulong)reader.GetInt64(8);
                    result.Add(player);
                }
            }
            if (result.Count == 1)
            {
                using var ccmd = connection?.CreateCommand();
                if (ccmd != null)
                {
                    ccmd.CommandText = $"SELECT Name FROM Club WHERE Federation = \"GER\" and Id = \"{ result[0].Club }\"";
                    using var creader = ccmd.ExecuteReader();
                    while (creader.Read())
                    {
                        result[0].Club = creader.GetString(0);
                    }
                }
            }
            return result;
        }

        public override Player? GetById(string id)
        {
            string federation = Key.ToString();
            using var cmd = connection?.CreateCommand();
            if (cmd == null) return null;
            cmd.CommandText = $"SELECT * FROM Player WHERE Federation = \"{federation}\" AND Id = \"{id}\"";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Player player = new($"{reader.GetString(0)}_{reader.GetString(1)}", reader.GetString(3));
                player.Federation = reader.GetString(0);
                player.Club = reader.GetString(2);
                player.Title = (FideTitle)reader.GetInt32(3);
                player.Sex = (Sex)reader.GetInt16(4);
                player.Rating = reader.GetInt32(5);
                player.Inactive = reader.GetInt16(6) == 1;
                player.YearOfBirth = reader.GetInt16(7);
                player.FideId = (ulong)reader.GetInt64(8);
                return player;
            }
            return null;
        }

    }

    public class AustriaPlayerBase : NationalPlayerBase
    {
        public override string Description => Strings.BaseDescription_AUT;
        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.AUS;

        public override async Task<bool> UpdateAsync()
        {
            if (connection == null) return false;
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
            if (!Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".xls").Any())
            {
                ZipFile.ExtractToDirectory(tmpFile, tmpDir);
            }
            string file = Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".xls").First();
            if (file == null || file.Trim().Length == 0) return false;

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
                    string[] parameters = new[] { "@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId" };
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
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    } while (reader.NextResult());
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.AUT}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
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
            var httpClient = new HttpClient();
            string page = await httpClient.GetStringAsync("https://auschess.org.au/rating-lists/");
            var links = LinkFinder.Find(page);
            int indx = page.IndexOf("Classic ACF Master File");
            var rl = links.Find(l => l.ToIndex < indx && l.ToIndex + 20 > indx);
            if (rl.Href == null || rl.Href.Trim().Length == 0) return false;
            string rating_data = await httpClient.GetStringAsync($"https://auschess.org.au{rl.Href}");
            if (rating_data == null || rating_data.Length == 0) return false;
            string[] rating_data_lines = rating_data.Split(new String[] { "\r\n", "\n", "\r" }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            using (var transaction = connection.BeginTransaction())
            {
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
                    string[] parameters = new[] { "@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId" };
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
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.AUS}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
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
            var httpClient = new HttpClient();
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "sui");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "sui.csv");

            if (!File.Exists(tmpFile))
            {
                using var stream = await httpClient.GetStreamAsync("http://adapter.swisschess.ch/schachsport/fl/export.php?profile=swissmanager&output=csv");
                using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                await stream.CopyToAsync(fileStream);
            }

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
                    string[] parameters = new[] { "@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId" };
                    foreach (var p in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = p;
                        cmd.Parameters.Add(parameter);
                    }

                    int count = 0;
                    using StreamReader reader = new(tmpFile, Encoding.Latin1);
                    CsvConfiguration config = new(CultureInfo.InvariantCulture);
                    config.Delimiter = ";";
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
                            cmd.Parameters["@Sex"].Value = csv.GetField<string>(2).Length > 0 && csv.GetField<char>(2) == 'f' ? "1" : "0";
                            cmd.Parameters["@Federation"].Value = "SUI";
                            cmd.Parameters["@Club"].Value = $"{csv.GetField<string>(5)}";
                            cmd.Parameters["@Rating"].Value = $"{(csv.GetField<string>(7).Length > 0 ? csv.GetField<int>(7) : 0)}";
                            cmd.Parameters["@Birthyear"].Value = $"{(csv.GetField<string>(6).Length > 0 ? csv.GetField<int>(6) : 0)}";
                            string fideid = csv.GetField<string>(8);
                            if (fideid != null && ulong.TryParse(fideid, out ulong f))
                                cmd.Parameters["@FideId"].Value = f;
                            else
                                cmd.Parameters["@FideId"].Value = 0;
                            if (count == 1) await cmd.PrepareAsync();
                            await cmd.ExecuteNonQueryAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.SUI}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            return true;
        }
    }

    public class EnglishPlayerBase : NationalPlayerBase
    {
        public override string Description => Strings.BaseDescription_ENG;

        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.ENG;


        public override async Task<bool> UpdateAsync()
        {
            var httpClient = new HttpClient();
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "ecf");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "ecf.csv");

            if (!File.Exists(tmpFile))
            {
                using var stream = await httpClient.GetStreamAsync("https://www.ecfrating.org.uk/v2/new/api.php?v2/rating_list_csv");
                using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                await stream.CopyToAsync(fileStream);
            }

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
                    string[] parameters = new[] { "@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId" };
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
                            if (csv.GetField<string>(6).Length <= 0) continue;
                            ++count;
                            cmd.Parameters["@Id"].Value = $"{csv.GetField<string>(0)}";
                            cmd.Parameters["@Inactive"].Value = "0";
                            cmd.Parameters["@Name"].Value = csv.GetField<string>(1);
                            cmd.Parameters["@Sex"].Value = csv.GetField<string>(4).Length > 0 && csv.GetField<char>(4) == 'F' ? "1" : "0";
                            cmd.Parameters["@Federation"].Value = "ENG";
                            cmd.Parameters["@Club"].Value = $"{csv.GetField<string>(19)}";
                            cmd.Parameters["@Rating"].Value = $"{csv.GetField<int>(6)}";
                            cmd.Parameters["@Birthyear"].Value = $"0000";
                            string fideid = csv.GetField<string>(3);
                            if (fideid != null && ulong.TryParse(fideid, out ulong f))
                                cmd.Parameters["@FideId"].Value = f;
                            else
                                cmd.Parameters["@FideId"].Value = 0;
                            if (count == 1) await cmd.PrepareAsync();
                            await cmd.ExecuteNonQueryAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.ENG}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            return true;
        }
    }

    public class GermanPlayerBase : NationalPlayerBase
    {
        public override string Description => Strings.BaseDescription_GER;

        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.GER;

        public async override Task<bool> UpdateAsync()
        {
            var httpClient = new HttpClient();
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "dwz");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "dwz.zip");

            if (!File.Exists(tmpFile))
            {
                using var stream = await httpClient.GetStreamAsync("https://dwz.svw.info/services/files/export/csv/LV-0-csv.zip");
                using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                await stream.CopyToAsync(fileStream);
            }
            if (!Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".csv").Any())
            {
                ZipFile.ExtractToDirectory(tmpFile, tmpDir);
            }
            //Find txt-file
            string clubfile = Directory.GetFiles(tmpDir).Where(f => Path.GetFileName(f) == "vereine.csv").First();
            string playerfile = Directory.GetFiles(tmpDir).Where(f => Path.GetFileName(f) == "spieler.csv").First();

            if (connection == null || !File.Exists(clubfile) || !File.Exists(playerfile)) return false;

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
                    string[] parameters = new[] { "@Federation", "@Id", "@Name" };
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
                    string[] parameters = new[] { "@Federation", "@Id", "@Club", "@Name", "@Sex", "@Rating", "@Inactive", "@Birthyear", "@FideId" };
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
                            cmd.Parameters["@Birthyear"].Value = $"{csv.GetField<int>(6)}";
                            string fideid = csv.GetField<string>(12);
                            if (fideid != null && ulong.TryParse(fideid, out ulong f))
                                cmd.Parameters["@FideId"].Value = f;
                            else
                                cmd.Parameters["@FideId"].Value = 0;
                            if (count == 1) await cmd.PrepareAsync();
                            await cmd.ExecuteNonQueryAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE AdminData SET LastUpdate = \"{DateTime.UtcNow.Ticks}\" where Id = \"{(int)PlayerBaseFactory.Base.GER}\"";
                lastUpdate = DateTime.UtcNow;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                command = connection.CreateCommand();
                command.CommandText = $"VACUUM";
                await command.ExecuteNonQueryAsync();
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            return true;
        }
    }
}
