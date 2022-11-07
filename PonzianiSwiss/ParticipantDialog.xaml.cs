using AutoCompleteTextBox.Editors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvvmDialogs;
using PonzianiPlayerBase;
using PonzianiSwissLib;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for ParticipantDialog.xaml
    /// </summary>
    public partial class ParticipantDialog : MetroWindow
    {
        public ParticipantDialog()
        {
            InitializeComponent();
            var items = FederationUtil.Federations.OrderBy(e => e.Value);
            ComboBox_Federation.ItemsSource = items;
            ComboBox_Federation.DisplayMemberPath = "Value";
            ComboBox_Federation.SelectedValuePath = "Key";
            //strange workaround to make combobox show federation value 
            /*int indx = items.ToList().FindIndex(e => e.Key == ((ParticipantDialogViewModel)DataContext).Participant?.Federation);
            ComboBox_Federation.SelectedIndex = indx;*/
            //ComboBox_Federation.SelectedValue = Model.Participant.Federation != null && Model.Participant.Federation != String.Empty ? Model.Participant.Federation : "FIDE";
            ComboBox_Title.ItemsSource = Enum.GetValues(typeof(FideTitle));
        }
    }

    public partial class ParticipantDialogViewModel : ViewModel, IModalDialogViewModel
    {

        public ParticipantDialogViewModel(ILogger? logger, IDialogService dialogService)
        {
            Logger = logger;
            DialogService = dialogService;
            FideBase = PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE, logger);
            PlayerSearchDialogCommand = new RelayCommand(PlayerSearchDialog);
            ParticipantAttributesDialogCommand = new RelayCommand(ParticipantAttributeDialog);
        }

        [ObservableProperty]
        private bool? dialogResult;

        [ObservableProperty]
        private Participant? participant;

        [ObservableProperty]
        private Tournament? tournament;

        public ICommand PlayerSearchDialogCommand { get; }
        public ICommand ParticipantAttributesDialogCommand { get; }

        [RelayCommand]
        void Ok()
        {
            FixParticipant();
            DialogResult = true;
        }

        [RelayCommand]
        void Cancel()
        {
            DialogResult = false;
        }

        public ulong FideId
        {
            get
            {
                return Participant?.FideId ?? 0;
            }
            set
            {
                if (Participant != null)
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
                    OnPropertyChanged(nameof(FideId));
                    OnPropertyChanged(nameof(Participant));
                }
            }
        }

        public bool Female
        {
            get => Participant?.Sex == Sex.Female;
            set
            {
                if (value == (Participant?.Sex == Sex.Female)) return;
                if (Participant != null)
                {
                    Participant.Sex = value ? Sex.Female : Sex.Male;
                    OnPropertyChanged(nameof(Female));
                    OnPropertyChanged(nameof(Participant));
                }
            }
        }

        private readonly IDialogService? DialogService;
        private readonly IPlayerBase FideBase;

        private void PlayerSearchDialog()
        {
            ShowDialog(viewModel => DialogService?.ShowDialog(this, viewModel));
        }

        private void ShowDialog(Func<PlayerSearchDialogViewModel, bool?> showDialog)
        {
            var dialogViewModel = App.Current.Services?.GetService<PlayerSearchDialogViewModel>();

            if (dialogViewModel != null)
            {
                bool? success = showDialog(dialogViewModel);
                if (success == true)
                {
                    Player? nplayer = dialogViewModel.Player;
                    if (nplayer != null && nplayer.FideId != 0)
                    {
                        FideId = nplayer.FideId;

                    }
                    else
                    {
                        if (Participant != null)
                        {
                            Participant.Name = nplayer?.Name ?? string.Empty;
                            Participant.Title = nplayer?.Title ?? FideTitle.NONE;

                            Participant.Federation = nplayer?.Federation ?? "FIDE";
                            Participant.YearOfBirth = nplayer?.YearOfBirth ?? 0;
                            Female = (nplayer?.Sex ?? Sex.Male) == Sex.Female;
                        }
                    }
                    if (Participant != null)
                    {
                        Participant.Club = nplayer?.Club ?? string.Empty;
                        Participant.AlternativeRating = nplayer?.Rating ?? 0;
                    }
                    OnPropertyChanged(nameof(Participant));
                }
            }
        }

        private void ParticipantAttributeDialog()
        {
            ShowParticipantAttributeDialog(viewModel => DialogService?.ShowDialog(this, viewModel));
        }

        private void ShowParticipantAttributeDialog(Func<ParticipantAttributeDialogViewModel, bool?> showDialog)
        {
            var dialogViewModel = App.Current.Services?.GetService<ParticipantAttributeDialogViewModel>();

            if (dialogViewModel != null)
            {
                dialogViewModel.Participant = Participant;
                bool? success = showDialog(dialogViewModel);
                if (success == true)
                {

                }
            }
        }

        private static readonly Regex regexFixParticipant = new(@"(GM|IM|FM|CM|WGM|WIM|WFM|WCM|WH)?\s?([^\(]+)\s\((\d+)\)", RegexOptions.Compiled);
        private void FixParticipant()
        {
            if (Participant?.Name != null)
            {
                Match m = regexFixParticipant.Match(Participant.Name);
                if (m.Success && m.Groups[3].Value.Trim() == Participant.FideId.ToString())
                {
                    Participant.Name = m.Groups[2].Value.Trim();
                }
            }
        }

        public string SelectedName
        {
            set
            {
                if (value != null) UpdateFromSuggest(value);
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
                Logger?.LogError("{exc}", ex.Message);
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
