using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace PonzianiPlayerBase
{

    public enum Sex { Male, Female }

    /// <summary>
    /// Chess title 
    /// </summary>
    public enum FideTitle
    {
        /// <summary>
        /// Grandmaster
        /// </summary>
        GM,
        /// <summary>
        /// Woman Grandmaster
        /// </summary>
        WGM,
        /// <summary>
        /// International Master
        /// </summary>
        IM,
        /// <summary>
        /// Woman International Master
        /// </summary>
        WIM,
        /// <summary>
        /// Fide Master
        /// </summary>
        FM,
        /// <summary>
        /// Woman Fide Master
        /// </summary>
        WFM,
        /// <summary>
        /// Candidate master
        /// </summary>
        CM,
        /// <summary>
        /// Woman Candidate master
        /// </summary>
        WCM,
        /// <summary>
        /// Woman Honarary Grandmaster
        /// </summary>
        WH,
        /// <summary>
        /// Untitled
        /// </summary>
        NONE
    };

    public interface IPlayerBase
    {
        DateTime LastUpdate { get; }
        Task<bool> UpdateAsync();

        Task<bool> InitializeAsync();

        List<Player> Find(string searchstring);

        Player? GetById(string id);

    }

    public abstract class PlayerBase : IPlayerBase
    {
        protected string filename = "";
        protected Data data = new();

        public DateTime LastUpdate => data != null ? data.LastUpdate : DateTime.MinValue;

        public List<Player> Find(string searchstring)
        {
            return data.Player.Where(e => e.Value.Name.Contains(searchstring, StringComparison.InvariantCultureIgnoreCase)).Select(e => e.Value).ToList();
        }

        public Player? GetById(string id)
        {
            return data != null && data.Player.ContainsKey(id) ? data.Player[id] : null;
        }

        public async Task<bool> InitializeAsync()
        {
            if (!File.Exists(filename)) return false;
            var bytes = await File.ReadAllBytesAsync(filename);
            string json = Encoding.Unicode.GetString(await Brotli.DecompressBytesAsync(bytes));
            if (json != null)
            {
                data = JsonSerializer.Deserialize<Data>(json) ?? new();
                return data.Player.Count > 0;
            }
            else return false;
        }

        protected async Task SaveData()
        {
            data.LastUpdate = DateTime.Now;
            var bytes = await Brotli.CompressBytesAsync(Encoding.Unicode.GetBytes(JsonSerializer.Serialize<Data>(data)));
            await File.WriteAllBytesAsync(filename, bytes);
        }

        public abstract Task<bool> UpdateAsync();
    }

    public class PlayerBaseFactory
    {
        private static Dictionary<string, IPlayerBase> bases = new Dictionary<string, IPlayerBase>();

        public static async Task<IPlayerBase> GetAsync(string id)
        {
            if (!bases.ContainsKey(id))
            {
                if (id == "FIDE")
                {
                    bases.Add(id, new FidePlayerBase());
                    await bases[id].InitializeAsync();

                }
            }
            return bases[id];
        }
    }

    public class Player
    {
        public Player(string id, string name = "")
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string? Federation { get; set; }

        public Sex Sex { get; set; }

        public FideTitle Title { get; set; } = FideTitle.NONE;
        public int YearOfBirth { get; set; } = 0;

        public bool Inactive { get; set; } = false;

        public int Rating { get; set; } = 0;

        public int RatingRapid { get; set; } = 0;

        public int RatingBlitz { get; set; } = 0;
    }

    public class Data
    {
        public Dictionary<string, Player> Player { set; get; } = new();
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
    }

    /// <summary>
    /// Copied from https://www.prowaretech.com/Computer/DotNet/BrotliStream
    /// </summary>
    internal class Brotli
    {
        public static byte[] CompressBytes(byte[] bytes)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var compressStream = new BrotliStream(outputStream, CompressionLevel.Optimal))
                {
                    compressStream.Write(bytes, 0, bytes.Length);
                }
                return outputStream.ToArray();
            }
        }

        public static byte[] DecompressBytes(byte[] bytes)
        {
            using (var inputStream = new MemoryStream(bytes))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var decompressStream = new BrotliStream(inputStream, CompressionMode.Decompress))
                    {
                        decompressStream.CopyTo(outputStream);
                    }
                    return outputStream.ToArray();
                }
            }
        }

        public static async Task<byte[]> CompressBytesAsync(byte[] bytes, CancellationToken cancel = default(CancellationToken))
        {
            using (var outputStream = new MemoryStream())
            {
                using (var compressStream = new BrotliStream(outputStream, CompressionLevel.Fastest))
                {
                    await compressStream.WriteAsync(bytes, 0, bytes.Length, cancel);
                }
                return outputStream.ToArray();
            }
        }

        public static async Task<byte[]> DecompressBytesAsync(byte[] bytes, CancellationToken cancel = default(CancellationToken))
        {
            using (var inputStream = new MemoryStream(bytes))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var decompressStream = new BrotliStream(inputStream, CompressionMode.Decompress))
                    {
                        await decompressStream.CopyToAsync(outputStream, cancel);
                    }
                    return outputStream.ToArray();
                }
            }
        }
    }
}