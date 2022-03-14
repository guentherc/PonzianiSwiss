using PonzianiPlayerBase;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiSwissGui
{
    internal class Utils
    {
        public static void PrepareFederationComboBox(ComboBox cbFederation)
        {
            List<KeyValuePair<string, string>> fedl = new();
            foreach (var fed in FederationUtil.Federations.OrderBy(e => e.Key))
            {
                fedl.Add(new(fed.Key, $"{fed.Key} {fed.Value}"));
            }
            cbFederation.DataSource = fedl;
            cbFederation.DisplayMember = "Value";
            cbFederation.ValueMember = "Key";
        }

        public static void PrepareTitelComboBox(ComboBox cbTitle)
        {
            List<KeyValuePair<FideTitle, string>> titl = new();
            foreach (var t in Enum.GetValues<FideTitle>())
            {
                titl.Add(new(t, t.ToString()));
            }
            cbTitle.DataSource = titl;
            cbTitle.DisplayMember = "Value";
            cbTitle.ValueMember = "Key";
        }
    }
}
