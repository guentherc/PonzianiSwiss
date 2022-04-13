using PonzianiPlayerBase;
using PonzianiSwissLib;
using System.Data;

namespace PonzianiSwissGui
{
    public partial class PlayerDialog : Form
    {
        public PlayerDialog(Participant player, Tournament? tournament)
        {
            InitializeComponent();
            Player = player;
            Tournament = tournament;
            PlayersAdded = 0;
        }

        private readonly IPlayerBase FideBase = PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE);

        public Participant Player { private set; get; }
        public Tournament? Tournament { private set; get; }

        public int PlayersAdded { private set; get; }

        private void PlayerDialog_Shown(object sender, EventArgs e)
        {
            Utils.PrepareTitelComboBox(cbTitle);
            Utils.PrepareFederationComboBox(cbFederation);
            UpdateUI();
        }

        private void UpdateUI(object? sender = null)
        {
            tbFideId.Text = Player.FideId > 0 ? Player.FideId.ToString() : string.Empty;
            if (sender != tbName)
                tbName.Text = Player.Name;
            cbTitle.SelectedValue = Player.Title;
            if (Player.Federation != null) cbFederation.SelectedValue = Player.Federation;
            nudRating.Value = Player.FideRating;
            nudAltRating.Value = Player.AlternativeRating;
            cbFemale.Checked = Player.Sex == Sex.Female;
            tbYearOfBirth.Text = Player.YearOfBirth > 0 ? Player.YearOfBirth.ToString() : String.Empty;
            tbClub.Text = Player.Club ?? string.Empty;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            UpdateFromUI();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void UpdateFromUI()
        {
            if (ulong.TryParse(tbFideId.Text, out ulong fid))
                Player.FideId = fid;
            else Player.FideId = 0;
            Player.Name = tbName.Text;
            Player.Title = (FideTitle)cbTitle.SelectedValue;
            Player.Federation = cbFederation.SelectedValue.ToString();
            Player.FideRating = (int)nudRating.Value;
            Player.AlternativeRating = (int)nudAltRating.Value;
            Player.Sex = cbFemale.Checked ? Sex.Female : Sex.Male;
            Player.Club = tbClub.Text.Length > 0 ? tbClub.Text : null;
            if (Int32.TryParse(tbYearOfBirth.Text, out int yearOfBirth))
                Player.YearOfBirth = yearOfBirth;
            else Player.YearOfBirth = 0;
        }

        private void TbFideId_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void TbFideId_Leave(object sender, EventArgs e)
        {
            GetPlayerFromFideID(tbFideId.Text);
        }

        private void GetPlayerFromFideID(string fideid)
        {
            var player = FideBase.GetById(fideid);
            if (player != null)
            {
                UpdateFromPlayer(player);
                UpdateUI();
            }
        }

        private void TbName_KeyUp(object sender, KeyEventArgs e)
        {
            if (tbName.Text.Length == 4)
            {
                Invoke((MethodInvoker)(() =>
                {
                    tbName.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    tbName.AutoCompleteCustomSource = new();
                    tbName.AutoCompleteCustomSource.AddRange(FideBase.Find(tbName.Text).Select(p => p.Name).ToArray());
                }));
            }
            if (tbName.AutoCompleteCustomSource.Count > 0)
            {
                Invoke((MethodInvoker)(() =>
                {
                    var player = FideBase.Find(tbName.Text, 1);
                    if (player.Count > 0)
                    {
                        UpdateFromPlayer(player[0]);
                    }
                    UpdateUI(sender);
                }));
            }
        }

        private string? nameOnEnter = null;

        private void TbName_Enter(object sender, EventArgs e)
        {
            nameOnEnter = tbName.Text;
        }

        private void TbName_Leave(object sender, EventArgs e)
        {
            if (nameOnEnter != null && tbName.Text.Length > 4 && tbName.Text != nameOnEnter)
            {
                var player = FideBase.Find(tbName.Text).FirstOrDefault();
                if (player != null)
                {
                    UpdateFromPlayer(player);
                    UpdateUI();
                }
            }
            nameOnEnter = null;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            PlayerSearchDialog dlg = new();
            if (dlg.ShowDialog() == DialogResult.OK && dlg.Player != null)
            {
                Player? player = dlg.Player;
                if (player != null && player.FideId != 0)
                {
                    if (player.Id != player.FideId.ToString()) 
                        Player.AlternativeRating = player.Rating;
                    player = FideBase.GetById(player.FideId.ToString());
                    if (player != null)
                        player.Club = dlg.Player.Club;
                    else player = dlg.Player;
                }
                if (player != null)
                {
                    UpdateFromPlayer(player);
                }
                UpdateUI();
            }
        }

        private void UpdateFromPlayer(Player player)
        {
            int arating = Player.AlternativeRating;
            Player = Player2Participant(player);
            Player.FideRating = Player.AlternativeRating;
            Player.AlternativeRating = arating;
        }

        private static Participant Player2Participant(Player p)
        {
            Participant participant = new()
            {
                Name = p.Name,
                Title = p.Title,
                AlternativeRating = p.Rating,
                Federation = p.Federation,
                FideId = p.FideId,
                YearOfBirth = p.YearOfBirth,
                Club = p.Club,
                Sex = p.Sex
            };
            return participant;
        }

        private void TbName_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = tbName.Text.Trim().Length > 0;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            UpdateFromUI();
            if (Tournament != null && Player.Name != null && Player.Name.Trim().Length > 0)
            {
                Tournament.Participants.RemoveAll(p => p.FideId > 0 && p.FideId == Player.FideId);
                Tournament.Participants.Add(Player);
                ++PlayersAdded;
                Player = new();
                UpdateUI();
            }
        }
    }
}
