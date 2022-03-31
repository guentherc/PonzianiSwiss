using PonzianiPlayerBase;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PonzianiSwissGui
{
    public partial class PlayerDialog : Form
    {
        public PlayerDialog(Participant player)
        {
            InitializeComponent();
            Player = player;
            if (!player.Attributes.ContainsKey(Participant.AttributeKey.Sex)) player.Attributes.Add(Participant.AttributeKey.Sex, Sex.Male);
            if (!player.Attributes.ContainsKey(Participant.AttributeKey.Birthyear)) player.Attributes.Add(Participant.AttributeKey.Birthyear, 0);
        }

        private readonly IPlayerBase FideBase = PlayerBaseFactory.Get(PlayerBaseFactory.Base.FIDE);

        public Participant Player { private set; get; }

        private void PlayerDialog_Shown(object sender, EventArgs e)
        {
            Utils.PrepareTitelComboBox(cbTitle);
            Utils.PrepareFederationComboBox(cbFederation);
            UpdateUI();
        }

        private void UpdateUI(object? sender = null)
        {
            tbFideId.Text = Player.FideId.ToString();
            if (sender != tbName)
                tbName.Text = Player.Name;
            cbTitle.SelectedValue = Player.Title;
            if (Player.Federation != null) cbFederation.SelectedValue = Player.Federation;
            nudRating.Value = Player.FideRating;
            nudAltRating.Value = Player.AlternativeRating;
            cbFemale.Checked = (Sex)Player.Attributes[Participant.AttributeKey.Sex] == Sex.Female;
            tbYearOfBirth.Text = Player.Attributes[Participant.AttributeKey.Birthyear].ToString();
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            UpdateFromUI();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void UpdateFromUI()
        {
            Player.FideId = ulong.Parse(tbFideId.Text);
            Player.Name = tbName.Text;
            Player.Title = (FideTitle)cbTitle.SelectedValue;
            Player.Federation = cbFederation.SelectedValue.ToString();
            Player.FideRating = (int)nudRating.Value;
            Player.AlternativeRating = (int)nudAltRating.Value;
            Player.Attributes[Participant.AttributeKey.Sex] = cbFemale.Checked ? Sex.Female : Sex.Male;
            if (Int32.TryParse(tbYearOfBirth.Text, out int yearOfBirth))
                Player.Attributes[Participant.AttributeKey.Birthyear] = yearOfBirth;
            else Player.Attributes[Participant.AttributeKey.Birthyear] = 0;
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
                Player = Player2Participant(player);
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
                        Player = Player2Participant(player[0]);
                        Player.FideRating = Player.AlternativeRating;
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
                    Player = Player2Participant(player);
                    Player.FideRating = Player.AlternativeRating;
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
                    player = FideBase.GetById(player.FideId.ToString());
                    if (player != null)
                        player.Club = dlg.Player.Club;
                    else player = dlg.Player;
                }
                if (player != null) Player = Player2Participant(player);
                UpdateUI();
            }
        }

        private static Participant Player2Participant(Player p)
        {
            Participant participant = new()
            {
                Name = p.Name,
                Title = p.Title,
                Team = p.Club,
                AlternativeRating = p.Rating,
                Federation = p.Federation,
                FideId = p.FideId
            };
            participant.Attributes[Participant.AttributeKey.Sex] = p.Sex;
            participant.Attributes[Participant.AttributeKey.Birthyear] = p.YearOfBirth;
            if (p.Club != null && p.Club.Length > 0)
            {
                if (participant.Attributes.ContainsKey(Participant.AttributeKey.Club))
                    participant.Attributes[Participant.AttributeKey.Club] = p.Club;
                else participant.Attributes.Add(Participant.AttributeKey.Club, p.Club);
            }
            participant.Attributes[Participant.AttributeKey.Birthyear] = p.YearOfBirth;
            return participant;
        }
    }
}
