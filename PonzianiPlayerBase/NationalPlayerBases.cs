﻿using CsvHelper;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiPlayerBase
{
    public class GermanPlayerBase : PlayerBase
    {
        public override string Description => Strings.BaseDescription_GER;

        public override PlayerBaseFactory.Base Key => PlayerBaseFactory.Base.GER;

        public override List<Player> Find(string searchstring, int max = 0)
        {
            List<Player> result = new();
            using var cmd = connection?.CreateCommand();
            if (cmd == null) return result;
            cmd.CommandText = max > 0 ? $"SELECT * FROM Player WHERE Name LIKE @ss LIMIT { max }" : "SELECT * FROM Player WHERE Name LIKE @ss";
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
            using var cmd = connection?.CreateCommand();
            if (cmd == null) return null;
            cmd.CommandText = $"SELECT * FROM Player WHERE Federation = \"GER\" AND Id = \"{id}\"";
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
            }

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            return true;
        }
    }
}