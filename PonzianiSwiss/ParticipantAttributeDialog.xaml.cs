using MahApps.Metro.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MvvmDialogs;
using PonzianiSwiss.Resources;
using PonzianiSwissLib;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaktionslogik für ParticipantAttributeDialog.xaml
    /// </summary>
    public partial class ParticipantAttributeDialog : MetroWindow
    {
        public ParticipantAttributeDialog()
        {
            InitializeComponent();
        }
    }

    public partial class ParticipantAttributeDialogViewModel : ViewModel, IModalDialogViewModel
    {
        [ObservableProperty]
        public bool? dialogResult;

        private Participant? participant;
        private readonly IDialogService dialogService;

        public ObservableCollection<CustomAttributeEntry> CustomAttributes { set; get; } = new();

        public Participant? Participant
        {
            get => participant;
            set
            {
                participant = value;
                if (Participant != null)
                {
                    foreach (var entry in Participant.CustomAttributes)
                    {
                        CustomAttributes.Add(new(entry.Key, entry.Value ?? String.Empty));
                    }
                }
                OnPropertyChanged(nameof(Participant));
            }
        }

        public ParticipantAttributeDialogViewModel(ILogger logger, IDialogService dialogService)
        {
            Logger = logger;
            this.dialogService = dialogService;
        }

        private bool CheckForDuplicates()
        {
            var empty = CustomAttributes.Where(a => String.IsNullOrWhiteSpace(a.Value) && String.IsNullOrWhiteSpace(a.Key)).Skip(1).ToList();
            foreach (var ca in empty)
            {
                CustomAttributes.Remove(ca);
            }

            foreach (var ca in CustomAttributes) ca.IsDuplicate = false;
            var duplicates = CustomAttributes.Where(a => !string.IsNullOrEmpty(a.Key)).GroupBy(n => n.Key.Trim().ToUpper()).SelectMany(s => s.Skip(1));
            foreach (var ca in duplicates)
            {
                ca.IsDuplicate = true;
            }
            return duplicates.Any();
        }

        [ICommand]
        void Validate()
        {
            CheckForDuplicates();
            OnPropertyChanged(nameof(CustomAttributes));
        }

        [ICommand]
        void Ok()
        {
            if (Participant != null)
            {
                var empty = CustomAttributes.Where(a => String.IsNullOrWhiteSpace(a.Value) || String.IsNullOrWhiteSpace(a.Key)).ToList();
                foreach (var ca in empty) CustomAttributes.Remove(ca);
                if (CheckForDuplicates())
                {
                    dialogService.ShowMessageBox(this, LocalizedStrings.Get("ParticipantAttributeDialog_DuplicateMessage_Text", CustomAttributes.Where(a => a.IsDuplicate).First().Key),
                                                        LocalizedStrings.Instance["ParticipantAttributeDialog_DuplicateMessage_Caption"], MessageBoxButton.OK, MessageBoxImage.Error);
                    OnPropertyChanged(nameof(CustomAttributes));
                    return;
                }
                else
                {
                    Participant.CustomAttributes.Clear();
                    foreach (CustomAttributeEntry entry in CustomAttributes)
                    {
                        if (!string.IsNullOrEmpty(entry.Key.Trim()) && !string.IsNullOrEmpty(entry.Value.Trim()))
                        {
                            Participant.CustomAttributes.Add(entry.Key, entry.Value);
                        }
                    }
                }
            }
            DialogResult = true;
        }

        [ICommand]
        void Cancel()
        {
            DialogResult = false;
        }
    }

    public partial class CustomAttributeEntry : ObservableObject
    {
        [ObservableProperty]
        private string key = string.Empty;

        [ObservableProperty]
        private string value = string.Empty;

        [ObservableProperty]
        private bool isDuplicate = false;

        public CustomAttributeEntry()
        {
        }

        public CustomAttributeEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

}
