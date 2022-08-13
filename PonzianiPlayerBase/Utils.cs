using System.Text.RegularExpressions;

namespace PonzianiPlayerBase
{
    internal struct LinkItem
    {
        public string Href;
        public string Text;
        public int FromIndex;
        public int ToIndex;

        public override string ToString()
        {
            return Href + "\n\t" + Text;
        }
    }

    internal static class LinkFinder
    {
        public static List<LinkItem> Find(string file)
        {
            List<LinkItem> list = new();

            // 1.
            // Find all matches in file.
            MatchCollection m1 = Regex.Matches(file, @"(<a.*?>.*?</a>)",
                RegexOptions.Singleline);

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
                Match m2 = Regex.Match(value, @"href=\""(.*?)\""",
                    RegexOptions.Singleline);
                if (m2.Success)
                {
                    i.Href = m2.Groups[1].Value;
                }

                // 4.
                // Remove inner tags from text.
                string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
                    RegexOptions.Singleline);
                i.Text = t;

                list.Add(i);
            }
            return list;
        }
    }
}
