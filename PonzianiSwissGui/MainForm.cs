using PonzianiPlayerBase;
using PonzianiSwissLib;

namespace PonzianiSwissGui
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
            editHeaderToolStripMenuItem.Enabled = false;
        }

        private Tournament? _tournament;
        public Tournament? Tournament
        {
            set
            {
                _tournament = value;
                saveToolStripMenuItem.Enabled = _tournament != null;
                saveAsToolStripMenuItem.Enabled = _tournament != null;
                editHeaderToolStripMenuItem.Enabled = _tournament != null;
                if (_tournament != null) Text = _tournament.Name;
            }
            get { return _tournament; }
        }
        public string? FileName { set; get; }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TournamentDialog td = new(new());
            if (td.ShowDialog() == DialogResult.OK)
            {
                Tournament = td.Tournament;
                this.Text = Tournament.Name;
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Tournament != null)
            {
                if (FileName == null) SaveAsToolStripMenuItem_Click(sender, e);
                else File.WriteAllText(FileName, Tournament.Serialize());
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = $"{Properties.Strings.TournamentFiles}|*.tjson|{Properties.Strings.AllFiles}|*.*";
            if (FileName != null) saveFileDialog.FileName = FileName; else saveFileDialog.FileName = Tournament?.Name + ".tjson";
            saveFileDialog.DefaultExt = ".tjson";
            saveFileDialog.Title = Properties.Strings.SaveTournamentFile;
            saveFileDialog.AddExtension = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileName = saveFileDialog.FileName;
                SaveToolStripMenuItem_Click(sender, e);
            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = $"{Properties.Strings.TournamentFiles}|*.tjson|{Properties.Strings.AllFiles}|*.*",
                DefaultExt = ".tjson",
                Title = Properties.Strings.OpenTournamentFile,
                CheckPathExists = true,
                AddExtension = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string json = File.ReadAllText(openFileDialog.FileName);
                Tournament = Extensions.Deserialize(json);
                if (Tournament != null)
                    FileName = openFileDialog.FileName;
            }
        }

        private void EditHeaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TournamentDialog td = Tournament != null ? new TournamentDialog((Tournament)((ICloneable)Tournament).Clone()) : new TournamentDialog(new());
            if (td.ShowDialog() == DialogResult.OK)
            {
                Tournament = td.Tournament;
                this.Text = Tournament.Name;
            }
        }

        private async void UpdateFIDEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateFideToolStripMenuItem.Enabled = false;
            mainStatusLabel.Text = Properties.Strings.PlayerListUpdate;
            IPlayerBase pbase = PlayerBaseFactory.Get("FIDE");
            Player? player = pbase.GetById("1503014");
            await pbase.UpdateAsync().ConfigureAwait(false);
            Invoke((MethodInvoker)(() =>
            {
                updateFideToolStripMenuItem.Enabled = true;
                mainStatusLabel.Text = String.Empty;
            }));
        }
    }
}