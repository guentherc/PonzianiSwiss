using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using PonzianiPlayerBase;
using PonzianiSwiss.Resources;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WPFLocalizeExtension.Engine;
using Extensions = PonzianiSwissLib.Extensions;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            Logger = App.Current.Services?.GetService<ILogger>();
            Logger?.LogDebug("Creating MainWindow");
            InitializeComponent();
            ThemeManager.Current.ChangeTheme(Application.Current, Properties.Settings.Default.BaseTheme, Properties.Settings.Default.ThemeColor);
            Model = new(Logger);
            DataContext = Model;
            //Add Playerbase Update entries dynamically
            RenderThemeMenuEntries();
            RenderLanguageEntries();
            PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE, Logger);
            lvParticipants.ItemsSource = Model.Participants;
            _ = FederationUtil.GetFederations();
            Model.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != null && args.PropertyName.Equals(nameof(Model.RoundCount)))
                    AdjustTabitems();
                else if (args.PropertyName != null && args.PropertyName.Equals(nameof(Model.CurrentRound)))
                    SyncRound();
            };
            AdjustTabitems();
        }

        private readonly ILogger? Logger;

        private void SyncRound()
        {
            var tabitem = MainTabControl.Items[^1] as TabItem;
            Round? r = tabitem?.Content as Round;
            r?.Model.SyncRound();
        }

        private void RenderLanguageEntries()
        {
            MenuItem_Settings_Language.Items.Clear();
            string[] languages = new string[] { "en", "de" };
            foreach (var l in languages)
            {
                MenuItem mi = new()
                {
                    Header = l,
                    Tag = l,
                    IsCheckable = true,
                    IsChecked = l == Properties.Settings.Default.Language[..2],
                    IsEnabled = l != Properties.Settings.Default.Language[..2]
                };
                mi.Click += ChangeLanguage; ;
                MenuItem_Settings_Language.Items.Add(mi);
            }
        }

        private void ChangeLanguage(object sender, RoutedEventArgs e)
        {
            string tag = (string)((MenuItem)sender).Tag;
            Properties.Settings.Default.Language = tag;
            LocalizeDictionary.Instance.Culture = new CultureInfo(PonzianiSwiss.Properties.Settings.Default.Language);
            RenderLanguageEntries();
        }

        private void RenderThemeMenuEntries()
        {
            MenuItem_Settings_Basetheme.Items.Clear();
            MenuItem_Settings_Themecolor.Items.Clear();
            string[] baseThemes = new string[] { "Light", "Dark" };
            string[] themeColor = new string[] { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };
            foreach (var bt in baseThemes)
            {
                MenuItem mi = new()
                {
                    Header = $"{bt}",
                    Tag = bt,
                    IsCheckable = true,
                    IsChecked = bt == Properties.Settings.Default.BaseTheme,
                    IsEnabled = bt != Properties.Settings.Default.BaseTheme
                };
                mi.Click += Update_BaseTheme;
                MenuItem_Settings_Basetheme.Items.Add(mi);
            }
            foreach (var tc in themeColor)
            {
                MenuItem mi = new()
                {
                    Header = $"{tc}",
                    Tag = tc,
                    IsCheckable = true,
                    IsChecked = tc == Properties.Settings.Default.ThemeColor,
                    IsEnabled = tc != Properties.Settings.Default.ThemeColor
                };
                mi.Click += Update_ThemeColor;
                MenuItem_Settings_Themecolor.Items.Add(mi);
            }
        }

        private void Update_ThemeColor(object sender, RoutedEventArgs e)
        {
            MenuItem? mi = sender as MenuItem;
            Properties.Settings.Default.ThemeColor = mi?.Tag.ToString() ?? "Blue";
            LogUserEvent(null, Properties.Settings.Default.ThemeColor);
            ThemeManager.Current.ChangeTheme(Application.Current, Properties.Settings.Default.BaseTheme, Properties.Settings.Default.ThemeColor);
            RenderThemeMenuEntries();
        }

        private void Update_BaseTheme(object sender, RoutedEventArgs e)
        {
            MenuItem? mi = sender as MenuItem;
            Properties.Settings.Default.BaseTheme = mi?.Tag.ToString() ?? "Light";
            LogUserEvent(null, Properties.Settings.Default.BaseTheme);
            ThemeManager.Current.ChangeTheme(Application.Current, Properties.Settings.Default.BaseTheme, Properties.Settings.Default.ThemeColor);
            RenderThemeMenuEntries();
        }

        private void LogUserEvent(string? methodName = null, string? parameter = null)
        {
            if (Logger?.IsEnabled(LogLevel.Debug) ?? false)
            {
                if (parameter != null)
                    Logger?.LogDebug("{menuitem} ({parameter}) clicked", methodName ?? (new System.Diagnostics.StackTrace()).GetFrame(1)?.GetMethod()?.Name, parameter);
                else
                    Logger?.LogDebug("{menuitem} clicked", methodName ?? (new System.Diagnostics.StackTrace()).GetFrame(1)?.GetMethod()?.Name);
            }

        }

        public MainModel Model { set; get; }

        private void MenuItem_Tournament_Exit_Click(object sender, RoutedEventArgs e)
        {
            LogUserEvent();
            Close();
        }

        private void AdjustTabitems()
        {
            int count_before = MainTabControl.Items.Count;
            while ((Model.Tournament?.Rounds.Count ?? 0) + 1 < MainTabControl.Items.Count)
            {
                MainTabControl.Items.Remove(MainTabControl.Items[^1]);
            }
            for (int tabIndx = MainTabControl.Items.Count; tabIndx <= (Model.Tournament?.Rounds.Count ?? 0); ++tabIndx)
            {
                if (Model.Tournament != null)
                {
                    int roundIndex = tabIndx - 1;
                    var content = new Round(Model.Tournament, roundIndex) { };
                    ((Round)content).ResultSet += (s, e) => Model.SetResult(roundIndex, e.Pairing, e.Result);
                    MainTabControl.Items.Add(new TabItem()
                    {
                        Header = $"Round {tabIndx}",
                        Content = content
                    });
                }
            }
            if (MainTabControl.Items.Count != count_before)
                MainTabControl.SelectedIndex = MainTabControl.Items.Count - 1;
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            LogUserEvent();
            MainModel.OnApplicationClose();
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            LogUserEvent();
            if (((MainModel)DataContext).CanClose())
                Application.Current.Shutdown();
            else
                e.Cancel = true;
        }

    }

    public class ViewModel : ObservableObject
    {
        protected ILogger? Logger;

        protected void LogCommand(string? parameter = null, [CallerMemberName] string caller = "")
        {
            if (Logger != null && Logger.IsEnabled(LogLevel.Debug))
            {
                if (parameter == null)
                    Logger.LogDebug("{cmd}", caller);
                else
                    Logger.LogDebug("{cmd}({parm})", caller, parameter);
            }
        }
    }

    public partial class MainModel : ViewModel
    {
        private Tournament? tournament;
        public int TournamentHash = 0;

        private string? fileName;
        public int RoundCount { get => Tournament?.CountRounds ?? 0; }

        private readonly IDialogService? DialogService;
        public ICommand ParticipantDialogCommand { get; set; }
        public ICommand TournamentEditDialogCommand { get; set; }
        public ICommand TournamentAddDialogCommand { get; set; }
        public RelayCommand TournamentSaveAsCommand { get; set; }
        public RelayCommand TournamentSaveCommand { get; set; }
        public RelayCommand TournamentSaveOrSaveAsCommand { get; set; }
        public RelayCommand ForbiddenPairingsRuleDialogCommand { get; set; }

        public RelayCommand<string> HtmlViewerCommand { get; set; }
        public RelayCommand<string> AddRandomParticipantsCommand { get; set; }

        public RelayCommand DrawCommand { get; set; }
        public RelayCommand<TournamentParticipant> ParticipantDeleteCommand { get; set; }
        public RelayCommand DeleteLastRoundCommand { get; set; }

        public MainModel(ILogger? logger)
        {
            Logger = logger;
            logger?.LogDebug("Creating MainWindow");
            var settings = App.Current.Services?.GetService<AppSettings>();
            DialogService = App.Current.Services?.GetService<IDialogService>();
            Mode = settings?.Mode ?? App.Mode.Release;

            ParticipantDialogCommand = new RelayCommand<TournamentParticipant?>((p) => ParticipantDialog(p), (p) => Tournament != null);
            TournamentEditDialogCommand = new RelayCommand(TournamentEditDialog, () => Tournament != null);
            TournamentAddDialogCommand = new RelayCommand(TournamentAddDialog);
            foreach (var entry in PlayerBaseFactory.AvailableBases)
                UpdateMenuEntries.Add(new(entry));
            TournamentSaveAsCommand = new RelayCommand(TournamentSaveAs, () => Tournament != null);
            TournamentSaveCommand = new RelayCommand(TournamentSave, () => FileName != null);
            TournamentSaveOrSaveAsCommand = new RelayCommand(TournamentSave, () => Tournament != null);
            ForbiddenPairingsRuleDialogCommand = new RelayCommand(ForbiddenPairingsRuleDialog, () => Tournament != null);
            HtmlViewerCommand = new RelayCommand<string>((t) => HtmlViewer(int.Parse(t ?? "0")),
                (t) => Tournament != null && Tournament.Participants != null && Tournament.Participants.Count > 0);
            AddRandomParticipantsCommand = new RelayCommand<string>((s) => AddRandomParticipants(int.Parse(s ?? "100")), (s) => Tournament != null);
            DrawCommand = new RelayCommand(Draw, () => DrawEnabled);
            ParticipantDeleteCommand = new RelayCommand<TournamentParticipant>((p) => ParticipantDelete(p), (p) => Tournament != null && Tournament.Rounds.Count == 0);
            DeleteLastRoundCommand = new RelayCommand(DeleteLastRound, () => Tournament != null && Tournament.Rounds.Count > 0);
            UpdateMRUMenu();
            if (settings?.Filename != null && File.Exists(settings.Filename))
                Load(settings.Filename);
        }

        public App.Mode Mode { get; private set; } = App.Mode.Release;

        internal ObservableCollection<TournamentParticipant> Participants { get; set; } = new();
        public ObservableCollection<MenuEntry<PlayerBaseFactory.Base>> UpdateMenuEntries { get; set; } = new();
        public ObservableCollection<MenuEntry<string>> MRUMenuEntries { get; set; } = new();

        public ObservableCollection<MenuEntry<AdditionalRanking>> AdditionalRankingMenuEntries { get; set; } = new();

        public PonzianiSwissLib.Round? CurrentRound { get => Tournament?.Rounds[^1]; }

        public Tournament? Tournament
        {
            get => tournament;
            set
            {
                if (tournament != value)
                {
                    tournament = value;
                    ((RelayCommand<TournamentParticipant>)ParticipantDialogCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)TournamentEditDialogCommand).NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(Tournament));
                    OnPropertyChanged(nameof(DrawEnabled));
                    DeleteLastRoundCommand.NotifyCanExecuteChanged();
                    TournamentSaveAsCommand.NotifyCanExecuteChanged();
                    TournamentSaveOrSaveAsCommand.NotifyCanExecuteChanged();
                    ForbiddenPairingsRuleDialogCommand.NotifyCanExecuteChanged();
                    HtmlViewerCommand.NotifyCanExecuteChanged();
                    AddRandomParticipantsCommand.NotifyCanExecuteChanged();
                    DrawCommand.NotifyCanExecuteChanged();
                }
                if (tournament != null)
                {
                    AdditionalRankingMenuEntries.Clear();
                    foreach (var ar in tournament.AdditionalRankings)
                    {
                        AdditionalRankingMenuEntries.Add(new(ar, ar.Title ?? "?"));
                    }
                    if (tournament.TeamSize.GetValueOrDefault(0) > 1)
                    {
                        AdditionalRankingMenuEntries.Add(new(new(LocalizedStrings.Instance["Export_Team_Ranking_Header"]), LocalizedStrings.Instance["Export_Team_Ranking_Header"]));
                    }
                }
            }
        }

        public bool HasMRUEntries { get => MRUMenuEntries.Count > 0; }

        private bool DrawEnabled
        {
            get => Tournament != null && Tournament.Participants.Count > 0
                && (Tournament.Rounds.Count == 0 || !Tournament.Rounds[^1].Pairings.Where(p => p.Result == Result.Open).Any());
        }

        public string? FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                OnPropertyChanged(nameof(FileName));
                TournamentSaveCommand.NotifyCanExecuteChanged();
            }
        }

        void ForbiddenPairingsRuleDialog()
        {
            ShowForbiddenPairingsRuleDialog(ViewModel => DialogService?.ShowDialog(this, ViewModel));
        }

        void ParticipantDialog(TournamentParticipant? tournamentParticipant)
        {
            ShowParticipantDialog(viewModel => DialogService?.ShowDialog(this, viewModel), tournamentParticipant);
        }

        void TournamentEditDialog()
        {
            ShowTournamentDialog(viewModel => DialogService?.ShowDialog(this, viewModel), Tournament);
        }

        void TournamentAddDialog()
        {
            ShowTournamentDialog(viewModel => DialogService?.ShowDialog(this, viewModel), null);
        }

        void HtmlViewer(int tag)
        {
            ShowHtmlViewer(viewModel => DialogService?.ShowDialog(this, viewModel), tag);
        }

        private void ShowForbiddenPairingsRuleDialog(Func<ForbiddenPairingsDialogViewModel, bool?> showDialog)
        {
            LogCommand();
            if (Tournament == null) return;
            var dialogViewModel = App.Current.Services?.GetService<ForbiddenPairingsDialogViewModel>();

            if (dialogViewModel != null)
            {
                dialogViewModel.Tournament = Tournament;
                showDialog(dialogViewModel);
            }
        }

        private void ShowParticipantDialog(Func<ParticipantDialogViewModel, bool?> showDialog, TournamentParticipant? tournamentParticipant)
        {
            LogCommand(tournamentParticipant?.Participant.Name);
            var dialogViewModel = App.Current.Services?.GetService<ParticipantDialogViewModel>();

            if (dialogViewModel != null)
            {
                bool add = tournamentParticipant == null;
                dialogViewModel.Participant = tournamentParticipant?.Participant ?? new();
                dialogViewModel.Tournament = Tournament;
                bool? success = showDialog(dialogViewModel);
                if (success == true)
                {
                    if (dialogViewModel.Participant != null)
                    {
                        if (add) Tournament?.Participants.Add(dialogViewModel.Participant);
                        SyncParticipants();
                        SyncRounds();
                    }
                    HtmlViewerCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private void ShowTournamentDialog(Func<TournamentDialogViewModel, bool?> showDialog, Tournament? tournament)
        {
            LogCommand(tournament?.Name);
            var dialogViewModel = App.Current.Services?.GetService<TournamentDialogViewModel>();
            if (dialogViewModel != null)
            {
                dialogViewModel.Tournament = tournament ?? new();
                bool? success = showDialog(dialogViewModel);
                if (success == true)
                {
                    if (dialogViewModel.Tournament != null)
                    {
                        Tournament = dialogViewModel.Tournament;
                        SyncParticipants();
                        SyncRounds();
                    }
                }
            }
        }


        private void ShowHtmlViewer(Func<HTMLViewerViewModel, bool?> showDialog, int tag, AdditionalRanking? additionalRanking = null)
        {
            LogCommand(tag.ToString());
            string html = string.Empty;
            string title = string.Empty;
            switch (tag)
            {
                case 0:
                    title = LocalizedStrings.Instance["Participant_List_by_Starting_Rank"];
                    html = Tournament?.ParticipantListHTML("Rating", true) ?? string.Empty;
                    break;
                case 1:
                    title = LocalizedStrings.Instance["Participant_List_by_Name"];
                    html = Tournament?.ParticipantListHTML("Name", false) ?? string.Empty;
                    break;
                case 2:
                    title = LocalizedStrings.Instance["Crosstable"];
                    if (additionalRanking != null && additionalRanking.Title == LocalizedStrings.Instance["Export_Team_Ranking_Header"])
                    {
                        html = Tournament?.TeamHTML(int.MaxValue) ?? string.Empty;
                    }
                    else
                    {
                        html = Tournament?.CrosstableHTML(int.MaxValue, additionalRanking) ?? string.Empty;
                    }
                    break;
                case 3:
                    title = LocalizedStrings.Get("Pairings_Round_X", Tournament?.Rounds.Count ?? 0);
                    html = Tournament?.RoundHTML() ?? string.Empty;
                    break;
            }
            var dialogViewModel = App.Current.Services?.GetService<HTMLViewerViewModel>();
            if (dialogViewModel != null)
            {
                dialogViewModel.Html = html;
                dialogViewModel.Title = title;
                bool? success = showDialog(dialogViewModel);
                if (success == true)
                {

                }
            }
        }

        internal void LoadWithDialog()
        {
            var settings = new OpenFileDialogSettings
            {
                Filter = LocalizedStrings.Instance["Open_Tournament_Dialog_Filter"],
                DefaultExt = ".tjson",
                Title = LocalizedStrings.Instance["Open_Tournament_Dialog_Title"],
                CheckPathExists = true,
                AddExtension = true
            };
            var dialogService = App.Current.Services?.GetService<IDialogService>();
            bool? success = dialogService?.ShowOpenFileDialog(this, settings);
            if (success == true)
            {
                Load(settings.FileName);
            }
        }


        void TournamentSaveAs()
        {
            var settings = new SaveFileDialogSettings
            {
                Title = LocalizedStrings.Instance["Save_Tournament_Dialog_Title"],
                DefaultExt = ".tjson",
                Filter = LocalizedStrings.Instance["Open_Tournament_Dialog_Filter"],
                FileName = FileName ?? Tournament?.Name + ".tjson",
                AddExtension = true
            };

            var dialogService = App.Current.Services?.GetService<IDialogService>();
            bool? success = dialogService?.ShowSaveFileDialog(this, settings);
            if (success == true)
            {
                FileName = settings.FileName;
                TournamentSave();
            }
        }

        void TournamentSave()
        {
            if (Tournament != null)
            {
                if (FileName == null) TournamentSaveAs();
                else
                {
                    File.WriteAllText(FileName, Tournament.Serialize());
                    Logger?.LogInformation("Tournament {name} saved to {filename}", Tournament.Name, FileName);
                }
                if (FileName != null) ProcessMRU(FileName);
            }
        }

        public bool CanClose()
        {
            if (Tournament != null && TournamentHash != Tournament.Hash())
            {

                var messageDialogResult = DialogCoordinator.Instance.ShowModalMessageExternal(this, LocalizedStrings.Instance["Data_Loss_Exit_Title"], LocalizedStrings.Instance["Data_Loss_Exit_Text"], MessageDialogStyle.AffirmativeAndNegative);
                return messageDialogResult != MessageDialogResult.Negative;
            }
            return true;
        }

        public static void OnApplicationClose()
        {
            Properties.Settings.Default.Save();
        }

        [RelayCommand]
        void SettingsReset()
        {
            LogCommand();
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
            ThemeManager.Current.ChangeTheme(Application.Current, Properties.Settings.Default.BaseTheme, Properties.Settings.Default.ThemeColor);
        }

        [RelayCommand]
        async void Update_Base(PlayerBaseFactory.Base b)
        {
            LogCommand(b.ToString());
            IPlayerBase pbase = PlayerBaseFactory.Get(b, Logger);

            var controller = await DialogCoordinator.Instance.ShowProgressAsync(this, LocalizedStrings.Instance["Dialog_Title_Wait"], LocalizedStrings.Get("Base_Update_Might_Take", pbase.Description));
            void updateProgressBar(object? s, ProgressChangedEventArgs e)
            {
                controller.SetProgress(Math.Min(1.0, 0.01 * e.ProgressPercentage));
                controller.SetMessage(e.UserState?.ToString());
            }
            pbase.ProgressChanged += updateProgressBar;
            bool ok = false;
            await Task.Run(async () =>
            {
                ok = await pbase.UpdateAsync();
            });
            await controller.CloseAsync();
            if (ok)
                await DialogCoordinator.Instance.ShowMessageAsync(this, LocalizedStrings.Instance["Base_Update_Ok_Title"], LocalizedStrings.Get("Base_Update_Text", b));
            else
                await DialogCoordinator.Instance.ShowMessageAsync(this, LocalizedStrings.Instance["Base_Update_Ok_Failed"], LocalizedStrings.Get("Base_Update_Text", b));
            pbase.ProgressChanged -= updateProgressBar;
        }

        [RelayCommand]
        async void Open()
        {
            LogCommand();
            if (Tournament != null && TournamentHash != Tournament.Hash())
            {
                var messageDialogResult = await DialogCoordinator.Instance.ShowMessageAsync(this, LocalizedStrings.Instance["Data_Loss_Open_Title"], LocalizedStrings.Instance["Data_Loss_Open_Text"], MessageDialogStyle.AffirmativeAndNegative);
                if (messageDialogResult == MessageDialogResult.Negative) return;
            }
            LoadWithDialog();
        }

        void ParticipantDelete(TournamentParticipant? p)
        {
            LogCommand(p?.Participant.Name);
            if (p != null)
            {
                Tournament?.Participants.Remove(p.Participant);
                SyncParticipants();
            }
        }

        async void Draw()
        {
            LogCommand();
            var controller = await DialogCoordinator.Instance.ShowProgressAsync(this, LocalizedStrings.Instance["Dialog_Title_Wait"], LocalizedStrings.Instance["Draw_Takes_Time"]);
            Tournament?.GetScorecards();
            if (Tournament != null && await Tournament.DrawAsync(Tournament.Rounds.Count))
            {
                Tournament?.GetScorecards();
            }
            await controller.CloseAsync();
            SyncRounds();
            SyncParticipants();
            OnPropertyChanged(nameof(RoundCount));
        }

        void DeleteLastRound()
        {
            LogCommand();
            Tournament?.Rounds.Remove(Tournament.Rounds.Last());
            Tournament?.OrderByRank();
            OnPropertyChanged(nameof(RoundCount));
            SyncParticipants();
            SyncRounds();
        }

        [RelayCommand]
        void Crosstable(AdditionalRanking ar)
        {
            if (ar != null)
                ShowHtmlViewer(viewModel => DialogService?.ShowDialog(this, viewModel), 2, ar);
        }

        [RelayCommand]
        async void LoadTournament(string? filename)
        {
            LogCommand(filename);
            if (filename != null && fileName != string.Empty)
            {
                if (Tournament != null && TournamentHash != Tournament.Hash())
                {
                    MessageDialogResult messageDialogResult = await DialogCoordinator.Instance.ShowMessageAsync(this,
                                               LocalizedStrings.Get("Load_Tournament_Message_Title", System.IO.Path.GetFileNameWithoutExtension(fileName)),
                                               LocalizedStrings.Instance["Data_Loss_Open_Text"], MessageDialogStyle.AffirmativeAndNegative);
                    if (messageDialogResult == MessageDialogResult.Negative) return;
                }
                Load(filename);
            }
        }

        [RelayCommand]
        void Abandon(TournamentParticipant p)
        {
            LogCommand(p.Participant.Name);
            if (Tournament != null && Tournament.Rounds.Count < Tournament.CountRounds)
            {
                if (p.Participant.Active == null)
                {
                    p.Participant.Active = new bool[Tournament.CountRounds];
                    Array.Fill(p.Participant.Active, true);
                }
                for (int i = Tournament.Rounds.Count; i < Tournament.CountRounds; ++i) p.Participant.Active[i] = false;
                SyncParticipants();
            }
        }

        [RelayCommand]
        void Pause(TournamentParticipant p)
        {
            LogCommand(p.Participant.Name);
            if (Tournament != null && Tournament.Rounds.Count < Tournament.CountRounds)
            {
                if (p.Participant.Active == null)
                {
                    p.Participant.Active = new bool[Tournament.CountRounds];
                    Array.Fill(p.Participant.Active, true);
                }
                p.Participant.Active[Tournament.Rounds.Count] = false;
                SyncParticipants();
            }
        }

        [RelayCommand]
        void UndoPause(TournamentParticipant p)
        {
            LogCommand(p.Participant.Name);
            if (p.Participant.Active != null)
            {
                Array.Fill(p.Participant.Active, true);
            }
            SyncParticipants();
        }

        [RelayCommand]
        async void About()
        {
            LogCommand();
            _ = await DialogCoordinator.Instance.ShowMessageAsync(this, LocalizedStrings.Get("About_Dialog_Title", App.VERSION), LocalizedStrings.Instance["About_Dialog_Text"]);
        }

        private string? sortCol = null;
        private bool sort_ascending = true;

        [RelayCommand]
        void SortParticipants(string sortBy)
        {
            LogCommand(sortBy);
            if (sortBy == sortCol) sort_ascending = !sort_ascending; else sort_ascending = true;
            sortCol = sortBy;
            List<TournamentParticipant>? sortedList = null;
            if (sortCol == "Name")
            {
                sortedList = Participants.OrderBy(x => x.Participant.Name ?? string.Empty).ToList();
            }
            else if (sortCol == "Federation")
            {
                sortedList = Participants.OrderBy(x => x.Participant.Federation).ToList();
            }
            else if (sortCol == "FideId")
            {
                sortedList = Participants.OrderBy(x => x.Participant.FideId).ToList();
            }
            else if (sortCol == "Score")
            {
                sortedList = Participants.OrderBy(x => x.Score).ToList();
            }
            else if (sortCol == "Rating")
            {
                sortedList = Participants.OrderBy(x => Tournament?.Rating(x.Participant)).ToList();
            }
            else if (sortCol == "Id")
            {
                sortedList = Participants.OrderBy(x => x.Participant.ParticipantId ?? string.Empty).ToList();
            }
            else if (sortCol == "Elo")
            {
                sortedList = Participants.OrderBy(x => x.Participant.FideRating).ToList();
            }
            else if (sortCol == "NationalRating")
            {
                sortedList = Participants.OrderBy(x => x.Participant.AlternativeRating).ToList();
            }
            else if (sortCol == "Club")
            {
                sortedList = Participants.OrderBy(x => x.Participant.Club ?? string.Empty).ToList();
            }
            else if (sortCol == "EloPerformance")
            {
                sortedList = Participants.OrderBy(x => x.EloPerformance).ToList();
            }
            else if (sortCol == "TournamentPerformance")
            {
                sortedList = Participants.OrderBy(x => x.TournamentPerformance).ToList();
            }
            if (!sort_ascending) sortedList?.Reverse();
            if (sortedList != null)
            {
                Participants.Clear();
                foreach (var p in sortedList)
                    Participants.Add(p);
            }
        }

        private void Load(string filename)
        {
            Logger?.LogInformation("Loading {filename}", filename);
            string json = File.ReadAllText(filename);
            Tournament = Extensions.Deserialize(json);
            if (Tournament != null)
            {
                Tournament.GetScorecards();
                while (Properties.Settings.Default.MRU != null && Properties.Settings.Default.MRU.Count > 10)
                    Properties.Settings.Default.MRU.RemoveAt(10);
                FileName = filename;
                SyncParticipants();
                SyncRounds();
                ProcessMRU(filename);
                TournamentHash = Tournament.Hash();
                OnPropertyChanged(nameof(RoundCount));
            }
            else
                Logger?.LogError("Tournament {filename} wasn't loaded!", filename);
        }

        internal void ProcessMRU(string filename)
        {
            if (Properties.Settings.Default.MRU == null) Properties.Settings.Default.MRU = new StringCollection();
            Logger?.LogDebug("MRU List: {list}  ...", string.Join('|', new List<string>(Properties.Settings.Default.MRU.Cast<string>().ToList())));
            if (Properties.Settings.Default.MRU.Count == 0)
            {
                Properties.Settings.Default.MRU.Add(filename);
                UpdateMRUMenu();
            }
            else if (Properties.Settings.Default.MRU[0] != filename)
            {
                Properties.Settings.Default.MRU.Remove(filename);
                Properties.Settings.Default.MRU.Insert(0, filename);
                UpdateMRUMenu();
            }
            Logger?.LogDebug("MRU List changed to {list}", string.Join('|', Properties.Settings.Default.MRU.Cast<string>().ToList()));
        }

        private void UpdateMRUMenu()
        {
            MRUMenuEntries.Clear();
            if (Properties.Settings.Default.MRU == null) Properties.Settings.Default.MRU = new();
            foreach (var file in Properties.Settings.Default.MRU)
            {
                if (file != null)
                {
                    MRUMenuEntries.Add(new(file, System.IO.Path.GetFileNameWithoutExtension(file)));
                }
                if (MRUMenuEntries.Count > 3) break;
            }
        }

        internal void SyncParticipants()
        {
            Participants.Clear();
            if (Tournament != null)
            {
                foreach (Participant p in Tournament.Participants)
                {
                    Participants.Add(new(tournament, p));
                }
            }
        }

        internal void SyncRounds()
        {
            OnPropertyChanged(nameof(DrawEnabled));
            DrawCommand.NotifyCanExecuteChanged();
            DeleteLastRoundCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        internal void SimulateResults()
        {
            LogCommand();
            if (Tournament != null)
            {
                foreach (Pairing pairing in Tournament.Rounds[^1].Pairings)
                {
                    pairing.Result = PonzianiSwissLib.Utils.Simulate(Tournament.Rating(pairing.White), Tournament.Rating(pairing.Black));
                }
                Tournament.GetScorecards();
            }
            SyncRounds();
            OnPropertyChanged(nameof(CurrentRound));
        }

        internal async void AddRandomParticipants(int count = 100)
        {
            LogCommand(count.ToString());
            FidePlayerBase fide_base = (FidePlayerBase)PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE, Logger);
            var player = await fide_base.GetRandomPlayers(count);
            if (player != null)
            {
                foreach (var p in player)
                {
                    Participant participant = new()
                    {
                        FideId = p.FideId,
                        Name = p.Name,
                        Title = p.Title,
                        Federation = p.Federation ?? "FIDE",
                        YearOfBirth = p.YearOfBirth,
                        FideRating = p.Rating,
                        Club = p.Club ?? string.Empty,
                        Sex = p.Sex
                    };
                    Tournament?.Participants.Add(participant);
                }
                SyncParticipants();
                SyncRounds();
            }
        }

        internal void SetResult(int roundIndex, Pairing pairing, Result result)
        {
            PonzianiSwissLib.Round? round = Tournament?.Rounds[roundIndex];
            if (round != null)
            {
                Pairing? p = round.Pairings.Find((pa) => pa.White == pairing.White && pa.Black == pairing.Black);
                if (p != null)
                {
                    p.Result = result;
                    Tournament?.GetScorecards();
                    foreach (var par in Participants.Where(s => s.Participant == p.White || s.Participant == p.Black))
                        par.UpdateResult();
                    SyncRounds();
                }
            }
        }
    }

    public partial class TournamentParticipant : ObservableObject
    {
        private readonly Tournament? tournament;

        [ObservableProperty]
        private Participant participant;

        public TournamentParticipant(Tournament? tournament, Participant participant)
        {
            this.tournament = tournament;
            this.participant = participant;
        }

        public float Score
        {
            get => Participant.Scorecard?.Score() ?? 0;
        }

        public int EloPerformance
        {
            get => Participant.Scorecard?.EloPerformance() ?? 0;
        }

        public void UpdateResult()
        {
            OnPropertyChanged(nameof(Score));
            OnPropertyChanged(nameof(EloPerformance));
            OnPropertyChanged(nameof(TournamentPerformance));
        }

        public int TournamentPerformance
        {
            get
            {
                if (Participant.Scorecard != null)
                {
                    var wins = Participant.Scorecard.Entries.Where(e => e.Result == Result.Win && tournament?.Rating(e.Opponent) > 0);
                    var draws = Participant.Scorecard.Entries.Where(e => e.Result == Result.Draw && tournament?.Rating(e.Opponent) > 0);
                    var losses = Participant.Scorecard.Entries.Where(e => e.Result == Result.Loss && tournament?.Rating(e.Opponent) > 0);
                    float score = wins.Count() + .5f * draws.Count();
                    if (score == 0) return 0;
                    int totalRating = wins.Sum(e => tournament?.Rating(e.Opponent) ?? 0) + draws.Sum(e => tournament?.Rating(e.Opponent) ?? 0) + losses.Sum(e => tournament?.Rating(e.Opponent) ?? 0);
                    int countGames = wins.Count() + draws.Count() + losses.Count();
                    float avgRating = 1f * totalRating / countGames;
                    return (int)Math.Round(avgRating + 800 * ((score / countGames) - 0.5f));
                }
                else return 0;
            }
        }


        public int TournamentRating { get => tournament?.Rating(Participant) ?? 0; }

        /// <summary>
        /// Abandoned párticipants will be rendered with strikethrough
        /// </summary>
        public TextDecorationCollection? TextDecoration
        {
            get
            {
                if (tournament == null) return null;
                TextDecorationCollection? result = null;
                for (int i = tournament.Rounds.Count; i < tournament.CountRounds; ++i)
                {
                    result = TextDecorations.Strikethrough;
                    if ((bool)(Participant.Active?.GetValue(i) ?? true))
                    {
                        return null;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Paused participants will be rendered in italics
        /// </summary>
        public FontStyle FontStyle
        {
            get
            {
                if (tournament == null) return FontStyles.Normal;
                bool paused = false;
                if (Participant.Active != null && Participant.Active.Last())
                {
                    for (int i = tournament.Rounds.Count; i < Participant.Active?.Length; ++i)
                    {
                        if (!((bool)(Participant.Active?.GetValue(i) ?? true)))
                        {
                            paused = true;
                            break;
                        }
                    }
                }
                return paused ? FontStyles.Italic : FontStyles.Normal;
            }
        }
    }

    public class MenuEntry<T>
    {
        public MenuEntry(KeyValuePair<T, string> entry)
        {
            Header = $"{entry.Key} ({entry.Value})";
            Key = entry.Key;
        }

        public MenuEntry(T key, string header)
        {
            Header = header;
            Key = key;
        }

        public string Header { get; set; }
        public T Key { get; set; }
    }

    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Source = Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
