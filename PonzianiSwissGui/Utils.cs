using PonzianiPlayerBase;
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
    }
}
