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
        }

        private readonly IPlayerBase FideBase = PlayerBaseFactory.Get(PlayerBaseFactory.Base.Fide);

        public Participant Player { private set; get; }

        private void PlayerDialog_Shown(object sender, EventArgs e)
        {
            tbFideId.Text = Player.FideId.ToString();
            tbName.Text = Player.Name;
            Utils.PrepareTitelComboBox(cbTitle);
            cbTitle.SelectedValue = Player.Title;
            Utils.PrepareFederationComboBox(cbFederation);
            if (Player.Federation != null) cbFederation.SelectedValue = Player.Federation;
            nudRating.Value = Player.FideRating;
            nudAltRating.Value = Player.AlternativeRating;
            cbFemale.Checked = (Sex)Player.Attributes[Participant.AttributeKey.Sex] == Sex.Female;
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
                tbName.Text = player.Name;
                cbTitle.SelectedValue = player.Title;
                cbFederation.SelectedValue = player.Federation;
                nudRating.Value = player.Rating;
                cbFemale.Checked = player.Sex == Sex.Female;
            }
        }
    }
}
