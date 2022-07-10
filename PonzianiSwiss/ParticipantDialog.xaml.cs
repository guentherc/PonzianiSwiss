using AutoCompleteTextBox.Editors;
using MahApps.Metro.Controls;
using Microsoft.Extensions.Logging;
using PonzianiPlayerBase;
using PonzianiSwissLib;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for ParticipantDialog.xaml
    /// </summary>
    public partial class ParticipantDialog : MetroWindow
    {
        public ParticipantDialog(Participant participant, Tournament? tournament, ILogger? logger)
        {
            Logger = logger;
            Model = new(participant, tournament, Logger);
            this.DataContext = Model;
            InitializeComponent();
            var items = FederationUtil.Federations.OrderBy(e => e.Value);
            ComboBox_Federation.ItemsSource = items;
            ComboBox_Federation.DisplayMemberPath = "Value";
            ComboBox_Federation.SelectedValuePath = "Key";
            //strange workaround to make combobox show federation value 
            int indx = items.ToList().FindIndex(e => e.Key == participant.Federation);
            ComboBox_Federation.SelectedIndex = indx;
            //ComboBox_Federation.SelectedValue = Model.Participant.Federation != null && Model.Participant.Federation != String.Empty ? Model.Participant.Federation : "FIDE";

            ComboBox_Title.ItemsSource = Enum.GetValues(typeof(FideTitle));
            Logger = logger;
        }

        private readonly ILogger? Logger;

        public ParticipantModel Model { set; get; }

        private void ParticipantDialogOkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            FixParticipant();
            this.Close();
        }

        private static readonly Regex regexFixParticipant = new(@"(GM|IM|FM|CM|WGM|WIM|WFM|WCM|WH)?\s?([^\(]+)\s\((\d+)\)", RegexOptions.Compiled);
        private void FixParticipant()
        {
            if (Model.Participant.Name != null)
            {
                Match m = regexFixParticipant.Match(Model.Participant.Name);
                if (m.Success && m.Groups[3].Value.Trim() == Model.Participant.FideId.ToString())
                {
                    Model.Participant.Name = m.Groups[2].Value.Trim();
                }
            }
        }

        private void ParticipantDialogCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void MenuItem_PlayerSearch_Open_Click(object sender, RoutedEventArgs e)
        {
            PlayerSearchDialog psd = new(Logger)
            {
                Owner = this
            };
            if (psd.ShowDialog() ?? false)
            {
                Player? nplayer = psd.Player;
                if (nplayer != null && nplayer.FideId != 0)
                {
                    Model.FideId = nplayer.FideId;

                }
                else
                {
                    Model.Participant.Name = nplayer?.Name ?? string.Empty;
                    Model.Participant.Title = nplayer?.Title ?? FideTitle.NONE;

                    Model.Participant.Federation = nplayer?.Federation ?? "FIDE";
                    Model.Participant.YearOfBirth = nplayer?.YearOfBirth ?? 0;
                    Model.Female = (nplayer?.Sex ?? Sex.Male) == Sex.Female;
                }
                Model.Participant.Club = nplayer?.Club ?? string.Empty;
                Model.Participant.AlternativeRating = nplayer?.Rating ?? 0;
                Model.Sync();

            }
        }

    }

    public class ParticipantModel : ViewModel
    {
        public Participant Participant
        {
            get => participant;
            set
            {
                participant = value;
                RaisePropertyChange();
            }
        }
        public Tournament? Tournament { set; get; }

        private readonly IPlayerBase FideBase;
        private Participant participant;

        public ParticipantModel(Participant participant, Tournament? tournament, ILogger? logger)
        {
            Logger = logger;
            this.participant = participant;
            Tournament = tournament;
            FideBase = PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE, logger);
        }

        public void Sync() => RaisePropertyChange(nameof(Participant));

        [DependentProperties("Participant")]
        public ulong FideId
        {
            get
            {
                return Participant.FideId;
            }
            set
            {
                if (Participant.FideId == value) return;
                Participant.FideId = value;
                if (Participant.FideId != 0)
                {
                    var p = FideBase.GetById(Participant.FideId.ToString());
                    if (p != null)
                    {
                        Participant.Name = p.Name;
                        Participant.Title = p.Title;
                        Participant.Federation = p.Federation ?? "FIDE";
                        Participant.YearOfBirth = p.YearOfBirth;
                        Participant.FideRating = p.Rating;
                        Participant.Club = p.Club ?? string.Empty;
                        Female = p.Sex == Sex.Female;
                    }
                }
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


        [DependentProperties("Participant")]
        public bool Female
        {
            get => Participant.Sex == Sex.Female;
            set
            {
                if (value == (Participant.Sex == Sex.Female)) return;
                Participant.Sex = value ? Sex.Female : Sex.Male;
                RaisePropertyChange();
            }
        }

        public void UpdateFromSuggest(string suggest)
        {
            string token = suggest.Split(' ').Last();
            string fid = token[1..^1];
            try
            {
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
            return PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE, null).Find(filter)
                .Select(p => $"{(p.Title != FideTitle.NONE ? p.Title : string.Empty)} {p.Name} {(p.FideId != 0 ? "(" + p.FideId + ")" : string.Empty)}".Trim());
        }
    }

    public class TextSetToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value?.ToString()?.Trim().Length > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
