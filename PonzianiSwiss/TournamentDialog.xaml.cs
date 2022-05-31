using MahApps.Metro.Controls;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for TournamentDialog.xaml
    /// </summary>
    public partial class TournamentDialog : MetroWindow
    {
        public TournamentDialog(Tournament tournament)
        {
            Model = new(tournament);
            InitializeComponent();
            this.DataContext = Model;
            ComboBox_Federation.ItemsSource = Federations.OrderBy(e => e.Key);
            ComboBox_Federation.DisplayMemberPath = "Value";
            ComboBox_Federation.SelectedValuePath = "Key";
            ComboBox_Federation.SelectedValue = Model.Tournament.Federation != null && Model.Tournament.Federation != String.Empty ? Model.Tournament.Federation : "FIDE";
            this.Title = (tournament.Name == null || tournament.Name == String.Empty) ? this.Title = "Create new Tournament" : this.Title = $"Edit {Model.Tournament.Name}";
            ComboBox_PairingSystem.ItemsSource = Enum.GetValues(typeof(PairingSystem));
            ComboBox_RatingDetermination.ItemsSource = Enum.GetValues(typeof(TournamentRatingDetermination));
        }

        public TournamentModel Model { set; get; }

        private void TournamentDialogOkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void TournamentDialogCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public readonly static Dictionary<string, string> Federations = new()
        {
            { "ACF", "Asian Chess Federation" },
            { "AFG", "Afghanistan" },
            { "AHO", "Netherlands Antilles" },
            { "ALB", "Albania" },
            { "ALG", "Algeri" },
            { "AND", "Andorra" },
            { "ANG", "Angola" },
            { "ANT", "Antigua and Barbuda" },
            { "ARG", "Argentine" },
            { "ARM", "Armenia" },
            { "ARU", "Aruba" },
            { "AUS", "Australia" },
            { "AUT", "Austria" },
            { "AZE", "Azerbaijan" },
            { "BAH", "Bahamas" },
            { "BAN", "Bangladesh" },
            { "BAR", "Barbados" },
            { "BDI", "Burundi" },
            { "BEL", "Belgium" },
            { "BEN", "Benin" },
            { "BHU", "Bhutan" },
            { "BIH", "Bosnia & Herzegovina" },
            { "BLR", "Belarus" },
            { "BOL", "Bolivia" },
            { "BOT", "Botswana" },
            { "BRA", "Brazil" },
            { "BRN", "Bahrain" },
            { "BRU", "Brunei Darussalam" },
            { "BUL", "Bulgaria" },
            { "BUR", "Burkina Faso" },
            { "CAM", "Cambodia" },
            { "CAN", "Canada" },
            { "CAT", "Catalonia" },
            { "CCA", "Confederation of Chess for Americas" },
            { "CHI", "Chile" },
            { "CHN", "China" },
            { "CIV", "Ivory Coast" },
            { "CMR", "Cameroon" },
            { "COL", "Colombia" },
            { "CPV", "Cape Verde" },
            { "CRC", "Costa Rica" },
            { "CRO", "Croatia" },
            { "CUB", "Cuba" },
            { "CYP", "Cyprus" },
            { "CZE", "Czech Republic" },
            { "DEN", "Denmark" },
            { "DJI", "Djibouti" },
            { "DOM", "Dominican Republic" },
            { "ECU", "Ecuador" },
            { "EGY", "Egypt" },
            { "ENG", "England" },
            { "ESA", "El Salvador" },
            { "ESP", "Spain" },
            { "EST", "Estonia" },
            { "ETH", "Ethiopia" },
            { "FAI", "Faroe Islands" },
            { "FIDE", "FIDE" },
            { "FIJ", "Fiji" },
            { "FIN", "Finland" },
            { "FRA", "France" },
            { "GAM", "Gambia" },
            { "GCI", "Guernsey" },
            { "GEO", "Georgia" },
            { "GER", "Germany" },
            { "GHA", "Ghana" },
            { "GRE", "Greece" },
            { "GUA", "Guatemala" },
            { "GUM", "Guam" },
            { "HAI", "Haiti" },
            { "HKG", "Hong Kong" },
            { "HON", "Honduras" },
            { "HUN", "Hungary" },
            { "IMN", "Isle of Man" },
            { "INA", "Indonesia" },
            { "IND", "India" },
            { "IRI", "Iran" },
            { "IRL", "Ireland" },
            { "IRQ", "Iraq" },
            { "ISL", "Iceland" },
            { "ISR", "Israel" },
            { "ISV", "US Virgin Islands" },
            { "ITA", "Italy" },
            { "IVB", "British Virgin Islands" },
            { "JAM", "Jamaica" },
            { "JCI", "Jersey" },
            { "JOR", "Jordan" },
            { "JPN", "Japan" },
            { "KAZ", "Kazakhstan" },
            { "KEN", "Kenya" },
            { "KGZ", "Kyrgyzstan" },
            { "KOR", "South Korea" },
            { "KOS", "Kosovo" },
            { "KSA", "Saudi Arabia" },
            { "KUW", "Kuwait" },
            { "LAO", "Laos" },
            { "LAT", "Latvia" },
            { "LBA", "Libya" },
            { "LBN", "Lebanon" },
            { "LBR", "Liberia" },
            { "LCA", "Saint Lucia" },
            { "LES", "Lesotho" },
            { "LIE", "Liechtenstein" },
            { "LTU", "Lithuania" },
            { "LUX", "Luxembourg" },
            { "MAC", "Macau" },
            { "MAD", "Madagascar" },
            { "MAR", "Morocco" },
            { "MAS", "Malaysia" },
            { "MAW", "Malawi" },
            { "MDA", "Moldova" },
            { "MDV", "Maldives" },
            { "MEX", "Mexico" },
            { "MGL", "Mongolia" },
            { "MKD", "North Macedonia" },
            { "MLI", "Mali" },
            { "MLT", "Malta" },
            { "MNC", "Monaco" },
            { "MNE", "Montenegro" },
            { "MOZ", "Mozambique" },
            { "MRI", "Mauritius" },
            { "MTN", "Mauritania" },
            { "MYA", "Myanmar" },
            { "NAM", "Namibia" },
            { "NCA", "Nicaragua" },
            { "NED", "Netherlands" },
            { "NEP", "Nepal" },
            { "NGR", "Nigeria" },
            { "NOR", "Norway" },
            { "NZL", "New Zealand" },
            { "OMA", "Oman" },
            { "PAK", "Pakistan" },
            { "PAN", "Panama" },
            { "PAR", "Paraguay" },
            { "PER", "Peru" },
            { "PHI", "Philippines" },
            { "PLE", "Palestine" },
            { "PLW", "Palau" },
            { "POL", "Poland" },
            { "POR", "Portugal" },
            { "PUR", "Puerto Rico" },
            { "QAT", "Qatar" },
            { "ROU", "Romania" },
            { "RSA", "South Africa" },
            { "RUS", "Russia" },
            { "RWA", "Rwanda" },
            { "SCG", "Yugoslavia" },
            { "SCO", "Scotland" },
            { "SEN", "Senegal" },
            { "SEY", "Seychelles" },
            { "SGP", "Singapore" },
            { "SLE", "Sierra Leone" },
            { "SLO", "Slovenia" },
            { "SMR", "San Marino" },
            { "SOL", "Solomon Islands" },
            { "SRB", "Serbia" },
            { "SRI", "Sri Lanka" },
            { "SSD", "South Sudan" },
            { "STP", "Sao Tome and Principe" },
            { "SUD", "Sudan" },
            { "SUI", "Switzerland" },
            { "SUR", "Suriname" },
            { "SVK", "Slovakia" },
            { "SWE", "Sweden" },
            { "SWZ", "Swaziland" },
            { "SYR", "Syria" },
            { "TAN", "Tanzania" },
            { "THA", "Thailand" },
            { "TJK", "Tajikistan" },
            { "TKM", "Turkmenistan" },
            { "TLS", "Timor-leste" },
            { "TOG", "Togo" },
            { "TPE", "Chinese Taipei" },
            { "TTO", "Trinidad & Tobago" },
            { "TUN", "Tunisia" },
            { "TUR", "Turkey" },
            { "UAE", "United Arab Emirates" },
            { "UGA", "Uganda" },
            { "UKR", "Ukraine" },
            { "URU", "Uruguay" },
            { "USA", "United States of America" },
            { "UZB", "Uzbekistan" },
            { "VEN", "Venezuela" },
            { "VIE", "Vietnam" },
            { "WLS", "Wales" },
            { "YEM", "Yemen" },
            { "ZAM", "Zambia" },
            { "ZIM", "Zimbabwe" }
        };

    }

    public class TournamentModel : ViewModel
    {
        private Tournament tournament;

        public Tournament Tournament { get => tournament; set {
                tournament = value;
                RaisePropertyChange(); }
        }

                public TournamentModel(Tournament tournament)
        {
            Tournament = tournament;
        }
    }
}
