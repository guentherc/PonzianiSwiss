namespace PonzianiSwissGui
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tournamentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editHeaderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.participantsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playerBaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PlayerBaseUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateFideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createTestTournamentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.mainStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpParticipants = new System.Windows.Forms.TabPage();
            this.lvParticipants = new System.Windows.Forms.ListView();
            this.chName = new System.Windows.Forms.ColumnHeader();
            this.chFideId = new System.Windows.Forms.ColumnHeader();
            this.chRating = new System.Windows.Forms.ColumnHeader();
            this.chTournamentId = new System.Windows.Forms.ColumnHeader();
            this.roundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteLastRoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.mainStatusStrip.SuspendLayout();
            this.tcMain.SuspendLayout();
            this.tpParticipants.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tournamentToolStripMenuItem,
            this.participantsToolStripMenuItem,
            this.roundToolStripMenuItem,
            this.playerBaseToolStripMenuItem,
            this.toolsToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // tournamentToolStripMenuItem
            // 
            this.tournamentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.editHeaderToolStripMenuItem});
            this.tournamentToolStripMenuItem.Name = "tournamentToolStripMenuItem";
            resources.ApplyResources(this.tournamentToolStripMenuItem, "tournamentToolStripMenuItem");
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            resources.ApplyResources(this.newToolStripMenuItem, "newToolStripMenuItem");
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            resources.ApplyResources(this.saveToolStripMenuItem, "saveToolStripMenuItem");
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            resources.ApplyResources(this.saveAsToolStripMenuItem, "saveAsToolStripMenuItem");
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.SaveAsToolStripMenuItem_Click);
            // 
            // editHeaderToolStripMenuItem
            // 
            resources.ApplyResources(this.editHeaderToolStripMenuItem, "editHeaderToolStripMenuItem");
            this.editHeaderToolStripMenuItem.Name = "editHeaderToolStripMenuItem";
            this.editHeaderToolStripMenuItem.Click += new System.EventHandler(this.EditHeaderToolStripMenuItem_Click);
            // 
            // participantsToolStripMenuItem
            // 
            this.participantsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem});
            resources.ApplyResources(this.participantsToolStripMenuItem, "participantsToolStripMenuItem");
            this.participantsToolStripMenuItem.Name = "participantsToolStripMenuItem";
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            resources.ApplyResources(this.addToolStripMenuItem, "addToolStripMenuItem");
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddToolStripMenuItem_Click);
            // 
            // playerBaseToolStripMenuItem
            // 
            this.playerBaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PlayerBaseUpdateToolStripMenuItem});
            this.playerBaseToolStripMenuItem.Name = "playerBaseToolStripMenuItem";
            resources.ApplyResources(this.playerBaseToolStripMenuItem, "playerBaseToolStripMenuItem");
            // 
            // PlayerBaseUpdateToolStripMenuItem
            // 
            this.PlayerBaseUpdateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateFideToolStripMenuItem});
            this.PlayerBaseUpdateToolStripMenuItem.Name = "PlayerBaseUpdateToolStripMenuItem";
            resources.ApplyResources(this.PlayerBaseUpdateToolStripMenuItem, "PlayerBaseUpdateToolStripMenuItem");
            // 
            // updateFideToolStripMenuItem
            // 
            this.updateFideToolStripMenuItem.Name = "updateFideToolStripMenuItem";
            resources.ApplyResources(this.updateFideToolStripMenuItem, "updateFideToolStripMenuItem");
            this.updateFideToolStripMenuItem.Click += new System.EventHandler(this.UpdateFIDEToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createTestTournamentToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            resources.ApplyResources(this.toolsToolStripMenuItem, "toolsToolStripMenuItem");
            // 
            // createTestTournamentToolStripMenuItem
            // 
            this.createTestTournamentToolStripMenuItem.Name = "createTestTournamentToolStripMenuItem";
            resources.ApplyResources(this.createTestTournamentToolStripMenuItem, "createTestTournamentToolStripMenuItem");
            this.createTestTournamentToolStripMenuItem.Click += new System.EventHandler(this.CreateTestTournamentToolStripMenuItem_Click);
            // 
            // mainStatusStrip
            // 
            this.mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mainStatusLabel});
            resources.ApplyResources(this.mainStatusStrip, "mainStatusStrip");
            this.mainStatusStrip.Name = "mainStatusStrip";
            // 
            // mainStatusLabel
            // 
            this.mainStatusLabel.Name = "mainStatusLabel";
            resources.ApplyResources(this.mainStatusLabel, "mainStatusLabel");
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpParticipants);
            resources.ApplyResources(this.tcMain, "tcMain");
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            // 
            // tpParticipants
            // 
            this.tpParticipants.Controls.Add(this.lvParticipants);
            resources.ApplyResources(this.tpParticipants, "tpParticipants");
            this.tpParticipants.Name = "tpParticipants";
            this.tpParticipants.UseVisualStyleBackColor = true;
            // 
            // lvParticipants
            // 
            this.lvParticipants.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName,
            this.chFideId,
            this.chRating,
            this.chTournamentId});
            resources.ApplyResources(this.lvParticipants, "lvParticipants");
            this.lvParticipants.Name = "lvParticipants";
            this.lvParticipants.UseCompatibleStateImageBehavior = false;
            this.lvParticipants.View = System.Windows.Forms.View.Details;
            // 
            // chName
            // 
            resources.ApplyResources(this.chName, "chName");
            // 
            // chFideId
            // 
            resources.ApplyResources(this.chFideId, "chFideId");
            // 
            // chRating
            // 
            resources.ApplyResources(this.chRating, "chRating");
            // 
            // chTournamentId
            // 
            resources.ApplyResources(this.chTournamentId, "chTournamentId");
            // 
            // roundToolStripMenuItem
            // 
            this.roundToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.drawToolStripMenuItem,
            this.deleteLastRoundToolStripMenuItem});
            resources.ApplyResources(this.roundToolStripMenuItem, "roundToolStripMenuItem");
            this.roundToolStripMenuItem.Name = "roundToolStripMenuItem";
            // 
            // drawToolStripMenuItem
            // 
            this.drawToolStripMenuItem.Name = "drawToolStripMenuItem";
            resources.ApplyResources(this.drawToolStripMenuItem, "drawToolStripMenuItem");
            // 
            // deleteLastRoundToolStripMenuItem
            // 
            this.deleteLastRoundToolStripMenuItem.Name = "deleteLastRoundToolStripMenuItem";
            resources.ApplyResources(this.deleteLastRoundToolStripMenuItem, "deleteLastRoundToolStripMenuItem");
            this.deleteLastRoundToolStripMenuItem.Click += new System.EventHandler(this.deleteLastRoundToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tcMain);
            this.Controls.Add(this.mainStatusStrip);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.mainStatusStrip.ResumeLayout(false);
            this.mainStatusStrip.PerformLayout();
            this.tcMain.ResumeLayout(false);
            this.tpParticipants.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem tournamentToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripMenuItem editHeaderToolStripMenuItem;
        private ToolStripMenuItem playerBaseToolStripMenuItem;
        private ToolStripMenuItem PlayerBaseUpdateToolStripMenuItem;
        private ToolStripMenuItem updateFideToolStripMenuItem;
        private StatusStrip mainStatusStrip;
        private ToolStripStatusLabel mainStatusLabel;
        private ToolStripMenuItem participantsToolStripMenuItem;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem createTestTournamentToolStripMenuItem;
        private TabControl tcMain;
        private TabPage tpParticipants;
        private ListView lvParticipants;
        private ColumnHeader chName;
        private ColumnHeader chFideId;
        private ColumnHeader chRating;
        private ColumnHeader chTournamentId;
        private ToolStripMenuItem roundToolStripMenuItem;
        private ToolStripMenuItem drawToolStripMenuItem;
        private ToolStripMenuItem deleteLastRoundToolStripMenuItem;
    }
}