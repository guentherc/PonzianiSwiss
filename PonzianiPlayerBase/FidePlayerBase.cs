using LiteDB;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PonzianiPlayerBase
{
    public class FidePlayerBase : PlayerBase
    {
        public FidePlayerBase(string? filename = null)
        {
            if (filename == null)
            {
                string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PonzianiPlayerBase");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                this.filename = Path.Combine(directory, "fideplayer.db");
            }
            else this.filename = filename;
            Console.WriteLine($"Fideplayer File: {filename}");
        }

        public override async Task<bool> UpdateAsync()
        {
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
            if (!Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".txt").Any())
            {
                ZipFile.ExtractToDirectory(tmpFile, tmpDir);
            }
            //Find txt-file
            string file = Directory.GetFiles(tmpDir).Where(f => Path.GetExtension(f) == ".txt").First();
            Console.WriteLine($"Data saved to {file}");

            var col = db?.GetCollection<Player>(PLAYER_COLL);
            col?.DeleteAll();
            int count = 0;
            using (StreamReader reader = new(file))
            {
                string? line;
                List<Player> list = new();

                while ((line = reader.ReadLine()) != null)
                {
                    ++count;
                    if (count == 1) continue;
                    string id = line[..15].Trim();
                    Player player = new(id);
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
                    list.Add(player);
                    if (count % 4096 == 0)
                    {
                        col?.InsertBulk(list);
                        list.Clear();
                    }
                }
                col?.InsertBulk(list);
                reader.Close();
            }
            col?.EnsureIndex(x => x.Name);
            Console.WriteLine($"{count} records processed!");


            //Clean up
            foreach (string f in Directory.GetFiles(tmpDir)) File.Delete(f);
            Directory.Delete(tmpDir);
            return true;
        }





    }
}
