using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using FlaUI.UIA3.Patterns;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiPlayerBase;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PonzianiSwissTest
{
    [TestClass]
    public class TestUI
    {
#if RELEASE
        const string exe = @"D:\chrgu\OneDrive\Dokumente\Visual Studio 2022\Projekte\PonzianiSwiss\PonzianiSwiss\bin\Release\net6.0-windows\PonzianiSwiss.exe";
#else
        const string exe = @"D:\chrgu\OneDrive\Dokumente\Visual Studio 2022\Projekte\PonzianiSwiss\PonzianiSwiss\bin\Debug\net6.0-windows\PonzianiSwiss.exe";
#endif

        private FlaUI.Core.Application? app;
        private ConditionFactory cf = new ConditionFactory(new UIA3PropertyLibrary());

        internal enum MenuItemKey
        {
            Tournament_New, Tournament_Open, Tournament_Save, Tournament_Save_As, Tournament_Edit, Tournament_Edit_Forbidden, Tournament_Export_Participant_Rank,
            Tournament_Export_Participant_Name, Tournament_Export_Crosstable, Tournament_Export_Pairings, Tournament_MRU1, Tournament_MRU2, Tournament_MRU3, Tournament_MRU4, Tournament_Exit,
            Participants_Add,
            Round_Draw, Round_Delete,
            Playerbase_Update,
            Settings_Basetheme, Settings_Themecolor, Settings_About
        }

        internal enum ToolbarButtonKey
        {
            Tournament_New, Tournament_Open, Tournament_Save, Tournament_Edit, Participants_Add, Round_Draw
        }

        internal static Dictionary<MenuItemKey, List<string>> MenuPaths = new()
        {
            { MenuItemKey.Tournament_New, new() {"MenuItem_Tournament", "MenuItem_Tournament_New" } },
            { MenuItemKey.Tournament_Open, new() {"MenuItem_Tournament", "MenuItem_Tournament_Open" } },
            { MenuItemKey.Tournament_Save, new() {"MenuItem_Tournament", "MenuItem_Tournament_Save" } },
            { MenuItemKey.Tournament_Save_As, new() { "MenuItem_Tournament", "MenuItem_Tournament_Save_As" } },
            { MenuItemKey.Tournament_Edit, new() {"MenuItem_Tournament", "MenuItem_Tournament_Edit" } },
            { MenuItemKey.Tournament_Edit_Forbidden, new() {"MenuItem_Tournament", "MenuItem_Tournament_Edit_Forbidden" } },
            { MenuItemKey.Tournament_Export_Participant_Rank, new() {"MenuItem_Tournament", "MenuItem_Tournament_Export", "MenuItem_Tournament_Participant_List", "MenuItem_Tournament_Participant_List_Rank" } },
            { MenuItemKey.Tournament_Export_Participant_Name, new() {"MenuItem_Tournament", "MenuItem_Tournament_Export", "MenuItem_Tournament_Participant_List", "MenuItem_Tournament_Participant_List_Name" } },
            { MenuItemKey.Tournament_Export_Crosstable, new() {"MenuItem_Tournament", "MenuItem_Tournament_Export", "MenuItem_Tournament_Crosstable" } },
            { MenuItemKey.Tournament_Export_Pairings, new() {"MenuItem_Tournament", "MenuItem_Tournament_Export", "MenuItem_Tournament_Pairings" } },
            { MenuItemKey.Tournament_MRU1, new() {"MenuItem_Tournament", "MenuItem_Tournament_MRU1" } },
            { MenuItemKey.Tournament_MRU2, new() {"MenuItem_Tournament", "MenuItem_Tournament_MRU2" } },
            { MenuItemKey.Tournament_MRU3, new() {"MenuItem_Tournament", "MenuItem_Tournament_MRU3" } },
            { MenuItemKey.Tournament_MRU4, new() {"MenuItem_Tournament", "MenuItem_Tournament_MRU4" } },
            { MenuItemKey.Tournament_Exit, new() {"MenuItem_Tournament", "MenuItem_Tournament_Exit" } },
            { MenuItemKey.Participants_Add, new() { "MenuItem_Participants", "MenuItem_Participants_Add" } },
            { MenuItemKey.Round_Draw, new() { "MenuItem_Round", "MenuItem_Round_Draw" } },
            { MenuItemKey.Round_Delete, new() { "MenuItem_Round", "MenuItem_Round_Delete" } },
            { MenuItemKey.Playerbase_Update, new() { "MenuItem_PlayerBase" } },
            { MenuItemKey.Settings_Basetheme, new() { "MenuItem_Settings", "MenuItem_Settings_Basetheme" } },
            { MenuItemKey. Settings_Themecolor, new() { "MenuItem_Settings", "MenuItem_Settings_Themecolor" } },
            { MenuItemKey.Settings_About, new() { "MenuItem_Settings", "MenuItem_Settings_About" } }
        };

        internal static Dictionary<ToolbarButtonKey, string> ToolbarButtonIds = new()
        {
            { ToolbarButtonKey.Tournament_New, "Toolbar_Tournament_New" },
            { ToolbarButtonKey.Tournament_Open,"Toolbar_Tournament_Open" },
            { ToolbarButtonKey.Tournament_Save,"Toolbar_Tournament_Save" },
            { ToolbarButtonKey.Tournament_Edit, "Toolbar_Tournament_Edit" },
            { ToolbarButtonKey.Participants_Add, "Toolbar_Participant_Add" },
            { ToolbarButtonKey.Round_Draw, "Toolbar_Round_Draw" }
        };

        [TestInitialize]
        public void Setup()
        {
            Utils.Seed(20);
            app = FlaUI.Core.Application.Launch(exe);
            app.WaitWhileBusy();
        }

        [TestCleanup]
        public void Teardown()
        {
            app?.Close();
        }

        [TestMethod]
        public void DrawInitialRound()
        {
            var automation = new UIA3Automation();
            var window = app?.GetMainWindow(automation);
            if (window == null)
            {
                Assert.Fail();
                return;
            }
            string filename = LoadTournament(window, Properties.Resources.Tournament_100_Participants_0_Rounds_tjson);
            //Retry.WhileFalse(() => ofd.IsOffscreen, TimeSpan.FromSeconds(5));
            var participants = GetParticipantsFromListView(window);
            Assert.AreEqual(100, participants.Count);
            ClickMenuEntry(window, MenuItemKey.Round_Draw);
            Retry.WhileNull(() => window.FindFirstByXPath("/Tab/TabItem[2]"), TimeSpan.FromSeconds(10));
            var tabitem = window.FindFirstByXPath("/Tab/TabItem[2]").AsTabItem();
            Assert.IsNotNull(tabitem);
            Thread.Sleep(1000);
            var pairings = GetPairingsFromListView(window, 0);
            Assert.AreEqual(50, pairings.Count);
            foreach (var pairing in pairings)
            {
                int wid = int.Parse(pairing["IDWhite"]);
                int bid = int.Parse(pairing["IDBlack"]);
                Assert.AreEqual(50, Math.Abs(wid - bid));
            }
            CheckAfterDraw(window);
            //Reread participants (Id should now be available)
            participants = GetParticipantsFromListView(window);
            foreach (var participant in participants)
                Assert.IsTrue(participant != null && participant.ContainsKey("ParticipantId") && int.Parse(participant["ParticipantId"]) > 0);
            //Switch back to Round Tab
            var tabview = window.FindFirstDescendant(cf.ByAutomationId("MainTabControl")).AsTab();
            tabview.SelectTabItem(1);
            app?.WaitWhileBusy();
            Retry.WhileFalse(() => tabview.SelectedTabItemIndex == 1, TimeSpan.FromSeconds(5));
            Retry.WhileNull(() => window.FindFirstByXPath($"/Tab/TabItem[2]/Custom/DataGrid"));
            var listview_round = window.FindFirstByXPath($"/Tab/TabItem[2]/Custom/DataGrid").AsDataGridView();
            Assert.IsNotNull(listview_round);
            Wait.UntilResponsive(listview_round);
            Assert.IsTrue(tabview.SelectedTabItemIndex == 1);

            for (int rowIndex = 0; rowIndex < pairings.Count; ++rowIndex)
            {
                listview_round.Rows[rowIndex].AsGridRow().ScrollIntoView();
                var white = participants.Where(p => p["ParticipantId"] == pairings[rowIndex]["IDWhite"]).First();
                var black = participants.Where(p => p["ParticipantId"] == pairings[rowIndex]["IDBlack"]).First();
                var result = Utils.Simulate(int.Parse(white["Rating"]), int.Parse(black["Rating"]));
                Mouse.Click(listview_round.Rows[rowIndex].Cells[0].GetClickablePoint(), MouseButton.Right);
                Wait.UntilInputIsProcessed();
                var ctxMenu = window.ContextMenu;
                Assert.IsNotNull(ctxMenu);
                string automationId = $"MenuItem_Set_Result_{(int)result}";
                var menuItem = ctxMenu.FindFirstDescendant(cf.ByAutomationId(automationId)).AsMenuItem();
                menuItem.Invoke();
                app?.WaitWhileBusy();
            }
            CheckAfterRoundCompleted(window);
            ClickMenuEntry(window, MenuItemKey.Tournament_Exit);
            File.Delete(filename);
        }

        private string LoadTournament(Window window, string json)
        {
            string filename = Path.GetTempFileName();
            Console.WriteLine(filename);
            File.WriteAllText(filename, json);
            LoadFromFile(window, filename);
            app?.WaitWhileBusy();
            return filename;
        }

        private void LoadFromFile(Window window, string filename)
        {
            ClickMenuEntry(window, MenuItemKey.Tournament_Open);
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            var ofd = window.FindFirstByXPath("/Window").AsWindow();
            var finput = ofd.FindFirstByXPath("/ComboBox[1]").AsComboBox();
            Assert.IsNotNull(finput);
            finput.Focus();
            finput.EditableText = filename;
            Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
        }

        [TestMethod]
        public void CreateTournament()
        {
            var automation = new UIA3Automation();
            var window = app?.GetMainWindow(automation);
            if (window == null)
            {
                Assert.Fail();
                return;
            }
            //Check active state of menu items and toolbar buttons
            CheckInitialState(window);

            //Create a new tournament
            ClickMenuEntry(window, MenuItemKey.Tournament_New);
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            var tournamentDialog = window.FindFirstByXPath("/Window").AsWindow();
            Assert.IsNotNull(tournamentDialog);
            FillTournamentDialog(tournamentDialog, new() { { "Name", "Test Tournament" }, { "City", "Chess City" }, { "Federation", "FRA" },
                                                           { "CountRounds", "9" }, { "PairingSystem", PairingSystem.Burstein },
                                                           { "ChiefArbiter", "Pier-Luigi Eschweiler"}, {"DeputyChiefArbiter", "Hans Pfeife, Markus Blind" },
                                                           { "TimeControl", "90/40+30 30+30" }, { "RatingDetermination", TournamentRatingDetermination.Average } });
            app?.WaitWhileBusy();
            Retry.WhileFalse(() => tournamentDialog.IsOffscreen, TimeSpan.FromSeconds(5));

            //Check active state of menu items and toolbar buttons
            CheckAfterTournamentCreation(window);

            //Add a player by it's Fide ID
            ClickToolbarButton(window, ToolbarButtonKey.Participants_Add);
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            var participantDialog = window.FindFirstByXPath("/Window").AsWindow();
            ulong fideid = 1503014;
            AddPlayerByFideID(participantDialog, fideid);
            app?.WaitWhileBusy();
            Retry.WhileFalse(() => participantDialog.IsOffscreen, TimeSpan.FromSeconds(5));
            //Check if player has been added by looking at Participant list
            var participants = GetParticipantsFromListView(window);
            Assert.AreEqual(1, participants.Count);
            Assert.AreEqual(0, double.Parse(participants[0]["Score"]));
            Assert.AreEqual(fideid, ulong.Parse(participants[0]["FideId"]));

            //Add a FIDE Player by name
            ClickToolbarButton(window, ToolbarButtonKey.Participants_Add);
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            participantDialog = window.FindFirstByXPath("/Window").AsWindow();
            string name = "Caruana, Fabiano";
            AddFidePlayerByName(participantDialog, name);
            app?.WaitWhileBusy();
            Retry.WhileFalse(() => participantDialog.IsOffscreen, TimeSpan.FromSeconds(5));
            //Check if player has been added by looking at Participant list
            participants = GetParticipantsFromListView(window);
            Assert.AreEqual(2, participants.Count);
            Assert.AreEqual(0, double.Parse(participants[1]["Score"]));
            Assert.AreEqual(2020009ul, ulong.Parse(participants[1]["FideId"]));
            Assert.AreEqual(name, participants[1]["Name"]);

            //Add a player unknown to FIDE Database
            ClickToolbarButton(window, ToolbarButtonKey.Participants_Add);
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            participantDialog = window.FindFirstByXPath("/Window").AsWindow();
            name = "Mustermann, Max";
            string federation = "GER";
            int rating = 1234;
            int year = 1989;
            string club = "Nichtraucher Schachclub Lichterfelde";
            AddPlayerFreeStyle(participantDialog, name, federation, rating, year, club);
            app?.WaitWhileBusy();
            Retry.WhileFalse(() => participantDialog.IsOffscreen, TimeSpan.FromSeconds(5));
            //Check if player has been added by looking at Participant list
            participants = GetParticipantsFromListView(window);
            Assert.AreEqual(3, participants.Count);
            Assert.AreEqual(0, double.Parse(participants[2]["Score"]));
            Assert.AreEqual(0ul, ulong.Parse(participants[2]["FideId"]));
            Assert.AreEqual(name, participants[2]["Name"]);
            Assert.AreEqual(federation, participants[2]["Federation"]);

            //Add a player by name from national database
            ClickToolbarButton(window, ToolbarButtonKey.Participants_Add);
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            participantDialog = window.FindFirstByXPath("/Window").AsWindow();
            name = "Telp,Peter";
            AddPlayerFromNationalDatabase(participantDialog, PlayerBaseFactory.Base.GER, name);

            participants = GetParticipantsFromListView(window);
            Assert.AreEqual(4, participants.Count);
            Assert.AreEqual(0, double.Parse(participants[3]["Score"]));
            Assert.AreEqual(name, participants[3]["Name"]);
            Assert.AreEqual("GER", participants[3]["Federation"]);

            //Check active state of menu items and toolbar buttons
            CheckAfterParticipantsAdded(window);

            ClickMenuEntry(window, MenuItemKey.Tournament_Exit);
        }

        private void AddPlayerFromNationalDatabase(Window window, PlayerBaseFactory.Base playerbase, string name, bool cancel = false)
        {
            var btnNationalBase = window.FindFirstDescendant(cf.ByAutomationId("Button_NationalBase")).AsButton();
            Assert.IsNotNull(btnNationalBase);
            btnNationalBase.Invoke();
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            var nwindow = window.FindFirstByXPath("/Window").AsWindow();

            var cbBase = nwindow.FindFirstDescendant(cf.ByAutomationId("ComboBox_Base")).AsComboBox();
            Assert.IsNotNull(cbBase);
            cbBase.Focus();
            Thread.Sleep(1000);
            Keyboard.Type(playerbase.ToString());
            Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            nwindow.Focus();

            var tbName = nwindow.FindFirstByXPath("/Edit").AsTextBox();
            Assert.IsNotNull(tbName);
            tbName.Focus();
            tbName.Enter(name);
            Wait.UntilInputIsProcessed(TimeSpan.FromSeconds(1));
            Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.DOWN);
            Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            Wait.UntilInputIsProcessed(TimeSpan.FromSeconds(1));

            var nbtn = cancel ? nwindow.FindFirstDescendant(cf.ByAutomationId("PlayerSearchDialogCancelButton")).AsButton() : nwindow.FindFirstDescendant(cf.ByAutomationId("PlayerSearchDialogOkButton")).AsButton();
            Assert.IsNotNull(nbtn);
            nbtn.Invoke();

            app?.WaitWhileBusy();
            Retry.WhileFalse(() => nwindow.IsOffscreen, TimeSpan.FromSeconds(5));

            var btn = cancel ? window.FindFirstDescendant(cf.ByAutomationId("ParticipantDialogCancelButton")).AsButton() : window.FindFirstDescendant(cf.ByAutomationId("ParticipantDialogOkButton")).AsButton();
            Assert.IsNotNull(btn);
            btn.Invoke();
        }

        private void AddPlayerFreeStyle(Window window, string name, string federation = "Fide", int rating = 0, int year = 0, string? club = null, bool cancel = false)
        {
            var tbName = window.FindFirstByXPath("/Edit[2]").AsTextBox();
            Assert.IsNotNull(tbName);
            tbName.Enter(name);
            var cbFederation = window.FindFirstDescendant(cf.ByAutomationId("ComboBox_Federation")).AsComboBox();
            SetFederationComboBox(window, federation, cbFederation);
            if (rating > 0)
            {
                var tbRating = window.FindFirstDescendant(cf.ByAutomationId("TextBox_AlternativeRating")).AsTextBox();
                Assert.IsNotNull(tbRating);
                tbRating.Enter(rating.ToString());
            }
            if (year > 0)
            {
                var tbYear = window.FindFirstDescendant(cf.ByAutomationId("TextBox_YearOfBirth")).AsTextBox();
                Assert.IsNotNull(tbYear);
                tbYear.Enter(year.ToString());
            }
            if (club != null)
            {
                var tbClub = window.FindFirstDescendant(cf.ByAutomationId("TextBox_Club")).AsTextBox();
                Assert.IsNotNull(tbClub);
                tbClub.Enter(club);
            }
            var btn = cancel ? window.FindFirstDescendant(cf.ByAutomationId("ParticipantDialogCancelButton")).AsButton() : window.FindFirstDescendant(cf.ByAutomationId("ParticipantDialogOkButton")).AsButton();
            Assert.IsNotNull(btn);
            btn.Invoke();
        }

        private void AddPlayerByFideID(Window window, ulong fideid, bool cancel = false)
        {
            var tbFideId = window.FindFirstDescendant(cf.ByAutomationId("TextBox_FideID")).AsTextBox();
            Assert.IsNotNull(tbFideId);
            tbFideId.Enter(fideid.ToString());
            Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.TAB);
            var btn = cancel ? window.FindFirstDescendant(cf.ByAutomationId("ParticipantDialogCancelButton")).AsButton() : window.FindFirstDescendant(cf.ByAutomationId("ParticipantDialogOkButton")).AsButton();
            Assert.IsNotNull(btn);
            btn.Invoke();
        }

        private void AddFidePlayerByName(Window window, string name, bool cancel = false)
        {
            var tbName = window.FindFirstByXPath("/Edit[2]").AsTextBox();
            Assert.IsNotNull(tbName);
            tbName.Focus();
            Keyboard.Type(name);
            Wait.UntilInputIsProcessed(TimeSpan.FromSeconds(1));
            Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.DOWN);
            Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            Wait.UntilInputIsProcessed(TimeSpan.FromSeconds(1));
            var btn = cancel ? window.FindFirstDescendant(cf.ByAutomationId("ParticipantDialogCancelButton")).AsButton() : window.FindFirstDescendant(cf.ByAutomationId("ParticipantDialogOkButton")).AsButton();
            Assert.IsNotNull(btn);
            btn.Invoke();
        }

        private List<Dictionary<string, string>> GetPairingsFromListView(Window window, int round_index)
        {
            var tabview = window.FindFirstDescendant(cf.ByAutomationId("MainTabControl")).AsTab();
            tabview.SelectTabItem(round_index + 1);
            
            app?.WaitWhileBusy();
            Retry.WhileFalse(() => tabview.SelectedTabItemIndex == round_index + 1, TimeSpan.FromSeconds(5));
            Retry.WhileNull(() => window.FindFirstByXPath($"/Tab/TabItem[{round_index + 2}]/Custom/DataGrid"));
            var listview_round = window.FindFirstByXPath($"/Tab/TabItem[{round_index + 2}]/Custom/DataGrid").AsDataGridView();
            Assert.IsNotNull(listview_round);
            Wait.UntilResponsive(listview_round);
            Assert.IsTrue(tabview.SelectedTabItemIndex == round_index + 1);
            List<Dictionary<string, string>> pairings = new();
            Thread.Sleep(3000);
            var rows = listview_round.Rows;
            foreach (var row in rows)
            {
                pairings.Add(new());
                pairings.Last().Add("IDWhite", row.Cells[0].FindFirstChild().Name);
                pairings.Last().Add("White", row.Cells[1].FindFirstChild().Name);
                pairings.Last().Add("IDBlack", row.Cells[2].FindFirstChild().Name);
                pairings.Last().Add("Black", row.Cells[3].FindFirstChild().Name);
                pairings.Last().Add("Result", row.Cells[4].FindFirstChild().Name);
            }
            return pairings;
        }

        private List<Dictionary<string, string>> GetParticipantsFromListView(Window window)
        {
            var tabview = window.FindFirstDescendant(cf.ByAutomationId("MainTabControl")).AsTab();
            tabview.SelectTabItem(0);
            app?.WaitWhileBusy();
            Retry.WhileFalse(() => tabview.SelectedTabItemIndex == 0, TimeSpan.FromSeconds(5));
            List<Dictionary<string, string>> participants = new();
            var listView = window.FindFirstDescendant(cf.ByAutomationId("lvParticipants")).AsListBox();
            if (listView == null)
                listView = window.FindFirstByXPath("/Tab/TabItem/DataGrid").AsListBox();
            Assert.IsNotNull(listView);
            foreach (var item in listView.Items)
            {
                participants.Add(new());
                var subItems = item.FindAllChildren();
                participants.Last().Add("Name", subItems[0].AsListBoxItem().Text);
                participants.Last().Add("Federation", subItems[1].AsListBoxItem().Text);
                participants.Last().Add("FideId", subItems[2].AsListBoxItem().Text);
                participants.Last().Add("Score", subItems[3].AsListBoxItem().Text);
                participants.Last().Add("Rating", subItems[4].AsListBoxItem().Text);
                participants.Last().Add("ParticipantId", subItems[5].AsListBoxItem().Text);
            }
            return participants;
        }

        private void FillTournamentDialog(Window window, Dictionary<string, object> values, bool cancel = false)
        {
            if (values.ContainsKey("Name"))
            {
                var tbName = window.FindFirstDescendant(cf.ByAutomationId("TextBox_Tournament_Name")).AsTextBox();
                Assert.IsNotNull(tbName);
                tbName.Enter((string)values["Name"]);
            }
            if (values.ContainsKey("City"))
            {
                var tbCity = window.FindFirstDescendant(cf.ByAutomationId("TextBox_Tournament_City")).AsTextBox();
                Assert.IsNotNull(tbCity);
                tbCity.Enter((string)values["City"]);
            }
            string federation = values.ContainsKey("Federation") ? (string)values["Federation"] : "Fide";
            var cbFederation = window.FindFirstDescendant(cf.ByAutomationId("ComboBox_Federation")).AsComboBox();
            SetFederationComboBox(window, federation, cbFederation);
            if (values.ContainsKey("CountRounds"))
            {
                var nudRounds = window.FindFirstByXPath("/Edit[3]").AsTextBox();
                Assert.IsNotNull(nudRounds);
                nudRounds.Text = values["CountRounds"].ToString();
            }
            var dpFrom = window.FindFirstDescendant(cf.ByAutomationId("DatePicker_From")).AsDateTimePicker();
            if (dpFrom == null)
            {
                dpFrom = window.FindFirstByXPath("/Custom[2]").AsDateTimePicker();
            }
            Assert.IsNotNull(dpFrom);
            DateTime fromDate = values.ContainsKey("StartDate") ? (DateTime)values["StartDate"] : DateTime.Now;
            dpFrom.SelectedDate = fromDate;
            var dpTo = window.FindFirstDescendant(cf.ByAutomationId("DatePicker_To")).AsDateTimePicker();
            if (dpTo == null)
            {
                dpTo = window.FindFirstByXPath("/Custom[3]").AsDateTimePicker();
            }
            Assert.IsNotNull(dpTo);
            DateTime toDate = values.ContainsKey("EndDate") ? (DateTime)values["EndDate"] : DateTime.Now;
            dpTo.SelectedDate = toDate;
            if (values.ContainsKey("ChiefArbiter"))
            {
                var tbCA = window.FindFirstDescendant(cf.ByAutomationId("TextBox_Tournament_ChiefArbiter")).AsTextBox();
                Assert.IsNotNull(tbCA);
                tbCA.Enter((string)values["ChiefArbiter"]);
            }
            if (values.ContainsKey("DeputyChiefArbiter"))
            {
                var tbDA = window.FindFirstDescendant(cf.ByAutomationId("TextBox_Tournament_DeputyArbiter")).AsTextBox();
                Assert.IsNotNull(tbDA);
                tbDA.Enter((string)values["DeputyChiefArbiter"]);
            }
            if (values.ContainsKey("PairingSystem"))
            {
                var cbPairingSystem = window.FindFirstDescendant(cf.ByAutomationId("ComboBox_PairingSystem")).AsComboBox();
                Assert.IsNotNull(cbPairingSystem);
                cbPairingSystem.Select(values["PairingSystem"].ToString());
            }
            if (values.ContainsKey("TimeControl"))
            {
                var tbTimeControl = window.FindFirstDescendant(cf.ByAutomationId("TextBox_Tournament_TimeControl")).AsTextBox();
                Assert.IsNotNull(tbTimeControl);
                tbTimeControl.Enter((string)values["TimeControl"]);
            }
            if (values.ContainsKey("RatingDetermination"))
            {
                var RatingDetermination = window.FindFirstDescendant(cf.ByAutomationId("ComboBox_RatingDetermination")).AsComboBox();
                Assert.IsNotNull(RatingDetermination);
                RatingDetermination.Select(values["RatingDetermination"].ToString());
            }
            var btn = cancel ? window.FindFirstDescendant(cf.ByAutomationId("TournamentDialogCancelButton")).AsButton() : window.FindFirstDescendant(cf.ByAutomationId("TournamentDialogOkButton")).AsButton();
            Assert.IsNotNull(btn);
            btn.Invoke();
        }

        private void SetFederationComboBox(Window window, string federation, ComboBox cbFederation)
        {
            if (federation != "Fide" && FederationUtil.Federations.ContainsKey(federation))
            {
                federation = $"{FederationUtil.Federations[federation]} ({federation})";
            }
            Assert.IsNotNull(cbFederation);
            cbFederation.Select(federation);
            if (cbFederation.SelectedItem == null)
            {
                cbFederation.Focus();
                Keyboard.Type(federation);
                Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            }
            window.Focus();
        }

        private void CheckInitialState(Window window)
        {
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_New));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Open));
            Assert.IsFalse(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Save));
            Assert.IsFalse(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Edit));
            Assert.IsFalse(IsToolbarButtonEnabled(window, ToolbarButtonKey.Participants_Add));
            Assert.IsFalse(IsToolbarButtonEnabled(window, ToolbarButtonKey.Round_Draw));

            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_New));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Open));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Tournament_Save));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Tournament_Save_As));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Tournament_Edit));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Tournament_Edit_Forbidden));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Crosstable));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Pairings));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Participant_Name));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Participant_Rank));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Exit));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Participants_Add));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Round_Delete));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Round_Draw));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_Basetheme));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_Themecolor));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_About));
        }

        private void CheckAfterTournamentCreation(Window window)
        {
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_New));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Open));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Save));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Edit));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Participants_Add));
            Assert.IsFalse(IsToolbarButtonEnabled(window, ToolbarButtonKey.Round_Draw));

            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_New));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Open));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Tournament_Save));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Save_As));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Edit));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Edit_Forbidden));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Crosstable));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Pairings));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Participant_Name));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Participant_Rank));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Exit));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Participants_Add));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Round_Delete));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Round_Draw));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_Basetheme));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_Themecolor));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_About));
        }

        private void CheckAfterParticipantsAdded(Window window)
        {
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_New));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Open));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Save));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Edit));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Participants_Add));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Round_Draw));

            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_New));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Open));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Tournament_Save));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Save_As));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Edit));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Edit_Forbidden));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Crosstable));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Pairings));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Participant_Name));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Participant_Rank));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Exit));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Participants_Add));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Round_Delete));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Round_Draw));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_Basetheme));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_Themecolor));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_About));
        }

        private void CheckAfterDraw(Window window)
        {
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_New));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Open));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Save));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Edit));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Participants_Add));
            Assert.IsFalse(IsToolbarButtonEnabled(window, ToolbarButtonKey.Round_Draw));

            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_New));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Open));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Save));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Save_As));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Edit));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Edit_Forbidden));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Crosstable));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Pairings));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Participant_Name));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Participant_Rank));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Exit));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Participants_Add));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Round_Delete));
            Assert.IsFalse(IsMenuItemEnabled(window, MenuItemKey.Round_Draw));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_Basetheme));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_Themecolor));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_About));
        }

        private void CheckAfterRoundCompleted(Window window)
        {
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_New));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Open));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Save));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Tournament_Edit));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Participants_Add));
            Assert.IsTrue(IsToolbarButtonEnabled(window, ToolbarButtonKey.Round_Draw));

            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_New));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Open));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Save));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Save_As));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Edit));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Edit_Forbidden));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Crosstable));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Pairings));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Participant_Name));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Export_Participant_Rank));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Tournament_Exit));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Participants_Add));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Round_Delete));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Round_Draw));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_Basetheme));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_Themecolor));
            Assert.IsTrue(IsMenuItemEnabled(window, MenuItemKey.Settings_About));
        }

        private void ClickToolbarButton(Window window, ToolbarButtonKey toolbarButtonKey)
        {
            Button button = FindToolbarButton(window, toolbarButtonKey);
            button.Click();
        }

        private Button FindToolbarButton(Window window, ToolbarButtonKey toolbarButtonKey)
        {
            var button = window.FindAllByXPath("/ToolBar/Button").ToList().Find(b => b.AutomationId == ToolbarButtonIds[toolbarButtonKey]).AsButton();
            Assert.IsNotNull(button);
            return button;
        }

        private void ClickMenuEntry(Window window, MenuItemKey menuItemKey)
        {
            MenuItem menuItem = FindMenuItem(window, MenuPaths[menuItemKey], false);
            menuItem.Invoke();
            app?.WaitWhileBusy();
        }

        private MenuItem FindMenuItem(Window window, List<string> path, bool parentIfNotAccessible = true)
        {
            var menu = window.FindFirstDescendant(cf.Menu()).AsMenu();
            AutomationElement? element = menu;
            int level = 0;

            foreach (string automationId in path)
            {
                AutomationElement? child;
                if (level == 0)
                    child = element.FindFirstDescendant(cf.ByAutomationId(automationId));
                else
                    child = element.AsMenuItem().Items.Find(mi => mi.AsMenuItem().AutomationId == automationId);
                if (child == null) break;
                element = child;
                if (!element.AsMenuItem().IsEnabled) break;
                ++level;
            }
            var menuItem = element.AsMenuItem();
            return menuItem;
        }

        private bool IsMenuItemEnabled(Window window, MenuItemKey menuItemKey)
        {
            MenuItem menuItem = FindMenuItem(window, MenuPaths[menuItemKey]);
            return menuItem.IsEnabled;
        }

        private bool IsToolbarButtonEnabled(Window window, ToolbarButtonKey toolbarButtonKey)
        {
            var button = FindToolbarButton(window, toolbarButtonKey);
            return button.IsEnabled;
        }
    }
}
