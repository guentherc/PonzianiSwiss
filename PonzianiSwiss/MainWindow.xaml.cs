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
                    Model.FileName = openFileDialog.FileName;
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

        [DependentProperties("SaveEnabled", "SaveAsEnabled")]
        public Tournament? Tournament { get => tournament; set { if (tournament != value) { tournament = value; RaisePropertyChange(); } } }
        
        [DependentProperties("SaveEnabled")]
        public string? FileName { get => fileName; set { if (fileName != value) { fileName = value; RaisePropertyChange(); } } }

        public bool SaveAsEnabled => Tournament != null;

        public bool SaveEnabled => Tournament != null && FileName != null && File.Exists(FileName);
    }
}
