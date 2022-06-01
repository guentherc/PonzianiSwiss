using AutoCompleteTextBox.Editors;
using MahApps.Metro.Controls;
using PonzianiPlayerBase;
using PonzianiSwissLib;
using System;
using System.Collections;
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
    /// Interaction logic for ParticipantDialog.xaml
    /// </summary>
    public partial class ParticipantDialog : MetroWindow
    {
        public ParticipantDialog(Participant participant, Tournament? tournament)
        {
            Model = new(participant, tournament);
            this.DataContext = Model;            
            InitializeComponent();
            ComboBox_Federation.ItemsSource = TournamentDialog.Federations.OrderBy(e => e.Key);
            ComboBox_Federation.DisplayMemberPath = "Value";
            ComboBox_Federation.SelectedValuePath = "Key";
            ComboBox_Federation.SelectedValue = "FIDE";

            ComboBox_Title.ItemsSource = Enum.GetValues(typeof(FideTitle));
        }

        public ParticipantModel Model { set; get; }

        private void TournamentDialogOkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void TournamentDialogCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

    }

    public class ParticipantModel : ViewModel
    {
        public Participant Participant { set; get; }
        public Tournament? Tournament { set; get; }

        private readonly IPlayerBase FideBase;

        public ParticipantModel(Participant participant, Tournament? tournament)
        {
            Participant = participant;
            Tournament = tournament;
            FideBase = PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE);
        }

        public ulong FideId
        {
            get => Participant.FideId;
            set
            {
                Participant.FideId = value;
                if (Participant.FideId != 0)
                {
                    var p = FideBase.GetById(Participant.FideId.ToString());
                    if (p != null)
                    {
                        Name = p.Name;
                        Title = p.Title;
                        Federation = p.Federation ?? "FIDE";
                        Year = p.YearOfBirth;
                        Rating = p.Rating;
                        Club = p.Club ?? string.Empty;
                        Female = p.Sex == Sex.Female;
                    }
                }
                RaisePropertyChange();
            }
        }

        public string? Name
        {
            get => Participant.Name;
            set
            {
                Participant.Name = value;
                RaisePropertyChange();
            }
        }

        public FideTitle Title
        {
            get => Participant.Title;
            set
            {
                Participant.Title = value;
                RaisePropertyChange();
            }
        }

        public string SelectedName
        {
            set
            {
                if (value != null) UpdateFromSuggest(value);
            }
        }


        public string Federation
        {
            get => Participant.Federation ?? "FIDE";
            set
            {
                Participant.Federation = value;
                RaisePropertyChange();
            }
        }

        public int Year
        {
            get => Participant.YearOfBirth;
            set
            {
                Participant.YearOfBirth = value;
                RaisePropertyChange();
            }
        }

        public int Rating
        {
            get => Participant.FideRating;
            set
            {
                Participant.FideRating = value;
                RaisePropertyChange();
            }
        }

        public int AlternativeRating
        {
            get => Participant.AlternativeRating;
            set
            {
                Participant.AlternativeRating = value;
                RaisePropertyChange();
            }
        }

        public string Club
        {
            get => Participant.Club ?? string.Empty;
            set
            {
                Participant.Club = value;
                RaisePropertyChange();
            }
        }

        public bool Female
        {
            get => Participant.Sex == Sex.Female;
            set
            {
                Participant.Sex = value ? Sex.Female : Sex.Male;
                RaisePropertyChange();
            }
        }

        public void UpdateFromSuggest(string suggest)
        {
            string token = suggest.Split(' ').Last();
            string fid = token[1..^1];
            try { 
            FideId = ulong.Parse(fid);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }

    public class ParticipantProvider : ISuggestionProvider
    {
        public IEnumerable GetSuggestions(string filter)
        {
            return PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE).Find(filter)
                .Select(p => $"{(p.Title != FideTitle.NONE ? p.Title : string.Empty)} {p.Name} {(p.FideId != 0 ? "(" + p.FideId + ")" : string.Empty)}".Trim());
        }
    }
}
