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
            tsbAdd.Enabled = false;
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
            saveToolStripMenuItem.Enabled = tsbSave.Enabled = _tournament != null;
            saveAsToolStripMenuItem.Enabled = _tournament != null;
            editHeaderToolStripMenuItem.Enabled = tsbEdit.Enabled = _tournament != null;
            roundToolStripMenuItem.Enabled = _tournament != null;
            exportToolStripMenuItem.Enabled = _tournament != null;
            tRFToolStripMenuItem.Enabled = _tournament != null && _tournament.Rounds.Count > 0;
            if (_tournament != null) Text = _tournament.Name;
            participantsToolStripMenuItem.Enabled = tsbAdd.Enabled = true;
            lvParticipants.Items.Clear();
            deleteLastRoundToolStripMenuItem.Enabled = _tournament != null && _tournament.Rounds.Count > 0;
            drawToolStripMenuItem.Enabled = tsbDraw.Enabled = _tournament != null && _tournament.DrawNextRoundPossible;
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
                int selTabIndx = tcMain.SelectedIndex;
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
                        ListViewItem lvi = new(p.White.RankId);
                        lvi.SubItems.Add($"{p.White.Name?.Trim()} ({p.White.Scorecard?.Score(indx)})");
                        lvi.SubItems.Add(p.Black.RankId);
                        lvi.SubItems.Add($"{p.Black.Name?.Trim()} ({p.Black.Scorecard?.Score(indx)})");
                        lvi.SubItems.Add(result_strings[(int)p.Result]);
                        lvi.Tag = p;
                        lvi.BackColor = p.Result == Result.Open ? Color.White : Color.LightGray;
                        lvr.Items.Add(lvi);
                    }
                    ++indx;
                }
                tcMain.SelectedIndex = selTabIndx;
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
            item.SubItems.Add(Tournament?.Rating(p).ToString());
            item.SubItems.Add(p.RankId?.ToString());
            item.SubItems.Add(p.FideRating > 0 ? p.FideRating.ToString() : string.Empty);
            item.SubItems.Add(p.AlternativeRating > 0 ? p.AlternativeRating.ToString() : string.Empty);
            item.SubItems.Add(p.Club ?? string.Empty);
            item.Tag = p;
            if (p.Active != null && Tournament != null)
            {
                bool abandoned = true;
                bool paused = !p.Active[Tournament.Rounds.Count];
                for (int i = Tournament.Rounds.Count; i < Tournament.CountRounds; ++i)
                {
                    if (p.Active[i])
                    {
                        abandoned = false;
                        break;
                    }
                }
                if (abandoned)
                {
                    item.Font = new Font(item.Font, FontStyle.Strikeout);
                }
                else if (paused)
                {
                    item.Font = new Font(item.Font, FontStyle.Italic);
                }
            }
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
            PlayerDialog pd = new(new(), _tournament);
            if (pd.ShowDialog() == DialogResult.OK && pd.Player != null && pd.Player.Name != null && pd.Player.Name.Trim().Length > 0)
            {
                Tournament.Participants.RemoveAll(p => p.FideId > 0 && p.FideId == pd.Player.FideId);
                Tournament.Participants.Add(pd.Player);
            }
            UpdateUI();
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
            _tournament?.OrderByRank();
            UpdateUI();
        }

        private void SetResultToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (selectedItem != null && selectedItem.Tag != null && sender is ToolStripMenuItem item && item.Tag != null)
            {
                Result r = (Result)int.Parse((string)item.Tag);
                ((Pairing)selectedItem.Tag).Result = r;
                selectedItem.SubItems[4].Text = result_strings[(int)r];
                selectedItem.BackColor = r == Result.Open ? Color.White : Color.LightGray;
                drawToolStripMenuItem.Enabled = tsbDraw.Enabled = _tournament != null && _tournament.DrawNextRoundPossible;
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
                    tcMain.SelectedIndex = tcMain.TabPages.Count - 1;
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)(() =>
            {
                PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE);
            }));
        }

        private string? SortedColumnId = null;
        private bool SortAscending = false;
        private void LvParticipants_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ColumnHeader ch = ((ListView)sender).Columns[e.Column];
            if (SortedColumnId != null && SortedColumnId == ch.Name)
            {
                SortAscending = !SortAscending;
            }
            else
            {
                SortAscending = false;
                SortedColumnId = ch.Name;
            }
            if (ch == chName)
            {
                Tournament?.Participants.Sort((p1, p2) => SortAscending ? (p2.Name ?? String.Empty).CompareTo(p1.Name) : (p1.Name ?? String.Empty).CompareTo(p2.Name));
            }
            else if (ch == chTournamentId)
            {
                Tournament?.Participants.Sort((p1, p2) => SortAscending ? (p2.ParticipantId ?? String.Empty).CompareTo(p1.ParticipantId) : (p1.ParticipantId ?? String.Empty).CompareTo(p2.ParticipantId));
            }
            else if (ch == chFideId)
            {
                Tournament?.Participants.Sort((p1, p2) => SortAscending ? p2.FideId.CompareTo(p1.FideId) : p1.FideId.CompareTo(p2.FideId));
            }
            else if (ch == chRating)
            {
                Tournament?.Participants.Sort((p1, p2) => SortAscending ? (Tournament?.Rating(p2) ?? 0).CompareTo(Tournament?.Rating(p1) ?? 0) : (Tournament?.Rating(p1) ?? 0).CompareTo(Tournament?.Rating(p2) ?? 0));
            }
            else if (ch == chFideRating)
            {
                Tournament?.Participants.Sort((p1, p2) => SortAscending ? p2.FideRating.CompareTo(p1.FideRating) : p1.FideRating.CompareTo(p2.FideRating));
            }
            else if (ch == chAlternativeRating)
            {
                Tournament?.Participants.Sort((p1, p2) => SortAscending ? p2.AlternativeRating.CompareTo(p1.AlternativeRating) : p1.AlternativeRating.CompareTo(p2.AlternativeRating));
            }
            UpdateUI();
        }

        private void AbandonTournamentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Tournament != null && selectedItem != null && selectedItem.Tag != null && selectedItem.Tag is Participant p)
            {
                if (p.Active == null)
                {
                    p.Active = new bool[Tournament.CountRounds];
                    Array.Fill(p.Active, true);
                }
                for (int i = Tournament.Rounds.Count; i < Tournament.CountRounds; ++i) p.Active[i] = false;
                UpdateUI();
            }
        }

        private void PauseNextRoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Tournament != null && selectedItem != null && selectedItem.Tag != null && selectedItem.Tag is Participant p)
            {
                if (p.Active == null)
                {
                    p.Active = new bool[Tournament.CountRounds];
                    Array.Fill(p.Active, true);
                }
                p.Active[Tournament.Rounds.Count] = false;
                UpdateUI();
            }
        }

        private void CmsParticipant_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool abEnabled = Tournament != null && Tournament.Rounds.Count < Tournament.CountRounds;
            if (abEnabled && selectedItem != null && selectedItem.Tag != null && selectedItem.Tag is Participant p)
            {
                if (Tournament != null && p.Active != null && !p.Active[Tournament.Rounds.Count]) abEnabled = false;
            }
            undoAbandonPauseToolStripMenuItem.Enabled = !abEnabled && Tournament != null && Tournament.Rounds.Count < Tournament.CountRounds;
            abandonTournamentToolStripMenuItem.Enabled = abEnabled;
            pauseNextRoundToolStripMenuItem.Enabled = abEnabled;
        }

        private void UndoAbandonPauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Tournament != null && selectedItem != null && selectedItem.Tag != null && selectedItem.Tag is Participant p)
            {
                if (p.Active != null)
                {
                    for (int i = Tournament.Rounds.Count; i < Tournament.CountRounds; ++i) p.Active[i] = true;
                }
                UpdateUI();
            }
        }

        private void EditPlayerToolstripItem(object sender, EventArgs e)
        {
            if (Tournament != null && selectedItem != null && selectedItem.Tag != null && selectedItem.Tag is Participant p)
            {
                Participant cp = new()
                {
                    Name = p.Name,
                    FideId = p.FideId,
                    FideRating = p.FideRating,
                    AlternativeRating = p.AlternativeRating,
                    Active = p.Active,
                    Attributes = new(p.Attributes),
                    Club = p.Club,
                    Federation = p.Federation,
                    ParticipantId = p.ParticipantId,
                    Sex = p.Sex,
                    Title = p.Title,
                    YearOfBirth = p.YearOfBirth
                };
                PlayerDialog pd = new(cp, _tournament);
                if (pd.ShowDialog() == DialogResult.OK)
                {
                    p.Name = cp.Name;
                    p.FideId = cp.FideId;
                    p.FideRating = cp.FideRating;
                    p.AlternativeRating = cp.AlternativeRating;
                    p.Active = cp.Active;
                    p.Attributes = new(cp.Attributes);
                    p.Club = cp.Club;
                    p.Federation = cp.Federation;
                    p.ParticipantId = cp.ParticipantId;
                    p.Sex = cp.Sex;
                    p.Title = cp.Title;
                    p.YearOfBirth = cp.YearOfBirth;
                    UpdateUI();
                }
            }
        }

        private readonly HTMLViewer htmlViewer = new();

        private void ExportParticipantListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender == byStartingRankToolStripMenuItem)
            {
                htmlViewer.Title = Properties.Strings.ParticipantListByRank;
                htmlViewer.Html = Tournament?.ParticipantListHTML("Rating", true);
            }
            else if (sender == byNameToolStripMenuItem)
            {
                htmlViewer.Title = Properties.Strings.ParticipantListByName;
                htmlViewer.Html = Tournament?.ParticipantListHTML("Name", false);
            }
            htmlViewer.ShowDialog();
        }

        private void CrosstableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlViewer.Title = Properties.Strings.Crosstable;
            htmlViewer.Html = Tournament?.CrosstableHTML();
            htmlViewer.ShowDialog();
        }

        private void PairingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlViewer.Title = Properties.Strings.Pairings;
            htmlViewer.Html = Tournament?.RoundHTML();
            htmlViewer.ShowDialog();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}