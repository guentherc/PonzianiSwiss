using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro.Controls;
using Microsoft.Extensions.Logging;
using MvvmDialogs;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for TiebreakDialog.xaml
    /// </summary>
    public partial class TiebreakDialog : MetroWindow
    {
        public TiebreakDialog()
        {
            InitializeComponent();

        }
    }

    public partial class TiebreakDialogViewModel : ViewModel, IModalDialogViewModel, IDropTarget
    {
        private List<TieBreak>? tiebreaks;

        [ObservableProperty]
        private ObservableCollection<TiebreakExtended> available = [];

        [ObservableProperty]
        private ObservableCollection<TiebreakExtended> selected = [];

        public List<TieBreak>? Tiebreaks
        {
            get => tiebreaks;

            set
            {
                tiebreaks = value;
                Initialize();
            }
        }

        [ObservableProperty]
        public bool? dialogResult;

        public TiebreakDialogViewModel(ILogger logger)
        {
            Logger = logger;
        }

        private void Initialize()
        {
            var tiebreaks = Enum.GetValues<TieBreak>();
            foreach (var tiebreak in tiebreaks)
            {
                bool selected = Tiebreaks?.Contains(tiebreak) ?? false;
                TiebreakExtended tb = new(tiebreak, selected);
                Available.Add(tb);
                if (selected) Selected.Add(tb);
            }
        }

        private bool IsSelected(TieBreak tb)
        {
            return Selected.Any(t => t.Key == tb);
        }

        [RelayCommand]
        void Update()
        {
            Tiebreaks?.Clear();
            foreach (var tiebreak in Selected) Tiebreaks?.Add(tiebreak.Key);
        }

        [RelayCommand]
        void Select(TieBreak tb)
        {
            if (!IsSelected(tb))
            {
                var tbo = Available.First(t => t.Key == tb);
                tbo.IsSelected = true;
                Selected.Add(tbo);
            }
        }

        [RelayCommand]
        void Unselect(TieBreak tb)
        {
            if (IsSelected(tb))
            {
                var tbo = Selected.First(t => t.Key == tb);
                tbo.IsSelected = false;
                Selected.Remove(tbo);
            }
        }

        [RelayCommand]
        void MoveUp(TiebreakExtended tb)
        {
            int index = Selected.IndexOf(tb);
            if (index > 0)
            {
                Selected.RemoveAt(index);
                Selected.Insert(index - 1, tb);
            }
        }

        [RelayCommand]
        void MoveDown(TiebreakExtended tb)
        {
            int index = Selected.IndexOf(tb);
            if (index < Selected.Count - 1)
            {
                Selected.RemoveAt(index);
                Selected.Insert(index + 1, tb);
            }
        }

        [RelayCommand]
        void Ok()
        {
            Update();
            DialogResult = true;
        }

        [RelayCommand]
        void Cancel()
        {
            DialogResult = false;
        }

        [RelayCommand]
        void Toggle(object parameter)
        {
            TieBreak clickedTiebreak = (TieBreak)parameter;
            if (IsSelected(clickedTiebreak))
                Unselect(clickedTiebreak);
            else
                Select(clickedTiebreak);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.SourceCollection == Available && ((TiebreakExtended)dropInfo.Data).IsSelected)
            {

            }
            else if (dropInfo.TargetCollection == Available && dropInfo.DragInfo.SourceCollection == Available)
            {

            }
            else
            {
                if (dropInfo.DragInfo.SourceCollection == Selected && dropInfo.TargetCollection == Available)
                {
                    dropInfo.Effects = DragDropEffects.Move;
                }
                else
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                    dropInfo.Effects = DragDropEffects.Move;
                }
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.SourceCollection == Selected && dropInfo.TargetCollection == Selected)
            {
                bool up = dropInfo.InsertIndex < dropInfo.DragInfo.SourceIndex;
                int offset = up ? 0 : -1;
                Selected.Remove((TiebreakExtended)dropInfo.Data);
                if (dropInfo.InsertIndex < Selected.Count)
                    Selected.Insert(Math.Clamp(dropInfo.InsertIndex + offset, 0, Selected.Count - 1), (TiebreakExtended)dropInfo.Data);
                else
                    Selected.Add((TiebreakExtended)dropInfo.Data);
            }
            else if (dropInfo.DragInfo.SourceCollection == Available && dropInfo.TargetCollection == Selected)
            {
                if (dropInfo.InsertIndex < Selected.Count)
                    Selected.Insert(Math.Clamp(dropInfo.InsertIndex, 0, Selected.Count - 1), (TiebreakExtended)dropInfo.Data);
                else
                    Selected.Add((TiebreakExtended)dropInfo.Data);
                ((TiebreakExtended)dropInfo.Data).IsSelected = true;
                Available.Remove((TiebreakExtended)dropInfo.Data);
                if (dropInfo.DragInfo.SourceIndex < Available.Count)
                    Available.Insert(Math.Clamp(dropInfo.DragInfo.SourceIndex, 0, Selected.Count - 1), (TiebreakExtended)dropInfo.Data);
                else
                    Available.Add((TiebreakExtended)dropInfo.Data);
            }
            else if (dropInfo.DragInfo.SourceCollection == Selected && dropInfo.TargetCollection == Available)
            {
                Selected.Remove((TiebreakExtended)dropInfo.Data);
                ((TiebreakExtended)dropInfo.Data).IsSelected = false;
                int indx = Math.Clamp(Available.IndexOf((TiebreakExtended)dropInfo.Data), 0, Available.Count);
                Available.RemoveAt(indx);
                if (indx < Available.Count)
                    Available.Insert(indx, (TiebreakExtended)dropInfo.Data);
                else
                    Available.Add((TiebreakExtended)dropInfo.Data);
            }

        }
    }

    public class TiebreakExtended(TieBreak key, bool isSelected = false)
    {
        public TieBreak Key { get; set; } = key;

        public string Text => Key.ToString();
        public bool IsSelected { get; set; } = isSelected;
    }

}
