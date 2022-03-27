using PonzianiPlayerBase;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;

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

    public static class FederationUtil
    {
        public readonly static Dictionary<string, string> Federations = new()
        {
            { "ACF", Properties.Federations.ACF },
            { "AHO", Properties.Federations.AHO },
            { "ALB", Properties.Federations.ALB },
            { "ALG", Properties.Federations.ALG },
            { "AND", Properties.Federations.AND },
            { "ANG", Properties.Federations.ANG },
            { "ANT", Properties.Federations.ANT },
            { "ARG", Properties.Federations.ARG },
            { "ARM", Properties.Federations.ARM },
            { "ARU", Properties.Federations.ARU },
            { "AUS", Properties.Federations.AUS },
            { "AUT", Properties.Federations.AUT },
            { "AZE", Properties.Federations.AZE },
            { "BAH", Properties.Federations.BAH },
            { "BAN", Properties.Federations.BAN },
            { "BAR", Properties.Federations.BAR },
            { "BDI", Properties.Federations.BDI },
            { "BEL", Properties.Federations.BEL },
            { "BEN", Properties.Federations.BEN },
            { "BHU", Properties.Federations.BHU },
            { "BIH", Properties.Federations.BIH },
            { "BLR", Properties.Federations.BLR },
            { "BOL", Properties.Federations.BOL },
            { "BOT", Properties.Federations.BOT },
            { "BRA", Properties.Federations.BRA },
            { "BRN", Properties.Federations.BRN },
            { "BRU", Properties.Federations.BRU },
            { "BUL", Properties.Federations.BUL },
            { "BUR", Properties.Federations.BUR },
            { "CAM", Properties.Federations.CAM },
            { "CAN", Properties.Federations.CAN },
            { "CAT", Properties.Federations.CAT },
            { "CCA", Properties.Federations.CCA },
            { "CHI", Properties.Federations.CHI },
            { "CHN", Properties.Federations.CHN },
            { "CIV", Properties.Federations.CIV },
            { "CMR", Properties.Federations.CMR },
            { "COL", Properties.Federations.COL },
            { "CPV", Properties.Federations.CPV },
            { "CRC", Properties.Federations.CRC },
            { "CRO", Properties.Federations.CRO },
            { "CUB", Properties.Federations.CUB },
            { "CYP", Properties.Federations.CYP },
            { "CZE", Properties.Federations.CZE },
            { "DEN", Properties.Federations.DEN },
            { "DJI", Properties.Federations.DJI },
            { "DOM", Properties.Federations.DOM },
            { "ECU", Properties.Federations.ECU },
            { "EGY", Properties.Federations.EGY },
            { "ENG", Properties.Federations.ENG },
            { "ESA", Properties.Federations.ESA },
            { "ESP", Properties.Federations.ESP },
            { "EST", Properties.Federations.EST },
            { "ETH", Properties.Federations.ETH },
            { "FAI", Properties.Federations.FAI },
            { "FID", Properties.Federations.FID },
            { "FIJ", Properties.Federations.FIJ },
            { "FIN", Properties.Federations.FIN },
            { "FRA", Properties.Federations.FRA },
            { "GAM", Properties.Federations.GAM },
            { "GCI", Properties.Federations.GCI },
            { "GEO", Properties.Federations.GEO },
            { "GER", Properties.Federations.GER },
            { "GHA", Properties.Federations.GHA },
            { "GRE", Properties.Federations.GRE },
            { "GUA", Properties.Federations.GUA },
            { "GUM", Properties.Federations.GUM },
            { "HAI", Properties.Federations.HAI },
            { "HKG", Properties.Federations.HKG },
            { "HON", Properties.Federations.HON },
            { "HUN", Properties.Federations.HUN },
            { "IMN", Properties.Federations.IMN },
            { "INA", Properties.Federations.INA },
            { "IND", Properties.Federations.IND },
            { "IRI", Properties.Federations.IRI },
            { "IRL", Properties.Federations.IRL },
            { "IRQ", Properties.Federations.IRQ },
            { "ISL", Properties.Federations.ISL },
            { "ISR", Properties.Federations.ISR },
            { "ISV", Properties.Federations.ISV },
            { "ITA", Properties.Federations.ITA },
            { "IVB", Properties.Federations.IVB },
            { "JAM", Properties.Federations.JAM },
            { "JCI", Properties.Federations.JCI },
            { "JOR", Properties.Federations.JOR },
            { "JPN", Properties.Federations.JPN },
            { "KAZ", Properties.Federations.KAZ },
            { "KEN", Properties.Federations.KEN },
            { "KGZ", Properties.Federations.KGZ },
            { "KOR", Properties.Federations.KOR },
            { "KOS", Properties.Federations.KOS },
            { "KSA", Properties.Federations.KSA },
            { "KUW", Properties.Federations.KUW },
            { "LAO", Properties.Federations.LAO },
            { "LAT", Properties.Federations.LAT },
            { "LBA", Properties.Federations.LBA },
            { "LBN", Properties.Federations.LBN },
            { "LBR", Properties.Federations.LBR },
            { "LCA", Properties.Federations.LCA },
            { "LES", Properties.Federations.LES },
            { "LIE", Properties.Federations.LIE },
            { "LTU", Properties.Federations.LTU },
            { "LUX", Properties.Federations.LUX },
            { "MAC", Properties.Federations.MAC },
            { "MAD", Properties.Federations.MAD },
            { "MAR", Properties.Federations.MAR },
            { "MAS", Properties.Federations.MAS },
            { "MAW", Properties.Federations.MAW },
            { "MDA", Properties.Federations.MDA },
            { "MDV", Properties.Federations.MDV },
            { "MEX", Properties.Federations.MEX },
            { "MGL", Properties.Federations.MGL },
            { "MKD", Properties.Federations.MKD },
            { "MLI", Properties.Federations.MLI },
            { "MLT", Properties.Federations.MLT },
            { "MNC", Properties.Federations.MNC },
            { "MNE", Properties.Federations.MNE },
            { "MOZ", Properties.Federations.MOZ },
            { "MRI", Properties.Federations.MRI },
            { "MTN", Properties.Federations.MTN },
            { "MYA", Properties.Federations.MYA },
            { "NAM", Properties.Federations.NAM },
            { "NCA", Properties.Federations.NCA },
            { "NED", Properties.Federations.NED },
            { "NEP", Properties.Federations.NEP },
            { "NGR", Properties.Federations.NGR },
            { "NOR", Properties.Federations.NOR },
            { "NZL", Properties.Federations.NZL },
            { "OMA", Properties.Federations.OMA },
            { "PAK", Properties.Federations.PAK },
            { "PAN", Properties.Federations.PAN },
            { "PAR", Properties.Federations.PAR },
            { "PER", Properties.Federations.PER },
            { "PHI", Properties.Federations.PHI },
            { "PLE", Properties.Federations.PLE },
            { "PLW", Properties.Federations.PLW },
            { "POL", Properties.Federations.POL },
            { "POR", Properties.Federations.POR },
            { "PUR", Properties.Federations.PUR },
            { "QAT", Properties.Federations.QAT },
            { "ROU", Properties.Federations.ROU },
            { "RSA", Properties.Federations.RSA },
            { "RUS", Properties.Federations.RUS },
            { "RWA", Properties.Federations.RWA },
            { "SCG", Properties.Federations.SCG },
            { "SCO", Properties.Federations.SCO },
            { "SEN", Properties.Federations.SEN },
            { "SEY", Properties.Federations.SEY },
            { "SGP", Properties.Federations.SGP },
            { "SLE", Properties.Federations.SLE },
            { "SLO", Properties.Federations.SLO },
            { "SMR", Properties.Federations.SMR },
            { "SOL", Properties.Federations.SOL },
            { "SRB", Properties.Federations.SRB },
            { "SRI", Properties.Federations.SRI },
            { "SSD", Properties.Federations.SSD },
            { "STP", Properties.Federations.STP },
            { "SUD", Properties.Federations.SUD },
            { "SUI", Properties.Federations.SUI },
            { "SUR", Properties.Federations.SUR },
            { "SVK", Properties.Federations.SVK },
            { "SWE", Properties.Federations.SWE },
            { "SWZ", Properties.Federations.SWZ },
            { "SYR", Properties.Federations.SYR },
            { "TAN", Properties.Federations.TAN },
            { "THA", Properties.Federations.THA },
            { "TJK", Properties.Federations.TJK },
            { "TKM", Properties.Federations.TKM },
            { "TLS", Properties.Federations.TLS },
            { "TOG", Properties.Federations.TOG },
            { "TPE", Properties.Federations.TPE },
            { "TTO", Properties.Federations.TTO },
            { "TUN", Properties.Federations.TUN },
            { "TUR", Properties.Federations.TUR },
            { "UAE", Properties.Federations.UAE },
            { "UGA", Properties.Federations.UGA },
            { "UKR", Properties.Federations.UKR },
            { "URU", Properties.Federations.URU },
            { "USA", Properties.Federations.USA },
            { "UZB", Properties.Federations.UZB },
            { "VEN", Properties.Federations.VEN },
            { "VIE", Properties.Federations.VIE },
            { "WLS", Properties.Federations.WLS },
            { "YEM", Properties.Federations.YEM },
            { "ZAM", Properties.Federations.ZAM },
            { "ZIM", Properties.Federations.ZIM },
        };

    }
}
