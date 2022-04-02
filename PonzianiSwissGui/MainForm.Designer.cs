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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tournamentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.editHeaderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tRFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.participantsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.roundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteLastRoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playerBaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PlayerBaseUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateFideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createTestTournamentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testTRFCreationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.mainStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpParticipants = new System.Windows.Forms.TabPage();
            this.cmsParticipant = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.abandonTournamentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseNextRoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lvParticipants = new System.Windows.Forms.ListView();
            this.chName = new System.Windows.Forms.ColumnHeader();
            this.chFideId = new System.Windows.Forms.ColumnHeader();
            this.chRating = new System.Windows.Forms.ColumnHeader();
            this.chTournamentId = new System.Windows.Forms.ColumnHeader();
            this.chFideRating = new System.Windows.Forms.ColumnHeader();
            this.chAlternativeRating = new System.Windows.Forms.ColumnHeader();
            this.chClub = new System.Windows.Forms.ColumnHeader();
            this.cmsSetResult = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.unratedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.forfeitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripMenuItem();
            this.byeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbNew = new System.Windows.Forms.ToolStripButton();
            this.tsbLoad = new System.Windows.Forms.ToolStripButton();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.tsbEdit = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbAdd = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbDraw = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            this.mainStatusStrip.SuspendLayout();
            this.tcMain.SuspendLayout();
            this.tpParticipants.SuspendLayout();
            this.cmsParticipant.SuspendLayout();
            this.cmsSetResult.SuspendLayout();
            this.toolStrip1.SuspendLayout();
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
            this.toolStripSeparator1,
            this.editHeaderToolStripMenuItem,
            this.toolStripSeparator2,
            this.exportToolStripMenuItem});
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
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // editHeaderToolStripMenuItem
            // 
            resources.ApplyResources(this.editHeaderToolStripMenuItem, "editHeaderToolStripMenuItem");
            this.editHeaderToolStripMenuItem.Name = "editHeaderToolStripMenuItem";
            this.editHeaderToolStripMenuItem.Click += new System.EventHandler(this.EditHeaderToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tRFToolStripMenuItem});
            resources.ApplyResources(this.exportToolStripMenuItem, "exportToolStripMenuItem");
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            // 
            // tRFToolStripMenuItem
            // 
            resources.ApplyResources(this.tRFToolStripMenuItem, "tRFToolStripMenuItem");
            this.tRFToolStripMenuItem.Name = "tRFToolStripMenuItem";
            this.tRFToolStripMenuItem.Click += new System.EventHandler(this.TRFToolStripMenuItem_Click);
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
            this.drawToolStripMenuItem.Click += new System.EventHandler(this.DrawToolStripMenuItem_Click);
            // 
            // deleteLastRoundToolStripMenuItem
            // 
            this.deleteLastRoundToolStripMenuItem.Name = "deleteLastRoundToolStripMenuItem";
            resources.ApplyResources(this.deleteLastRoundToolStripMenuItem, "deleteLastRoundToolStripMenuItem");
            this.deleteLastRoundToolStripMenuItem.Click += new System.EventHandler(this.DeleteLastRoundToolStripMenuItem_Click);
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
            this.updateFideToolStripMenuItem,
            this.gERToolStripMenuItem});
            this.PlayerBaseUpdateToolStripMenuItem.Name = "PlayerBaseUpdateToolStripMenuItem";
            resources.ApplyResources(this.PlayerBaseUpdateToolStripMenuItem, "PlayerBaseUpdateToolStripMenuItem");
            // 
            // updateFideToolStripMenuItem
            // 
            this.updateFideToolStripMenuItem.Name = "updateFideToolStripMenuItem";
            resources.ApplyResources(this.updateFideToolStripMenuItem, "updateFideToolStripMenuItem");
            this.updateFideToolStripMenuItem.Click += new System.EventHandler(this.UpdateFIDEToolStripMenuItem_Click);
            // 
            // gERToolStripMenuItem
            // 
            this.gERToolStripMenuItem.Name = "gERToolStripMenuItem";
            resources.ApplyResources(this.gERToolStripMenuItem, "gERToolStripMenuItem");
            this.gERToolStripMenuItem.Click += new System.EventHandler(this.GERToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createTestTournamentToolStripMenuItem,
            this.testTRFCreationToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            resources.ApplyResources(this.toolsToolStripMenuItem, "toolsToolStripMenuItem");
            // 
            // createTestTournamentToolStripMenuItem
            // 
            this.createTestTournamentToolStripMenuItem.Name = "createTestTournamentToolStripMenuItem";
            resources.ApplyResources(this.createTestTournamentToolStripMenuItem, "createTestTournamentToolStripMenuItem");
            this.createTestTournamentToolStripMenuItem.Click += new System.EventHandler(this.CreateTestTournamentToolStripMenuItem_Click);
            // 
            // testTRFCreationToolStripMenuItem
            // 
            this.testTRFCreationToolStripMenuItem.Name = "testTRFCreationToolStripMenuItem";
            resources.ApplyResources(this.testTRFCreationToolStripMenuItem, "testTRFCreationToolStripMenuItem");
            this.testTRFCreationToolStripMenuItem.Click += new System.EventHandler(this.TestTRFCreationToolStripMenuItem_Click);
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
            resources.ApplyResources(this.tcMain, "tcMain");
            this.tcMain.Controls.Add(this.tpParticipants);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            // 
            // tpParticipants
            // 
            this.tpParticipants.ContextMenuStrip = this.cmsParticipant;
            this.tpParticipants.Controls.Add(this.lvParticipants);
            resources.ApplyResources(this.tpParticipants, "tpParticipants");
            this.tpParticipants.Name = "tpParticipants";
            this.tpParticipants.UseVisualStyleBackColor = true;
            // 
            // cmsParticipant
            // 
            this.cmsParticipant.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.abandonTournamentToolStripMenuItem,
            this.pauseNextRoundToolStripMenuItem});
            this.cmsParticipant.Name = "cmsParticipant";
            resources.ApplyResources(this.cmsParticipant, "cmsParticipant");
            this.cmsParticipant.Opening += new System.ComponentModel.CancelEventHandler(this.CmsParticipant_Opening);
            // 
            // abandonTournamentToolStripMenuItem
            // 
            this.abandonTournamentToolStripMenuItem.Name = "abandonTournamentToolStripMenuItem";
            resources.ApplyResources(this.abandonTournamentToolStripMenuItem, "abandonTournamentToolStripMenuItem");
            this.abandonTournamentToolStripMenuItem.Click += new System.EventHandler(this.AbandonTournamentToolStripMenuItem_Click);
            // 
            // pauseNextRoundToolStripMenuItem
            // 
            this.pauseNextRoundToolStripMenuItem.Name = "pauseNextRoundToolStripMenuItem";
            resources.ApplyResources(this.pauseNextRoundToolStripMenuItem, "pauseNextRoundToolStripMenuItem");
            this.pauseNextRoundToolStripMenuItem.Click += new System.EventHandler(this.PauseNextRoundToolStripMenuItem_Click);
            // 
            // lvParticipants
            // 
            this.lvParticipants.AllowColumnReorder = true;
            this.lvParticipants.AutoArrange = false;
            this.lvParticipants.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName,
            this.chFideId,
            this.chRating,
            this.chTournamentId,
            this.chFideRating,
            this.chAlternativeRating,
            this.chClub});
            resources.ApplyResources(this.lvParticipants, "lvParticipants");
            this.lvParticipants.GridLines = true;
            this.lvParticipants.Name = "lvParticipants";
            this.lvParticipants.UseCompatibleStateImageBehavior = false;
            this.lvParticipants.View = System.Windows.Forms.View.Details;
            this.lvParticipants.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.LvParticipants_ColumnClick);
            this.lvParticipants.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Lv_MouseDown);
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
            // chFideRating
            // 
            resources.ApplyResources(this.chFideRating, "chFideRating");
            // 
            // chAlternativeRating
            // 
            resources.ApplyResources(this.chAlternativeRating, "chAlternativeRating");
            // 
            // chClub
            // 
            resources.ApplyResources(this.chClub, "chClub");
            // 
            // cmsSetResult
            // 
            this.cmsSetResult.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.unratedToolStripMenuItem,
            this.forfeitToolStripMenuItem,
            this.byeToolStripMenuItem});
            this.cmsSetResult.Name = "cmsSetResult";
            resources.ApplyResources(this.cmsSetResult, "cmsSetResult");
            this.cmsSetResult.Opening += new System.ComponentModel.CancelEventHandler(this.CmsSetResult_Opening);
            // 
            // openToolStripMenuItem1
            // 
            this.openToolStripMenuItem1.Name = "openToolStripMenuItem1";
            resources.ApplyResources(this.openToolStripMenuItem1, "openToolStripMenuItem1");
            this.openToolStripMenuItem1.Tag = "0";
            this.openToolStripMenuItem1.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.Tag = "9";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            this.toolStripMenuItem3.Tag = "6";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            resources.ApplyResources(this.toolStripMenuItem4, "toolStripMenuItem4");
            this.toolStripMenuItem4.Tag = "2";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // unratedToolStripMenuItem
            // 
            this.unratedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem5,
            this.toolStripMenuItem6,
            this.toolStripMenuItem7});
            this.unratedToolStripMenuItem.Name = "unratedToolStripMenuItem";
            resources.ApplyResources(this.unratedToolStripMenuItem, "unratedToolStripMenuItem");
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            resources.ApplyResources(this.toolStripMenuItem5, "toolStripMenuItem5");
            this.toolStripMenuItem5.Tag = "3";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            resources.ApplyResources(this.toolStripMenuItem6, "toolStripMenuItem6");
            this.toolStripMenuItem6.Tag = "7";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            resources.ApplyResources(this.toolStripMenuItem7, "toolStripMenuItem7");
            this.toolStripMenuItem7.Tag = "10";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // forfeitToolStripMenuItem
            // 
            this.forfeitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem8,
            this.toolStripMenuItem9,
            this.toolStripMenuItem13});
            this.forfeitToolStripMenuItem.Name = "forfeitToolStripMenuItem";
            resources.ApplyResources(this.forfeitToolStripMenuItem, "forfeitToolStripMenuItem");
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            resources.ApplyResources(this.toolStripMenuItem8, "toolStripMenuItem8");
            this.toolStripMenuItem8.Tag = "11";
            this.toolStripMenuItem8.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            resources.ApplyResources(this.toolStripMenuItem9, "toolStripMenuItem9");
            this.toolStripMenuItem9.Tag = "1";
            this.toolStripMenuItem9.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem13
            // 
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            resources.ApplyResources(this.toolStripMenuItem13, "toolStripMenuItem13");
            this.toolStripMenuItem13.Tag = "13";
            this.toolStripMenuItem13.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // byeToolStripMenuItem
            // 
            this.byeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem10,
            this.toolStripMenuItem11,
            this.toolStripMenuItem12});
            this.byeToolStripMenuItem.Name = "byeToolStripMenuItem";
            resources.ApplyResources(this.byeToolStripMenuItem, "byeToolStripMenuItem");
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            resources.ApplyResources(this.toolStripMenuItem10, "toolStripMenuItem10");
            this.toolStripMenuItem10.Tag = "12";
            this.toolStripMenuItem10.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem11
            // 
            this.toolStripMenuItem11.Name = "toolStripMenuItem11";
            resources.ApplyResources(this.toolStripMenuItem11, "toolStripMenuItem11");
            this.toolStripMenuItem11.Tag = "8";
            this.toolStripMenuItem11.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem12
            // 
            this.toolStripMenuItem12.Name = "toolStripMenuItem12";
            resources.ApplyResources(this.toolStripMenuItem12, "toolStripMenuItem12");
            this.toolStripMenuItem12.Tag = "5";
            this.toolStripMenuItem12.Click += new System.EventHandler(this.SetResultToolStripMenuItem1_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNew,
            this.tsbLoad,
            this.tsbSave,
            this.tsbEdit,
            this.toolStripSeparator3,
            this.tsbAdd,
            this.toolStripSeparator4,
            this.tsbDraw});
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Name = "toolStrip1";
            // 
            // tsbNew
            // 
            this.tsbNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbNew.Image = global::PonzianiSwissGui.Properties.Resources._new;
            resources.ApplyResources(this.tsbNew, "tsbNew");
            this.tsbNew.Name = "tsbNew";
            this.tsbNew.Click += new System.EventHandler(this.NewToolStripMenuItem_Click);
            // 
            // tsbLoad
            // 
            this.tsbLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbLoad.Image = global::PonzianiSwissGui.Properties.Resources.open;
            resources.ApplyResources(this.tsbLoad, "tsbLoad");
            this.tsbLoad.Name = "tsbLoad";
            this.tsbLoad.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // tsbSave
            // 
            this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tsbSave, "tsbSave");
            this.tsbSave.Image = global::PonzianiSwissGui.Properties.Resources.save;
            this.tsbSave.Name = "tsbSave";
            this.tsbSave.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // tsbEdit
            // 
            this.tsbEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tsbEdit, "tsbEdit");
            this.tsbEdit.Image = global::PonzianiSwissGui.Properties.Resources.edit;
            this.tsbEdit.Name = "tsbEdit";
            this.tsbEdit.Click += new System.EventHandler(this.EditHeaderToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // tsbAdd
            // 
            this.tsbAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tsbAdd, "tsbAdd");
            this.tsbAdd.Image = global::PonzianiSwissGui.Properties.Resources.add_participant;
            this.tsbAdd.Name = "tsbAdd";
            this.tsbAdd.Click += new System.EventHandler(this.AddToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // tsbDraw
            // 
            this.tsbDraw.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.tsbDraw, "tsbDraw");
            this.tsbDraw.Image = global::PonzianiSwissGui.Properties.Resources.draw;
            this.tsbDraw.Name = "tsbDraw";
            this.tsbDraw.Click += new System.EventHandler(this.DrawToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.tcMain);
            this.Controls.Add(this.mainStatusStrip);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.mainStatusStrip.ResumeLayout(false);
            this.mainStatusStrip.PerformLayout();
            this.tcMain.ResumeLayout(false);
            this.tpParticipants.ResumeLayout(false);
            this.cmsParticipant.ResumeLayout(false);
            this.cmsSetResult.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
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
        private ContextMenuStrip cmsSetResult;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripMenuItem unratedToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem5;
        private ToolStripMenuItem toolStripMenuItem6;
        private ToolStripMenuItem toolStripMenuItem7;
        private ToolStripMenuItem forfeitToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem8;
        private ToolStripMenuItem toolStripMenuItem9;
        private ToolStripMenuItem openToolStripMenuItem1;
        private ToolStripMenuItem byeToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem10;
        private ToolStripMenuItem toolStripMenuItem11;
        private ToolStripMenuItem toolStripMenuItem12;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem tRFToolStripMenuItem;
        private ToolStripMenuItem testTRFCreationToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem13;
        private ToolStripMenuItem gERToolStripMenuItem;
        private ToolStrip toolStrip1;
        private ToolStripButton tsbNew;
        private ToolStripButton tsbLoad;
        private ToolStripButton tsbSave;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton tsbEdit;
        private ToolStripButton tsbAdd;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton tsbDraw;
        private ColumnHeader chFideRating;
        private ColumnHeader chAlternativeRating;
        private ColumnHeader chClub;
        private ContextMenuStrip cmsParticipant;
        private ToolStripMenuItem abandonTournamentToolStripMenuItem;
        private ToolStripMenuItem pauseNextRoundToolStripMenuItem;
    }
}