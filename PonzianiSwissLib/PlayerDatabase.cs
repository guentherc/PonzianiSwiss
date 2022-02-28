using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiSwissLib
{
    public class PlayerDatabase
    {
    }

    public class FidePlayerContext : DbContext
    {
        public FidePlayerContext(string dbname = "player.db")
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, dbname);
            Console.WriteLine(DbPath);
        }

        public string DbPath { get; }
        public DbSet<FidePlayer>? Player { get; set; }
        public DbSet<AdministrationData>? AdminData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");

        public async Task<int> UpdateAsync()
        {
            if (Player == null) return 0;
            var httpClient = new HttpClient();
            var tmpDir = Path.GetTempPath();
            tmpDir = Path.Combine(tmpDir, "ratings");
            Directory.CreateDirectory(tmpDir);
            var tmpFile = Path.Combine(tmpDir, "ratings.zip");

            if (!File.Exists(tmpFile))
            {

                using var stream = await httpClient.GetStreamAsync("http://ratings.fide.com/download/players_list.zip");
                using var fileStream = new FileStream(tmpFile, FileMode.CreateNew);
                await stream.CopyToAsync(fileStream);
            }
            if (!Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".txt").Any()) ZipFile.ExtractToDirectory(tmpFile, tmpDir);
            //Find txt-file
            string file = Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".txt").First();
            Console.WriteLine($"Data saved to {file}");

            int count = 0;
            using (StreamReader reader = new(file))
            {
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    ++count;
                    if (count == 1) continue;
                    ulong id = ulong.Parse(line[..15].Trim());
                    FidePlayer player = Player?.Find(id) ?? new();
                    if (player.Id == 0)
                    {
                        player.Id = id;
                        Player?.Add(player);
                    }
                    player.Name = line.Substring(15, 61).Trim();
                    player.Federation = line.Substring(76, 4).Trim();
                    player.Sex = line[80] == 'F' ? Sex.Female : Sex.Male;
                    string title = line.Substring(84, 3).Trim();
                    if (title.Length > 0) player.Title = Enum.Parse<FideTitle>(title);
                    string rating = line.Substring(113, 4).Trim();
                    if (rating.Length > 0) player.Rating = int.Parse(rating);
                    rating = line.Substring(126, 4).Trim();
                    if (rating.Length > 0) player.RatingRapid = int.Parse(rating);
                    rating = line.Substring(139, 4).Trim();
                    if (rating.Length > 0) player.RatingBlitz = int.Parse(rating);
                    string year = line.Substring(152, 4).Trim();
                    if (year.Length > 0) player.YearOfBirth = int.Parse(year);
                    string flags = line.Substring(158, 4).Trim();
                    player.Inactive = flags.Contains('i');
                }
                reader.Close();
            }
            Console.WriteLine($"{count} records processed!");
            await UpdateDateAsync();
            count = await SaveChangesAsync();

            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);

            return count;
        }

        public async Task<DateTime> GetUpdateDateAsync()
        {
            if (AdminData == null) return DateTime.MinValue;
            else
            {
                var ad = await AdminData.Where(e => e.Key == AdministrationData.AttributeKey.LastUpdate).FirstOrDefaultAsync();
                return ad != null ? DateTime.Parse(ad.Value, CultureInfo.InvariantCulture) : DateTime.MinValue;
            }
        }

        public async Task<bool> SetUpdateDateAsync()
        {
            await UpdateDateAsync();
            return await SaveChangesAsync() != 0;
        }

        private async Task UpdateDateAsync()
        {
            if (AdminData == null) throw new Exception($"Database not initialized!");
            var ad = await AdminData.Where(e => e.Key == AdministrationData.AttributeKey.LastUpdate).FirstOrDefaultAsync();
            if (ad == null)
                Add(new AdministrationData(AdministrationData.AttributeKey.LastUpdate, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)));
            else ad.Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class AdministrationData
    {
        public enum AttributeKey { LastUpdate };
        public AdministrationData(AttributeKey key, string value)
        {
            Key = key;
            Value = value;
        }

        [Key]
        public AttributeKey Key { get; set; }
        public string Value { get; set; }
    }

    public class FidePlayer
    {
        [Key]
        public ulong Id { get; set; }
        public string? Name { get; set; }
        public string? Federation { get; set; }
        public Sex Sex { get; set; } 
        public FideTitle Title { get; set; } = FideTitle.NONE;

        public int YearOfBirth { get; set; } = 0;

        public bool Inactive { get; set; } = false;

        public int Rating { get; set; } = 0;

        public int RatingRapid { get; set; } = 0;

        public int RatingBlitz { get; set; } = 0;
    }
}
