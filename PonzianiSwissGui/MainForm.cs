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

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TournamentDialog td = new TournamentDialog(new());
            if (td.ShowDialog() == DialogResult.OK)
            {
                Tournament = td.Tournament;
                this.Text = Tournament.Name;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Tournament != null)
            {
                if (FileName == null) saveAsToolStripMenuItem_Click(sender, e);
                else File.WriteAllText(FileName, Tournament.Serialize());
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = $"{Properties.Strings.TournamentFiles}|*.tjson|{Properties.Strings.AllFiles}|*.*";
            if (FileName != null) saveFileDialog.FileName = FileName; else saveFileDialog.FileName = Tournament?.Name + ".tjson";
            saveFileDialog.DefaultExt = ".tjson";
            saveFileDialog.Title = Properties.Strings.SaveTournamentFile;
            saveFileDialog.AddExtension = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileName = saveFileDialog.FileName;
                saveToolStripMenuItem_Click(sender, e);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = $"{Properties.Strings.TournamentFiles}|*.tjson|{Properties.Strings.AllFiles}|*.*";
            openFileDialog.DefaultExt = ".tjson";
            openFileDialog.Title = Properties.Strings.OpenTournamentFile;
            openFileDialog.CheckPathExists = true;
            openFileDialog.AddExtension = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string json = File.ReadAllText(openFileDialog.FileName);
                Tournament = Extensions.Deserialize(json);
                if (Tournament != null)
                    FileName = openFileDialog.FileName;
            }
        }

        private void editHeaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TournamentDialog td = new TournamentDialog((Tournament)((ICloneable)Tournament)?.Clone() ?? new Tournament());
            if (td.ShowDialog() == DialogResult.OK)
            {
                Tournament = td.Tournament;
                this.Text = Tournament.Name;
            }
        }

        private async void updateFIDEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateFideToolStripMenuItem.Enabled = false;
            mainStatusLabel.Text = Properties.Strings.PlayerListUpdate;
            IPlayerBase pbase = await PlayerBaseFactory.GetAsync("FIDE").ConfigureAwait(false);
            await pbase.UpdateAsync().ConfigureAwait(false);
            Invoke((MethodInvoker)(() =>
            {
                updateFideToolStripMenuItem.Enabled = true;
                mainStatusLabel.Text = String.Empty;
            }));
        }
    }
}