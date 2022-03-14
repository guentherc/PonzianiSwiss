using LiteDB;
using PonzianiSwissLib;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace PonzianiPlayerBase
{
    public interface IPlayerBase
    {
        DateTime LastUpdate { get; }
        Task<bool> UpdateAsync();

        bool Initialize();

        List<Player> Find(string searchstring);

        Player? GetById(string id);

    }

    public abstract class PlayerBase : IPlayerBase
    {
        protected string filename = "";
        protected LiteDatabase? db;
        protected const string PLAYER_COLL = "player";

        public DateTime LastUpdate => DateTime.MinValue;

        public List<Player> Find(string searchstring)
        {
            var col = db?.GetCollection<Player>(PLAYER_COLL);
            return col?.Query().Where(x => x.Name.Contains(searchstring, StringComparison.CurrentCultureIgnoreCase)).ToList() ?? new();
        }

        public Player? GetById(string id)
        {
            var col = db?.GetCollection<Player>(PLAYER_COLL);
            var list = col?.Query().Where(x => x.Id == id).ToList();
            return list?.Count > 0 ? list[0] : null;
        }

        public bool Initialize()
        {
            if (!File.Exists(filename)) return false;
            db = new LiteDatabase(filename);
            return true;
        }

        public abstract Task<bool> UpdateAsync();

    }

    public class PlayerBaseFactory
    {
        public enum Base { Fide }

        private static readonly Dictionary<string, IPlayerBase> bases = new();

        public static IPlayerBase Get(Base b)
        {
            string id = b.ToString().ToUpper();
            if (!bases.ContainsKey(id))
            {
                if (id == "FIDE")
                {
                    bases.Add(id, new FidePlayerBase());
                    bases[id].Initialize();

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

}