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
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Extensions = PonzianiSwissLib.Extensions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Reflection;
using PonzianiPlayerBase;
using System.Collections.ObjectModel;
using System.Threading;
using ControlzEx.Theming;
using System.Collections.Specialized;
using MahApps.Metro.Controls.Dialogs;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(App.Mode mode = App.Mode.Release)
        {
            InitializeComponent();
            ThemeManager.Current.ChangeTheme(Application.Current, Properties.Settings.Default.BaseTheme, Properties.Settings.Default.ThemeColor);
            //Add Playerbase Update entries dynamically
            foreach (var entry in PlayerBaseFactory.AvailableBases)
            {
                MenuItem mi = new()
                {
                    Header = $"{entry.Key} ({entry.Value})",
                    Tag = entry.Key,
                };
                mi.Click += Update_Base;
                MenuItem_PlayerBase_Update.Items.Add(mi);
            }
            RenderThemeMenuEntries();
            //MenuItem_PlayerBase_Update.
            Model = new()
            {
                Mode = mode
            };
            DataContext = Model;
            PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE);
            lvParticipants.ItemsSource = Model.Participants;
            _ = FederationUtil.GetFederations();
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
            ThemeManager.Current.ChangeTheme(Application.Current, Properties.Settings.Default.BaseTheme, Properties.Settings.Default.ThemeColor);
            RenderThemeMenuEntries();
        }

        private void Update_BaseTheme(object sender, RoutedEventArgs e)
        {
            MenuItem? mi = sender as MenuItem;
            Properties.Settings.Default.BaseTheme = mi?.Tag.ToString() ?? "Light";
            ThemeManager.Current.ChangeTheme(Application.Current, Properties.Settings.Default.BaseTheme, Properties.Settings.Default.ThemeColor);
            RenderThemeMenuEntries();
        }

        private async void Update_Base(object sender, RoutedEventArgs e)
        {
            var uiContext = SynchronizationContext.Current;
            if (sender is not MenuItem mi) return;
            mi.IsEnabled = false;
            var b = (PlayerBaseFactory.Base)mi.Tag;
            IPlayerBase pbase = PlayerBaseFactory.Get(b);
            bool ok = await pbase.UpdateAsync();
            if (ok)
                uiContext?.Send(x =>
                {
                    MessageBox.Show(this, "Update successful", $"Update of Player Base {b}", MessageBoxButton.OK, MessageBoxImage.Information);
                }, null);
            else
                uiContext?.Send(x =>
                {
                    MessageBox.Show(this, "Update failed", $"Update of Player Base {b}", MessageBoxButton.OK, MessageBoxImage.Error);
                }, null);
            uiContext?.Send(x => { mi.IsEnabled = true; }, null);
        }

        public MainModel Model { set; get; }
        private readonly HTMLViewer htmlViewer = new();

        private void MenuItem_Tournament_New_Click(object sender, RoutedEventArgs e)
        {
            if (Model.Tournament != null)
            {
                if (MessageBox.Show(this, "There might be unsaved data!", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel) return;
            }
            TournamentDialog td = new(new())
            {
                Title = "Create new Tournament",
                Owner = this
            };
            if (td.ShowDialog() ?? false)
            {
                Model.Tournament = td.Model.Tournament;
                Model.Participants.Clear();                
            } 
        }

        private void MenuItem_Tournament_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Tournament_Open_Click(object sender, RoutedEventArgs e)
        {
            if (Model.Tournament != null)
            {
                if (MessageBox.Show(this, "There might be unsaved data!", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel) return;
            }
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
            string json = File.ReadAllText(filename);
            Model.Tournament = Extensions.Deserialize(json);
            if (Model.Tournament != null)
            {
                while (Properties.Settings.Default.MRU.Count > 10) 
                    Properties.Settings.Default.MRU.RemoveAt(10);
                Model.FileName = filename;
                Model.SyncParticipants();
                Model.SyncRounds();
                if (Properties.Settings.Default.MRU == null) Properties.Settings.Default.MRU = new StringCollection();
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

            }
        }

        private void MenuItem_Tournament_Save_Click(object sender, RoutedEventArgs e)
        {
            if (Model.Tournament != null)
            {
                if (Model.FileName == null) MenuItem_Tournament_Save_As_Click(sender, e);
                else File.WriteAllText(Model.FileName, Model.Tournament.Serialize());
            }
        }

        private void MenuItem_Tournament_Save_As_Click(object sender, RoutedEventArgs e)
        {
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

        private void MenuItem_Tournament_Edit_Click(object sender, RoutedEventArgs e)
        {
            TournamentDialog td = new(Model.Tournament ?? new());
            td.Owner = this;
            if (td.ShowDialog() ?? false)
            {
                Model.Tournament = td.Model.Tournament;
            }
        }

        private void MenuItem_Participant_Add_Click(object sender, RoutedEventArgs e)
        {
            ParticipantDialog pd = new(new(), Model.Tournament)
            {
                Title = "Add Participant",
                Owner = this
            };
            if (pd.ShowDialog() ?? false)
            {
                Model.Tournament?.Participants.Add(pd.Model.Participant);
                Model.SyncParticipants();
                Model.SyncRounds();
            }
        }

        private void MenuItem_Round_Delete_Click(object sender, RoutedEventArgs e)
        {
            Model.Tournament?.Rounds.Remove(Model.Tournament.Rounds.Last());
            Model.Tournament?.OrderByRank();
            AdjustTabitems();
            Model.SyncRounds();
        }

        private async void MenuItem_Round_Draw_Click(object sender, RoutedEventArgs e)
        {
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

        private void MenuItem_Participant_Edit_Click(object sender, RoutedEventArgs e)
        {
            TournamentParticipant? p = lvParticipants?.SelectedItem as TournamentParticipant;
            if (p != null)
            {
                ParticipantDialog pd = new(p.Participant, Model.Tournament)
                {
                    Title = $"Edit Participant {p.Participant.Name}",
                    Owner = this
                };
                if (pd.ShowDialog() ?? false)
                {
                    Model.SyncParticipants();
                }
            }
        }

        private void MenuItem_Participant_Abandon_Click(object sender, RoutedEventArgs e)
        {
            TournamentParticipant? p = lvParticipants?.SelectedItem as TournamentParticipant;
            if (p != null && Model.Tournament != null)
            {
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
            TournamentParticipant? p = lvParticipants?.SelectedItem as TournamentParticipant;
            if (p != null && Model.Tournament != null)
            {
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
            TournamentParticipant? p = lvParticipants?.SelectedItem as TournamentParticipant;
            if (p != null && Model.Tournament != null)
            {
                if (p.Participant.Active != null)
                {
                    Array.Fill(p.Participant.Active, true);
                }
                Model.SyncParticipants();
            }
        }

        private void MenuItem_Participant_Delete_Click(object sender, RoutedEventArgs e)
        {
            TournamentParticipant? p = lvParticipants?.SelectedItem as TournamentParticipant;
            if (p != null && Model.Tournament != null)
            {
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
                Model.AddRandomParticipants(count);
            }
        }

        private void MenuItem_Simulate_Results_Click(object sender, RoutedEventArgs e)
        {
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
            Properties.Settings.Default.Save();
        }

        private void MenuItem_Tournament_MRU_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            if (mi == null) return;
            string fileName = mi.Tag?.ToString() ?? string.Empty;
            if (fileName != string.Empty)
            {
                Load(fileName);
            }
        }

        private async void MenuItem_Settings_About_Click(object sender, RoutedEventArgs e)
        {
            _ = await this.ShowMessageAsync("PonzianiSwiss 0.2.0 - Swiss Pairing Program", "Find more information at https://github.com/guentherc/PonzianiSwiss");
        }

        private void MenuItem_Tournament_Edit_Forbidden_Click(object sender, RoutedEventArgs e)
        {
            if (Model.Tournament == null) return;
            ForbiddenPairingsDialog dlg = new(Model.Tournament);
            dlg.Owner = this;
            dlg.ShowDialog();
        }
    }

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

    public class MainModel : ViewModel
    {
        private Tournament? tournament;
        private string? fileName;
        private MRUModel mRUModel = new();

        public App.Mode Mode { get; set; } = App.Mode.Release;

        internal ObservableCollection<TournamentParticipant> Participants { get; set; } = new();

        [DependentProperties("DrawEnabled", "DeleteLastRoundEnabled")]
        public Tournament? Tournament { get => tournament; set { if (tournament != value) { tournament = value; RaisePropertyChange(); } } }

        public string? FileName { get => fileName; set { if (fileName != value) { fileName = value; RaisePropertyChange(); } } }
        
        public MRUModel MRUModel { get => mRUModel; set { mRUModel = value; RaisePropertyChange(); } }
        public bool DrawEnabled
        {
            get => Tournament != null && Tournament.Participants.Count > 0
                && (Tournament.Rounds.Count == 0 || !Tournament.Rounds[^1].Pairings.Where(p => p.Result == Result.Open).Any());
        }

        public bool DeletelastRoundEnabled { get => Tournament != null && Tournament.Rounds.Count > 0; }

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
            RaisePropertyChange("ParticipantListVisibility");
        }

        internal void SyncRounds()
        {
            RaisePropertyChange("DrawEnabled");
            RaisePropertyChange("DeleteLastRoundEnabled");
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
            FidePlayerBase fide_base = (FidePlayerBase)PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE);
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
}
