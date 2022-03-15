using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiSwissLib
{
    public class Participant
    {
        public enum AttributeKey { Sex, Birthdate, Scorecard, Birthyear };

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
        /// Tournament rating
        /// </summary>
        public int TournamentRating => Math.Max(FideRating, AlternativeRating);

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
        /// Team or club for team scores
        /// </summary>
        public string? Team { set; get; }

        /// <summary>
        /// Participant is active and shall be paired. If false participant is currently taking a break or withdrew
        /// </summary>
        public bool Active { set; get; } = true;
        /// <summary>
        /// Rank within tournament
        /// </summary>
        public int Rank {  set; get; }

        /// <summary>
        /// Property bag to store additional attributes
        /// </summary>
        public Dictionary<AttributeKey, object> Attributes { set; get; } = new Dictionary<AttributeKey, object>();

        public Scorecard? Scorecard => Attributes.ContainsKey(AttributeKey.Scorecard)? (Scorecard) Attributes[AttributeKey.Scorecard] : null;
    }
}
