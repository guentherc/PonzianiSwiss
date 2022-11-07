using CommunityToolkit.Mvvm.ComponentModel;
using MahApps.Metro.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using MvvmDialogs;
using System.IO;
using System.Windows;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for HTMLViewer.xaml
    /// </summary>
    public partial class HTMLViewer : MetroWindow
    {

        public HTMLViewer()
        {
            InitializeComponent();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new()
            {
                Filter = $"HTML  Files|*.html|PDF Files|*.pdf|All Files|*.*",
                FileName = $"{Title}.html",
                AddExtension = true
            };
            if (dialog.ShowDialog() ?? false)
            {
                if (Path.GetExtension(dialog.FileName) == ".pdf")
                {
                    await HTMLView.CoreWebView2.PrintToPdfAsync(dialog.FileName);
                }
                else File.WriteAllText(dialog.FileName, ((HTMLViewerViewModel)DataContext).Html);
            }
        }

        private async void HTMLViewer_Load(object sender, RoutedEventArgs e)
        {
            if (((HTMLViewerViewModel)DataContext).Html != null)
            {
                await HTMLView.EnsureCoreWebView2Async();
                HTMLView.NavigateToString(((HTMLViewerViewModel)DataContext).Html);
            }
        }
    }

    public partial class HTMLViewerViewModel : ViewModel, IModalDialogViewModel
    {
        [ObservableProperty]
        private bool? dialogResult;

        [ObservableProperty]
        private string? html;

        [ObservableProperty]
        private string? title;

        public HTMLViewerViewModel(ILogger logger) => Logger = logger;
    }

}
