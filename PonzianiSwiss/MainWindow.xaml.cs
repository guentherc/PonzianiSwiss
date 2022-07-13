using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using MvvmDialogs;
using PonzianiPlayerBase;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
            var settings = App.Current.Services?.GetService<AppSettings>();
            App.Mode mode = settings?.Mode ?? App.Mode.Release;
            string? filename = settings?.Filename;
            Logger = App.Current.Services?.GetService<ILogger>();
            Logger?.LogDebug("Creating MainWindow (Mode: {mode}, File: {file})", mode, settings?.Filename);
            InitializeComponent();
            ThemeManager.Current.ChangeTheme(Application.Current, Properties.Settings.Default.BaseTheme, Properties.Settings.Default.ThemeColor);
            Model = new(mode, Logger);
            DataContext = Model;
            //Add Playerbase Update entries dynamically
            RenderThemeMenuEntries();
            PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE, Logger);
            lvParticipants.ItemsSource = Model.Participants;
            _ = FederationUtil.GetFederations();
            if (filename != null) Load(filename);
        }

        private int TournamentHash = 0;
        private readonly ILogger? Logger;

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
        private readonly HTMLViewer htmlViewer = new();

        private void MenuItem_Tournament_Exit_Click(object sender, RoutedEventArgs e)
        {
            LogUserEvent();
            Close();
        }

        private async void MenuItem_Tournament_Open_ClickAsync(object sender, RoutedEventArgs e)
        {
            LogUserEvent("MenuItem_Tournament_Open_ClickAsync");
            var uiContext = SynchronizationContext.Current;
            MessageDialogResult messageDialogResult = MessageDialogResult.Affirmative;
            if (Model.Tournament != null && TournamentHash != Model.Tournament.Hash())
            {
                messageDialogResult = await this.ShowMessageAsync("Load Tournament", "There might be unsaved data which will be lost! Do you want to proceed?", MessageDialogStyle.AffirmativeAndNegative);
                if (messageDialogResult == MessageDialogResult.Negative) return;
            }
            uiContext?.Send(x => LoadWithDialog(), null);
        }

        private void LoadWithDialog()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = $"Tournament Files|*.tjson|All Files|*.*",
                DefaultExt = ".tjson",
                Title = "Open Tournament File",
                CheckPathExists = true,
                AddExtension = true
            };
            if (openFileDialog.ShowDialog() ?? false)
            {
                Load(openFileDialog.FileName);
            }
        }

        private void Load(string filename)
        {
            Logger?.LogInformation("Loading {filename}", filename);
            string json = File.ReadAllText(filename);
            Model.Tournament = Extensions.Deserialize(json);
            if (Model.Tournament != null)
            {
                Model.Tournament.GetScorecards();
                while (Properties.Settings.Default.MRU != null && Properties.Settings.Default.MRU.Count > 10)
                    Properties.Settings.Default.MRU.RemoveAt(10);
                Model.FileName = filename;
                Model.SyncParticipants();
                Model.SyncRounds();
                ProcessMRU(filename);
                AdjustTabitems();
                TournamentHash = Model.Tournament.Hash();
            }
            else
                Logger?.LogError("Tournament {filename} wasn't loaded!", filename);
        }

        private void ProcessMRU(string filename)
        {
            if (Properties.Settings.Default.MRU == null) Properties.Settings.Default.MRU = new StringCollection();
            Logger?.LogDebug("MRU List: {list}  ...", string.Join('|', new List<string>(Properties.Settings.Default.MRU.Cast<string>().ToList())));
            if (Properties.Settings.Default.MRU.Count == 0)
            {
                Properties.Settings.Default.MRU.Add(filename);
                Model.MRUModel = new();
            }
            else if (Properties.Settings.Default.MRU[0] != filename)
            {
                Properties.Settings.Default.MRU.Remove(filename);
                Properties.Settings.Default.MRU.Insert(0, filename);
                Model.MRUModel = new();
            }
            Logger?.LogDebug("MRU List changed to {list}", string.Join('|', Properties.Settings.Default.MRU.Cast<string>().ToList()));
        }

        private void MenuItem_Tournament_Save_Click(object sender, RoutedEventArgs e)
        {
            LogUserEvent();
            if (Model.Tournament != null)
            {
                if (Model.FileName == null) MenuItem_Tournament_Save_As_Click(sender, e);
                else
                {
                    File.WriteAllText(Model.FileName, Model.Tournament.Serialize());
                    Logger?.LogInformation("Tournament {name} saved to {filename}", Model.Tournament.Name, Model.FileName);
                }
                if (Model.FileName != null) ProcessMRU(Model.FileName);
            }
        }

        private void MenuItem_Tournament_Save_As_Click(object sender, RoutedEventArgs e)
        {
            LogUserEvent();
            SaveFileDialog saveFileDialog = new()
            {
                Filter = $"Tournament Files|*.tjson|All Files|*.*"
            };
            if (Model.FileName != null) saveFileDialog.FileName = Model.FileName; else saveFileDialog.FileName = Model.Tournament?.Name + ".tjson";
            saveFileDialog.DefaultExt = ".tjson";
            saveFileDialog.Title = "Save Tournament File";
            saveFileDialog.AddExtension = true;
            if (saveFileDialog.ShowDialog() ?? false)
            {
                Model.FileName = saveFileDialog.FileName;
                MenuItem_Tournament_Save_Click(sender, e);
            }
        }

        private void MenuItem_Round_Delete_Click(object sender, RoutedEventArgs e)
        {
            LogUserEvent();
            Model.Tournament?.Rounds.Remove(Model.Tournament.Rounds.Last());
            Model.Tournament?.OrderByRank();
            AdjustTabitems();
            Model.SyncRounds();
        }

        private async void MenuItem_Round_Draw_Click(object sender, RoutedEventArgs e)
        {
            LogUserEvent("MenuItem_Round_Draw_Click");
            var controller = await this.ShowProgressAsync("Please wait...", "Draw might take some time");
            Cursor = Cursors.Wait;
            var uiContext = SynchronizationContext.Current;
            Model.Tournament?.GetScorecards();
            if (Model.Tournament != null && await Model.Tournament.DrawAsync(Model.Tournament.Rounds.Count))
            {
                Model.Tournament?.GetScorecards();
            }
            uiContext?.Send(x => Model.SyncRounds(), null);
            uiContext?.Send(x => Model.SyncParticipants(), null);
            uiContext?.Send(x => AdjustTabitems(), null);
            uiContext?.Send(x => MainTabControl.SelectedItem = MainTabControl.Items[^1], null);
            uiContext?.Send(x => Cursor = Cursors.Arrow, null);
            await controller.CloseAsync();
        }

        private void MenuItem_Participant_Abandon_Click(object sender, RoutedEventArgs e)
        {
            if (lvParticipants?.SelectedItem is TournamentParticipant p && Model.Tournament != null)
            {
                LogUserEvent(null, p.Participant.Name);
                if (p.Participant.Active == null)
                {
                    p.Participant.Active = new bool[Model.Tournament.CountRounds];
                    Array.Fill(p.Participant.Active, true);
                }
                for (int i = Model.Tournament.Rounds.Count; i < Model.Tournament.CountRounds; ++i) p.Participant.Active[i] = false;
                Model.SyncParticipants();
            }
        }

        private void MenuItem_Participant_Pause_Click(object sender, RoutedEventArgs e)
        {
            if (lvParticipants?.SelectedItem is TournamentParticipant p && Model.Tournament != null)
            {
                LogUserEvent(null, p.Participant.Name);
                if (p.Participant.Active == null)
                {
                    p.Participant.Active = new bool[Model.Tournament.CountRounds];
                    Array.Fill(p.Participant.Active, true);
                }
                p.Participant.Active[Model.Tournament.Rounds.Count] = false;
                Model.SyncParticipants();
            }
        }

        private void MenuItem_Participant_UndoPause_Click(object sender, RoutedEventArgs e)
        {
            if (lvParticipants?.SelectedItem is TournamentParticipant p && Model.Tournament != null)
            {
                LogUserEvent(null, p.Participant.Name);
                if (p.Participant.Active != null)
                {
                    Array.Fill(p.Participant.Active, true);
                }
                Model.SyncParticipants();
            }
        }

        private void MenuItem_Participant_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (lvParticipants?.SelectedItem is TournamentParticipant p && Model.Tournament != null)
            {
                LogUserEvent(null, p.Participant.Name);
                Model.Tournament.Participants.Remove(p.Participant);
                Model.SyncParticipants();
            }
        }

        private void AdjustTabitems()
        {
            while ((Model.Tournament?.Rounds.Count ?? 0) + 1 < MainTabControl.Items.Count)
            {
                MainTabControl.Items.Remove(MainTabControl.Items[^1]);
            }
            for (int tabIndx = MainTabControl.Items.Count; tabIndx <= (Model.Tournament?.Rounds.Count ?? 0); ++tabIndx)
            {
                if (Model.Tournament != null)
                {
                    var content = new Round(Model.Tournament, tabIndx - 1) { DataContext = Model };
                    ((Round)content).ResultSet += (s, e) => Model.SyncRounds();
                    MainTabControl.Items.Add(new TabItem()
                    {
                        Header = $"Round {tabIndx}",
                        Content = content
                    });
                }
            }
        }

        private void MenuItem_Export_Click(object sender, RoutedEventArgs e)
        {
            int tag = int.Parse((string)((MenuItem)sender).Tag);
            LogUserEvent(null, tag.ToString());
            string html = string.Empty;
            string title = string.Empty;
            switch (tag)
            {
                case 0:
                    title = "Participant List by Starting Rank";
                    html = Model.Tournament?.ParticipantListHTML("Rating", true) ?? string.Empty;
                    break;
                case 1:
                    title = "Participant List by Name";
                    html = Model.Tournament?.ParticipantListHTML("Name", false) ?? string.Empty;
                    break;
                case 2:
                    title = "Crosstable";
                    html = Model.Tournament?.CrosstableHTML() ?? string.Empty;
                    break;
                case 3:
                    title = $"Pairings Round {Model.Tournament?.Rounds.Count ?? 0}";
                    html = Model.Tournament?.RoundHTML() ?? string.Empty;
                    break;
            }
            htmlViewer.Html = html;
            htmlViewer.Title = title;
            htmlViewer.Owner = this;
            htmlViewer.ShowDialog();
        }

        private void MenuItem_Add_Participants_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                MenuItem mi = (MenuItem)sender;
                int count = int.Parse((string)mi.Tag);
                LogUserEvent(null, count.ToString());
                Model.AddRandomParticipants(count);
            }
        }

        private void MenuItem_Simulate_Results_Click(object sender, RoutedEventArgs e)
        {
            LogUserEvent();
            Model.SimulateResults();
            var tabitem = MainTabControl.Items[^1] as TabItem;
            Round? r = tabitem?.Content as Round;
            r?.Model.SyncRound();
            Model.SyncRounds();
        }

        private GridViewColumnHeader? lvParticipantsSortCol = null;
        private bool sort_ascending = true;
        private void LvParticipantsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader? column = (sender as GridViewColumnHeader);
            if (column == null) return;
            LogUserEvent(null, column.Name);
            if (column == lvParticipantsSortCol) sort_ascending = !sort_ascending; else sort_ascending = true;
            lvParticipantsSortCol = column;
            string sortCol = column?.Tag?.ToString() ?? string.Empty;
            List<TournamentParticipant>? sortedList = null;
            if (sortCol == "Name")
            {
                sortedList = Model.Participants.OrderBy(x => x.Participant.Name ?? string.Empty).ToList();
            }
            else if (sortCol == "Federation")
            {
                sortedList = Model.Participants.OrderBy(x => x.Participant.Federation).ToList();
            }
            else if (sortCol == "FideId")
            {
                sortedList = Model.Participants.OrderBy(x => x.Participant.FideId).ToList();
            }
            else if (sortCol == "Score")
            {
                sortedList = Model.Participants.OrderBy(x => x.Score).ToList();
            }
            else if (sortCol == "Rating")
            {
                sortedList = Model.Participants.OrderBy(x => Model.Tournament?.Rating(x.Participant)).ToList();
            }
            else if (sortCol == "Id")
            {
                sortedList = Model.Participants.OrderBy(x => x.Participant.ParticipantId ?? string.Empty).ToList();
            }
            else if (sortCol == "Elo")
            {
                sortedList = Model.Participants.OrderBy(x => x.Participant.FideRating).ToList();
            }
            else if (sortCol == "NationalRating")
            {
                sortedList = Model.Participants.OrderBy(x => x.Participant.AlternativeRating).ToList();
            }
            else if (sortCol == "Club")
            {
                sortedList = Model.Participants.OrderBy(x => x.Participant.Club ?? string.Empty).ToList();
            }
            if (!sort_ascending) sortedList?.Reverse();
            if (sortedList != null)
            {
                Model.Participants.Clear();
                foreach (var p in sortedList)
                    Model.Participants.Add(p);
            }
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            LogUserEvent();
            Properties.Settings.Default.Save();
        }

        private async void MenuItem_Tournament_MRU_ClickAsync(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            if (mi == null) return;
            string fileName = mi.Tag?.ToString() ?? string.Empty;
            LogUserEvent("MenuItem_Tournament_MRU_ClickAsync", fileName);
            if (fileName != string.Empty)
            {
                var uiContext = SynchronizationContext.Current;
                MessageDialogResult messageDialogResult = MessageDialogResult.Affirmative;
                if (Model.Tournament != null && TournamentHash != Model.Tournament.Hash())
                {
                    messageDialogResult = await this.ShowMessageAsync($"Load Tournament {System.IO.Path.GetFileNameWithoutExtension(fileName)}", "There might be unsaved data which will be lost! Do you want to proceed?", MessageDialogStyle.AffirmativeAndNegative);
                    if (messageDialogResult == MessageDialogResult.Negative) return;
                }
                uiContext?.Send(x => Load(fileName), null);
            }
        }

        private async void MenuItem_Settings_About_Click(object sender, RoutedEventArgs e)
        {
            LogUserEvent("MenuItem_Settings_About_Click");
            _ = await this.ShowMessageAsync("PonzianiSwiss 0.3.0 - Swiss Pairing Program", "Find more information at https://github.com/guentherc/PonzianiSwiss");
        }

        private void MenuItem_Tournament_Edit_Forbidden_Click(object sender, RoutedEventArgs e)
        {
            if (Model.Tournament == null) return;
            LogUserEvent();
            ForbiddenPairingsDialog dlg = new(Model.Tournament)
            {
                Owner = this
            };
            dlg.ShowDialog();
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            LogUserEvent();
            if (Model.Tournament != null && TournamentHash != Model.Tournament.Hash())
            {
                e.Cancel = true;
                var messageDialogResult = this.ShowModalMessageExternal($"Exit Application", "There might be unsaved data which will be lost! Do you want to proceed?", MessageDialogStyle.AffirmativeAndNegative);
                e.Cancel = messageDialogResult == MessageDialogResult.Negative;
            }
            if (!e.Cancel) Application.Current.Shutdown();
        }

        private void MenuItem_Settings_Reset_Click(object sender, RoutedEventArgs e)
        {
            LogUserEvent();
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
            ThemeManager.Current.ChangeTheme(Application.Current, Properties.Settings.Default.BaseTheme, Properties.Settings.Default.ThemeColor);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DependentPropertiesAttribute : Attribute
    {
        private readonly string[] properties;

        public DependentPropertiesAttribute(params string[] dp)
        {
            properties = dp;
        }

        public string[] Properties
        {
            get
            {
                return properties;
            }
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        protected ILogger? Logger { get; set; }

        protected void RaisePropertyChange([CallerMemberName] string propertyName = "", List<string>? calledProperties = null)
        {
            OnPropertyChanged(propertyName);

            if (calledProperties == null)
            {
                calledProperties = new List<string>();
            }

            calledProperties.Add(propertyName);

            PropertyInfo? pInfo = GetType().GetProperty(propertyName);

            if (pInfo != null)
            {
                foreach (DependentPropertiesAttribute ca in
                  pInfo.GetCustomAttributes(false).OfType<DependentPropertiesAttribute>())
                {
                    if (ca.Properties != null)
                    {
                        foreach (string prop in ca.Properties)
                        {
                            if (prop != propertyName && !calledProperties.Contains(prop))
                            {
                                RaisePropertyChange(prop, calledProperties);
                            }
                        }
                    }
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class MainModel : ObservableObject
    {
        private Tournament? tournament;
        //private int TournamentHash = 0;

        [ObservableProperty]
        private string? fileName;

        [ObservableProperty]
        private MRUModel mRUModel = new();

        private readonly ILogger? Logger;

        private readonly IDialogService? DialogService;
        public ICommand ParticipantDialogCommand { get; set; }
        public ICommand TournamentEditDialogCommand { get; set; }
        public ICommand TournamentAddDialogCommand { get; set; }


        public MainModel(App.Mode mode, ILogger? logger)
        {
            DialogService = App.Current.Services?.GetService<IDialogService>();
            Mode = mode;
            Logger = logger;
            ParticipantDialogCommand = new RelayCommand<TournamentParticipant?>((p) => ParticipantDialog(p), (p) => Tournament != null);
            TournamentEditDialogCommand = new RelayCommand(TournamentEditDialog, () => Tournament != null);
            TournamentAddDialogCommand = new RelayCommand(TournamentAddDialog);
            foreach (var entry in PlayerBaseFactory.AvailableBases)
                MenuEntries.Add(new(entry));
        }

        public App.Mode Mode { get; private set; } = App.Mode.Release;

        internal ObservableCollection<TournamentParticipant> Participants { get; set; } = new();
        public ObservableCollection<MenuEntry> MenuEntries { get; set; } = new();

        [DependentProperties("DrawEnabled", "DeleteLastRoundEnabled")]
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
                    OnPropertyChanged(nameof(DeleteLastRoundEnabled));
                }
            }
        }

        public bool DrawEnabled
        {
            get => Tournament != null && Tournament.Participants.Count > 0
                && (Tournament.Rounds.Count == 0 || !Tournament.Rounds[^1].Pairings.Where(p => p.Result == Result.Open).Any());
        }

        public bool DeleteLastRoundEnabled { get => Tournament != null && Tournament.Rounds.Count > 0; }

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

        private void ShowParticipantDialog(Func<ParticipantDialogViewModel, bool?> showDialog, TournamentParticipant? tournamentParticipant)
        {
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
                }
            }
        }


        private void ShowTournamentDialog(Func<TournamentDialogViewModel, bool?> showDialog, Tournament? tournament)
        {
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

        [ICommand]
        async void Update_Base(PlayerBaseFactory.Base b)
        {
            IPlayerBase pbase = PlayerBaseFactory.Get(b, Logger);

            var controller = await DialogCoordinator.Instance.ShowProgressAsync(this, "Please wait...", $"Update of {pbase.Description} might take some time");
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
            if (ok) await DialogCoordinator.Instance.ShowMessageAsync(this, "Update successful", $"Update of Player Base {b}"); else await DialogCoordinator.Instance.ShowMessageAsync(this, "Update failed", $"Update of Player Base {b}");
            pbase.ProgressChanged -= updateProgressBar;
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
            OnPropertyChanged(nameof(DeleteLastRoundEnabled));
        }

        internal void SimulateResults()
        {
            if (Tournament != null)
            {
                foreach (Pairing pairing in Tournament.Rounds[^1].Pairings)
                {
                    pairing.Result = PonzianiSwissLib.Utils.Simulate(Tournament.Rating(pairing.White), Tournament.Rating(pairing.Black));
                }
                Tournament.GetScorecards();
            }
        }

        internal async void AddRandomParticipants(int count = 100)
        {
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
    }

    public class MRUModel : ViewModel
    {
        private Visibility visibility = Visibility.Collapsed;
        private Visibility visibility1 = Visibility.Collapsed;
        private Visibility visibility2 = Visibility.Collapsed;
        private Visibility visibility3 = Visibility.Collapsed;
        private Visibility visibility4 = Visibility.Collapsed;
        private string? fileName1;
        private string? fileName2;
        private string? fileName3;
        private string? fileName4;
        private string? path1;
        private string? path2;
        private string? path3;
        private string? path4;

        public MRUModel()
        {
            if (Properties.Settings.Default.MRU == null) return;
            int indx = 0;
            foreach (string? file in Properties.Settings.Default.MRU)
            {
                if (file != null && File.Exists(file))
                {
                    ++indx;
                    Visibility = Visibility.Visible;
                    switch (indx)
                    {
                        case 1:
                            Path1 = file;
                            FileName1 = System.IO.Path.GetFileNameWithoutExtension(file);
                            Visibility1 = Visibility.Visible;
                            break;
                        case 2:
                            Path2 = file;
                            FileName2 = System.IO.Path.GetFileNameWithoutExtension(file);
                            Visibility2 = Visibility.Visible;
                            break;
                        case 3:
                            Path3 = file;
                            FileName3 = System.IO.Path.GetFileNameWithoutExtension(file);
                            Visibility3 = Visibility.Visible;
                            break;
                        case 4:
                            Path4 = file;
                            FileName4 = System.IO.Path.GetFileNameWithoutExtension(file);
                            Visibility4 = Visibility.Visible;
                            break;
                    }
                }
            }
        }

        public Visibility Visibility
        {
            get => visibility; set
            {
                visibility = value;
                RaisePropertyChange();
            }
        }
        public Visibility Visibility1
        {
            get => visibility1; set
            {
                visibility1 = value;
                RaisePropertyChange();
            }
        }
        public Visibility Visibility2
        {
            get => visibility2; set
            {
                visibility2 = value;
                RaisePropertyChange();
            }
        }
        public Visibility Visibility3
        {
            get => visibility3; set
            {
                visibility3 = value;
                RaisePropertyChange();
            }
        }
        public Visibility Visibility4
        {
            get => visibility4; set
            {
                visibility4 = value;
                RaisePropertyChange();
            }
        }
        public string? FileName1
        {
            get => fileName1; set
            {
                fileName1 = value;
                RaisePropertyChange();
            }
        }
        public string? FileName2
        {
            get => fileName2; set
            {
                fileName2 = value;
                RaisePropertyChange();
            }
        }
        public string? FileName3
        {
            get => fileName3; set
            {
                fileName3 = value;
                RaisePropertyChange();
            }
        }
        public string? FileName4
        {
            get => fileName4; set
            {
                fileName4 = value;
                RaisePropertyChange();
            }
        }

        public string? Path1
        {
            get => path1; set
            {
                path1 = value;
                RaisePropertyChange();
            }
        }
        public string? Path2
        {
            get => path2; set
            {
                path2 = value;
                RaisePropertyChange();
            }
        }
        public string? Path3
        {
            get => path3; set
            {
                path3 = value;
                RaisePropertyChange();
            }
        }
        public string? Path4
        {
            get => path4; set
            {
                path4 = value;
                RaisePropertyChange();
            }
        }

    }

    internal class TournamentParticipant
    {
        private readonly Tournament? tournament;

        public Participant Participant { set; get; }

        public TournamentParticipant(Tournament? tournament, Participant participant)
        {
            this.tournament = tournament;
            this.Participant = participant;
        }

        public float Score
        {
            get => Participant.Scorecard?.Score() ?? 0;
        }


        public int TournamentRating { get => tournament?.Rating(Participant) ?? 0; }

        /// <summary>
        /// Abandoned párticipants will be renderedwith strikethrough
        /// </summary>
        public TextDecorationCollection? TextDecoration
        {
            get
            {
                if (tournament == null) return null;
                for (int i = tournament.Rounds.Count; i < tournament.CountRounds; ++i)
                {
                    if ((bool)(Participant.Active?.GetValue(i) ?? true))
                    {
                        return null;
                    }
                }
                return TextDecorations.Strikethrough;
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
                bool paused = !((bool)(Participant.Active?.GetValue(tournament.Rounds.Count) ?? true));
                return paused ? FontStyles.Italic : FontStyles.Normal;
            }
        }
    }

    public class MenuEntry
    {
        public MenuEntry(KeyValuePair<PlayerBaseFactory.Base, string> entry)
        {
            Header = $"{entry.Key} ({entry.Value})";
            Key = entry.Key;
        }

        public string Header { get; set; }
        public PlayerBaseFactory.Base Key { get; set; }
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
