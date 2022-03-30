using PonzianiPlayerBase;
using PonzianiSwissLib;

namespace PonzianiSwissGui
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            participantsToolStripMenuItem.Enabled = false;
        }

        private Tournament? _tournament;
        public Tournament? Tournament
        {
            set
            {
                _tournament = value;
                UpdateUI();
            }
            get { return _tournament; }
        }

        private void UpdateUI()
        {
            saveToolStripMenuItem.Enabled = _tournament != null;
            saveAsToolStripMenuItem.Enabled = _tournament != null;
            editHeaderToolStripMenuItem.Enabled = _tournament != null;
            roundToolStripMenuItem.Enabled = _tournament != null;
            exportToolStripMenuItem.Enabled = _tournament != null;
            tRFToolStripMenuItem.Enabled = _tournament != null && _tournament.Rounds.Count > 0;
            if (_tournament != null) Text = _tournament.Name;
            participantsToolStripMenuItem.Enabled = true;
            lvParticipants.Items.Clear();
            deleteLastRoundToolStripMenuItem.Enabled = _tournament != null && _tournament.Rounds.Count > 0;
            if (_tournament != null)
            {
                foreach (var p in _tournament.Participants)
                {
                    AddPlayerToListView(p);
                }
                foreach (TabPage page in tcMain.TabPages)
                {
                    if (page.Name.StartsWith("tpRound_"))
                    {
                        int i = int.Parse(page.Name[8..]);
                        if (i >= _tournament.Rounds.Count) tcMain.TabPages.Remove(page);
                    }
                }
                int indx = 0;
                foreach (var round in _tournament.Rounds)
                {
                    string id = $"tpRound_{indx}";
                    int tabIndx = tcMain.TabPages.IndexOfKey(id);
                    if (tabIndx == -1)
                    {
                        TabPage tp = new($"{Properties.Strings.Round} {indx + 1}");
                        tp.Name = id;
                        tcMain.TabPages.Add(tp);
                        ListView lv = new();
                        lv.Name = $"lvRound_{indx}";
                        lv.View = View.Details;
                        tp.Controls.Add(lv);
                        lv.Dock = DockStyle.Fill;
                        lv.ContextMenuStrip = cmsSetResult;
                        lv.UseCompatibleStateImageBehavior = false;
                        lv.Columns.Add(Properties.Strings.TournamentIdShort, 40);
                        lv.Columns.Add(Properties.Strings.White, 150);
                        lv.Columns.Add(Properties.Strings.TournamentIdShort, 40);
                        lv.Columns.Add(Properties.Strings.Black, 150);
                        lv.Columns.Add(Properties.Strings.Result, 80);
                        lv.MouseDown += Lv_MouseDown;
                        lv.FullRowSelect = true;
                    }
                    TabPage tabPage = indx >= 0 ? tcMain.TabPages[indx + 1] : tcMain.TabPages[^1];
                    ListView lvr = (ListView)tabPage.Controls[0];
                    lvr.Items.Clear();
                    foreach (var p in round.Pairings)
                    {
                        ListViewItem lvi = new(p.White.ParticipantId);
                        lvi.SubItems.Add($"{p.White.Name?.Trim()} ({p.White.Scorecard?.Score(indx)})");
                        lvi.SubItems.Add(p.Black.ParticipantId);
                        lvi.SubItems.Add($"{p.Black.Name?.Trim()} ({p.Black.Scorecard?.Score(indx)})");
                        lvi.SubItems.Add(result_strings[(int)p.Result]);
                        lvi.Tag = p;
                        lvi.BackColor = p.Result == Result.Open ? Color.White : Color.LightGray;
                        lvr.Items.Add(lvi);
                    }
                    ++indx;
                }
            }
            Invalidate();
        }

        private ListViewItem? selectedItem = null;
        private void Lv_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (sender is ListView listView)
                {
                    selectedItem = listView.GetItemAt(e.X, e.Y);
                }
            }
        }

        private void AddPlayerToListView(Participant p)
        {
            ListViewItem item = new(p.Name);
            item.SubItems.Add(p.FideId.ToString());
            item.SubItems.Add(p.TournamentRating.ToString());
            item.SubItems.Add(p.ParticipantId?.ToString());
            item.Tag = p;
            lvParticipants.Items.Add(item);
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
            PlayerBaseUpdateToolStripMenuItem.Enabled = false;
            updateFideToolStripMenuItem.Enabled = false;
            mainStatusLabel.Text = Properties.Strings.PlayerListUpdate;
            IPlayerBase pbase = PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE);
            await pbase.UpdateAsync().ConfigureAwait(false);
            Invoke((MethodInvoker)(() =>
            {
                PlayerBaseUpdateToolStripMenuItem.Enabled = true;
                updateFideToolStripMenuItem.Enabled = true;
                mainStatusLabel.Text = String.Empty;
            }));
        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Tournament == null) return;
            PlayerDialog pd = new(new());
            if (pd.ShowDialog() == DialogResult.OK)
            {
                Tournament.Participants.Add(pd.Player);
                AddPlayerToListView(pd.Player);
            }
        }

        private async void CreateTestTournamentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            _tournament = await PairingTool.GenerateAsync().ConfigureAwait(false);
            _tournament?.GetScorecards();
            Invoke((MethodInvoker)(() =>
            {
                UpdateUI();
                Cursor = Cursors.Default;
            }));
        }

        internal static readonly string[] result_strings = new string[14] {
            "*",
            "--+",
            "0-1",
            "0-1",
            Properties.Strings.Bye + " 0",
            Properties.Strings.Bye,
            "1/2-1/2",
            "1/2-1/2",
            Properties.Strings.Bye + " 1/2",
            "1-0",
            "1-0",
            "+--",
            Properties.Strings.Bye + " 1",
            "---"
        };

        private void DeleteLastRoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _tournament?.Rounds.Remove(_tournament.Rounds.Last());
            UpdateUI();
        }

        private void SetResultToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (selectedItem != null)
            {
                Result r = (Result)int.Parse((string)((ToolStripMenuItem)sender).Tag);
                ((Pairing)selectedItem.Tag).Result = r;
                selectedItem.SubItems[4].Text = result_strings[(int)r];
                selectedItem.BackColor = r == Result.Open ? Color.White : Color.LightGray;
                Invalidate(selectedItem.Bounds);
            }
        }

        private void CmsSetResult_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (selectedItem == null) e.Cancel = true;
        }

        private async void DrawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            _tournament?.GetScorecards();
            if (_tournament != null && await _tournament.DrawAsync(_tournament.Rounds.Count).ConfigureAwait(false))
            {
                _tournament?.GetScorecards();
                Invoke((MethodInvoker)(() =>
                {
                    UpdateUI();
                    Cursor = Cursors.Default;
                }));
            }
        }

        private void TRFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = $"{Properties.Strings.TournamentReportFile}|*.trf|{Properties.Strings.AllFiles}|*.*";
            if (FileName != null) saveFileDialog.FileName = FileName; else saveFileDialog.FileName = Tournament?.Name + ".trf";
            saveFileDialog.DefaultExt = ".trf";
            saveFileDialog.Title = Properties.Strings.ExportTRF;
            saveFileDialog.AddExtension = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK && _tournament != null && _tournament.Rounds.Count > 0)
            {
                var lines = _tournament?.CreateTRF(_tournament.Rounds.Count);
                if (lines != null) File.WriteAllLines(saveFileDialog.FileName, lines);
            }
        }

        private async void TestTRFCreationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool result = await Extensions.TestTRFGeneration();
            Invoke((MethodInvoker)(() =>
            {
                if (result)
                    MessageBox.Show("Test successful", "Test TRF Generation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Error", "Test TRF Generation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }));
        }

        private async void GERToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlayerBaseUpdateToolStripMenuItem.Enabled = false;
            gERToolStripMenuItem.Enabled = false;
            mainStatusLabel.Text = Properties.Strings.PlayerListUpdate;
            IPlayerBase pbase = PlayerBaseFactory.Get(PlayerBaseFactory.Base.GER);
            await pbase.UpdateAsync().ConfigureAwait(false);
            Invoke((MethodInvoker)(() =>
            {
                PlayerBaseUpdateToolStripMenuItem.Enabled = true;
                gERToolStripMenuItem.Enabled = true;
                mainStatusLabel.Text = String.Empty;
            }));
        }
    }
}