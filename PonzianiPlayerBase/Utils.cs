using System.Text.RegularExpressions;

namespace PonzianiPlayerBase
{
    internal struct LinkItem
    {
        public string Href;
        public string Text;
        public int FromIndex;
        public int ToIndex;

        public override readonly string ToString()
        {
            return Href + "\n\t" + Text;
        }
    }

    internal static partial class LinkFinder
    {
        public static List<LinkItem> Find(string file)
        {
            List<LinkItem> list = [];

            // 1.
            // Find all matches in file.
            MatchCollection m1 = RegexAnchor().Matches(file);

            // 2.
            // Loop over each match.
            foreach (Match m in m1.Cast<Match>())
            {
                string value = m.Groups[1].Value;
                LinkItem i = new()
                {
                    FromIndex = m.Index,
                    ToIndex = m.Index + m.Length
                };

                // 3.
                // Get href attribute.
                Match m2 = RegexLink().Match(value);
                if (m2.Success)
                {
                    i.Href = m2.Groups[1].Value;
                }

                // 4.
                // Remove inner tags from text.
                string t = RegexRemoveTags().Replace(value, "");
                i.Text = t;

                list.Add(i);
            }
            return list;
        }

        [GeneratedRegex(@"(<a.*?>.*?</a>)", RegexOptions.Singleline)]
        private static partial Regex RegexAnchor();
        [GeneratedRegex(@"href=\""(.*?)\""", RegexOptions.Singleline)]
        private static partial Regex RegexLink();
        [GeneratedRegex(@"\s*<.*?>\s*", RegexOptions.Singleline)]
        private static partial Regex RegexRemoveTags();
    }
}
