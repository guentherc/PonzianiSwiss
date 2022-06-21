using System.Text.Json.Serialization;

namespace PonzianiSwissLib
{
    public class Participant
    {
        public enum AttributeKey { Birthdate, Address, Email  };

        public Participant()
        {
        }

        public Participant(string name, int rating = 0)
        {
            Name = name;
            FideRating = rating;
        }

        public Participant(string name, int fideRating, FideTitle title = FideTitle.NONE, string? participantId = null) : this(name, fideRating)
        {
            Title = title;
            ParticipantId = participantId;
        }

        /// <summary>
        /// Name of the Participant
        /// </summary>
        public string? Name { set; get; }
        /// <summary>
        /// Participant's Fide rating
        /// </summary>
        public int FideRating { set; get; }
        /// <summary>
        /// Alternative rating
        /// </summary>
        public int AlternativeRating { set; get; }

        /// <summary>
        /// Title (e.g. GM - Grand Master or WIM - female international master)
        /// </summary>
        public FideTitle Title { set; get; } = FideTitle.NONE;
        /// <summary>
        /// Fide ID (0 if not available)
        /// </summary>
        public ulong FideId { set; get; } = 0;
        public string? Federation { set; get; }

        public readonly static Participant BYE = new("Bye", 0, FideTitle.NONE, "0000");

        /// <summary>
        /// Participants Id within tournament (usually Seed)
        /// </summary>
        public string? ParticipantId { set; get; }

        /// <summary>
        /// Rank Id within tournament (current Rank)
        /// </summary>
        public string? RankId { set; get; }

        /// <summary>
        /// Active status per Round of the Participant. If null, then Participant is active in all rounds
        /// </summary>
        public bool[]? Active { set; get; }
        /// <summary>
        /// Rank within tournament
        /// </summary>
        public int Rank { set; get; }

        /// <summary>
        /// Club or team
        /// </summary>
        public string? Club { set; get; }

        /// <summary>
        /// Year of Birth
        /// </summary>
        public int YearOfBirth { set; get; } = 0;

        public Sex Sex { set; get; } = Sex.Male;

        /// <summary>
        /// Property bag to store additional attributes
        /// </summary>
        public Dictionary<AttributeKey, object> Attributes { set; get; } = new Dictionary<AttributeKey, object>();

        [JsonIgnore]
        public Scorecard? Scorecard { set; get; }
    }
}
