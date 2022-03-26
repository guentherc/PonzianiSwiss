using Microsoft.Data.Sqlite;
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

    public abstract class PlayerBase : IPlayerBase, IDisposable
    {
        protected string? filename;
        protected SqliteConnection? connection;
        public DateTime LastUpdate => DateTime.MinValue;

        public void Dispose()
        {
            if (connection != null)
                ((IDisposable)connection).Dispose();
            GC.SuppressFinalize(this);
        }

        public abstract List<Player> Find(string searchstring);

        public abstract Player? GetById(string id);

        public bool Initialize()
        {
            if (filename == null)
            {
                string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PonzianiPlayerBase");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                this.filename = Path.Combine(directory, "fideplayer.db");
            }
            Console.WriteLine($"Fideplayer File: {filename}");
            bool create = !File.Exists(filename);
            connection = new SqliteConnection($"Data Source={filename}");
            connection.Open();
            if (create)
            {
                foreach (string sql in SQLCreate)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
            }
            return true;
        }

        public abstract Task<bool> UpdateAsync();


        private static readonly string[] SQLCreate = new string[]
                {
                    @"CREATE TABLE ""FidePlayer"" ( ""Id"" INTEGER, ""Name"" TEXT, ""Federation"" TEXT, ""Title"" INTEGER, ""Sex"" INTEGER, ""Rating"" INTEGER, ""Inactive"" INTEGER, PRIMARY KEY(""Id"") )",
                    @"CREATE INDEX ""IndxName"" ON ""FidePlayer"" (""Name"" ASC )",
                    @"CREATE TABLE ""AdminData"" ( ""Id"" INTEGER NOT NULL UNIQUE, ""Name""	TEXT, ""LastUpdate"" INTEGER, PRIMARY KEY(""Id"" AUTOINCREMENT) )",
                    $"INSERT into AdminData (Id, Name, LastUpdate) values (\"0\", \"FIDE\", \"{DateTime.UtcNow.Ticks}\")"
                };
    }

      public class PlayerBaseFactory
    {
        public enum Base { FIDE }

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
            }
            return bases[b];
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