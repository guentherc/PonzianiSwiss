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

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Model = new();
            DataContext = Model;
            FideBase = PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE);
            lvParticipants.ItemsSource = Model.Participants;
        }

        public MainModel Model { set; get; }

        private IPlayerBase? FideBase = null;

        private void MenuItem_Tournament_New_Click(object sender, RoutedEventArgs e)
        {
            TournamentDialog td = new(new());
            td.Title = "Create new Tournament";
            if (td.ShowDialog() ?? false)
            {
                Model.Tournament = td.Model.Tournament;
            }
        }

        private void MenuItem_Tournament_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Tournament_Open_Click(object sender, RoutedEventArgs e)
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
                string json = File.ReadAllText(openFileDialog.FileName);
                Model.Tournament = Extensions.Deserialize(json);
                if (Model.Tournament != null)
                {
                    Model.FileName = openFileDialog.FileName;
                    Model.SyncParticipants();
                    Model.SyncRounds();
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
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = $"Tournament Files|*.tjson|All Files|*.*";
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
            TournamentDialog td = new TournamentDialog(Model.Tournament ?? new());
            if (td.ShowDialog() ?? false)
            {
                Model.Tournament = td.Model.Tournament;
            }
        }

        private void MenuItem_Participant_Add_Click(object sender, RoutedEventArgs e)
        {
            ParticipantDialog pd = new(new(), Model.Tournament);
            pd.Title = "Add Participant";
            if (pd.ShowDialog() ?? false)
            {
                Model.Tournament?.Participants.Add(pd.Model.Participant);
                Model.SyncParticipants();
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
            var uiContext = SynchronizationContext.Current;
            Model.Tournament?.GetScorecards();
            if (Model.Tournament != null && await Model.Tournament.DrawAsync(Model.Tournament.Rounds.Count).ConfigureAwait(false))
            {
                Model.Tournament?.GetScorecards();
            }
            uiContext?.Send(x => Model.SyncRounds(), null);
            uiContext?.Send(x => Model.SyncParticipants(), null);
            uiContext?.Send(x => AdjustTabitems(), null);
        }

        private void MenuItem_Participant_Edit_Click(object sender, RoutedEventArgs e)
        {
            TournamentParticipant? p = lvParticipants?.SelectedItem as TournamentParticipant;
            if (p!=null)
            {
                ParticipantDialog pd = new(p.Participant, Model.Tournament);
                pd.Title = $"Edit Participant {p.Participant.Name}";
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
            while ((Model.Tournament?.Rounds.Count ?? 0) + 1  < MainTabControl.Items.Count)
            {
                MainTabControl.Items.Remove(MainTabControl.Items[MainTabControl.Items.Count - 1]);
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

        internal ObservableCollection<TournamentParticipant> Participants { get; } = new();

        [DependentProperties("SaveEnabled", "SaveAsEnabled")]
        public Tournament? Tournament { get => tournament; set { if (tournament != value) { tournament = value; RaisePropertyChange(); } } }
        
        [DependentProperties("SaveEnabled")]
        public string? FileName { get => fileName; set { if (fileName != value) { fileName = value; RaisePropertyChange(); } } }

        public bool SaveAsEnabled => Tournament != null;

        public bool SaveEnabled => Tournament != null && FileName != null && File.Exists(FileName);

        public bool DrawEnabled { get => Tournament != null && Tournament.Participants.Count > 0 
                && (Tournament.Rounds.Count == 0 || !Tournament.Rounds[Tournament.Rounds.Count - 1].Pairings.Where(p => p.Result == Result.Open).Any());
        }

        public bool DeletelastRoundEnabled { get => Tournament != null && Tournament.Rounds.Count > 0; }

        public Visibility ParticipantListVisibility => Tournament?.Participants.Count > 0  ? Visibility.Visible : Visibility.Hidden;

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
    }
    
    internal class TournamentParticipant
    {
        private Tournament? tournament;
        
        public Participant Participant { set; get; }

        public TournamentParticipant(Tournament? tournament, Participant participant)
        {
            this.tournament = tournament;
            this.Participant = participant;
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
