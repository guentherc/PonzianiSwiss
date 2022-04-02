using Microsoft.Data.Sqlite;
using PonzianiSwissLib;

namespace PonzianiPlayerBase
{
    public interface IPlayerBase
    {
        DateTime LastUpdate { get; }
        Task<bool> UpdateAsync();

        bool Initialize();

        List<Player> Find(string searchstring, int max = 0);

        Player? GetById(string id);

        string Description { get; }

        PlayerBaseFactory.Base Key { get; }

    }

    public abstract class PlayerBase : IPlayerBase, IDisposable
    {
        protected string? filename;
        protected SqliteConnection? connection;
        protected DateTime lastUpdate = DateTime.MinValue;
        public DateTime LastUpdate => lastUpdate;

        public abstract string Description { get; }
        public abstract PlayerBaseFactory.Base Key { get; }

        public void Dispose()
        {
            if (connection != null)
                ((IDisposable)connection).Dispose();
            GC.SuppressFinalize(this);
        }

        public abstract List<Player> Find(string searchstring, int max = 0);

        public abstract Player? GetById(string id);

        public virtual bool Initialize()
        {
            if (filename == null)
            {
                string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PonzianiPlayerBase");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                this.filename = Path.Combine(directory, "player.db");
            }
            Console.WriteLine($"Fideplayer File: {filename}");
            connection = new SqliteConnection($"Data Source={filename}");
            connection.Open();
            foreach (string sql in SQLCreate)
            {
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
            return true;
        }

        public abstract Task<bool> UpdateAsync();


        private static readonly string[] SQLCreate = new string[]
        {
            @"PRAGMA encoding = 'UTF-8'",
            @"CREATE TABLE IF NOT EXISTS ""FidePlayer"" ( ""Id"" INTEGER, ""Name"" TEXT, ""Federation"" TEXT, ""Title"" INTEGER, ""Sex"" INTEGER, ""Rating"" INTEGER, ""Inactive"" INTEGER, ""Birthyear"" INTEGER, PRIMARY KEY(""Id"") )",
            @"CREATE INDEX IF NOT EXISTS ""IndxName"" ON ""FidePlayer"" (""Name"" ASC )",
            @"CREATE TABLE IF NOT EXISTS ""AdminData"" ( ""Id"" INTEGER NOT NULL UNIQUE, ""Name""	TEXT, ""LastUpdate"" INTEGER, PRIMARY KEY(""Id"" AUTOINCREMENT) )",
            @"CREATE TABLE IF NOT EXISTS ""Club"" ( ""Federation"" TEXT NOT NULL, ""Id"" TEXT NOT NULL, ""Name"" TEXT NOT NULL, PRIMARY KEY(""Id"") )",
            @"CREATE TABLE IF NOT EXISTS ""Player"" ( ""Federation"" TEXT NOT NULL, ""Id"" TEXT NOT NULL, ""Club"" TEXT, ""Name"" TEXT NOT NULL, ""Sex"" INTEGER, ""Rating"" INTEGER, ""Inactive"" INTEGER, ""Birthyear"" INTEGER, ""FideId"" INTEGER, PRIMARY KEY(""Federation"",""Id""))",
            @"CREATE INDEX IF NOT EXISTS ""IndxName"" ON ""Player"" (""Name"" ASC )",
            $"INSERT OR IGNORE into AdminData (Id, Name, LastUpdate) values (\"0\", \"FIDE\", \"{DateTime.MinValue.Ticks}\")",
            $"INSERT OR IGNORE into AdminData (Id, Name, LastUpdate) values (\"1\", \"GER\", \"{DateTime.MinValue.Ticks}\")"
        };
    }

    public class PlayerBaseFactory
    {
        public enum Base { FIDE, GER }

        private static readonly Dictionary<Base, IPlayerBase> bases = new();

        public static IPlayerBase Get(Base b)
        {
            if (!bases.ContainsKey(b))
            {
                if (b == Base.FIDE)
                {
                    bases.Add(b, new FidePlayerBase());
                    bases[b].Initialize();

                }
                else if (b == Base.GER)
                {
                    bases.Add(b, new GermanPlayerBase());
                    bases[b].Initialize();
                }
            }
            return bases[b];
        }

        public static List<KeyValuePair<Base, string>> AvailableBases => new()
        {
            new KeyValuePair<Base, string>(Base.FIDE, Strings.BaseDescription_FIDE),
            new KeyValuePair<Base, string>(Base.GER, Strings.BaseDescription_GER)
        };
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

        public string? Club { get; set; }

        public ulong FideId { set; get; } = 0;
    }

}