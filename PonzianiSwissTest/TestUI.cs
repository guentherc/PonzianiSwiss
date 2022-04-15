using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiPlayerBase;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace PonzianiSwissTest
{
    [TestClass]
    public class TestUI
    {
#if RELEASE
        const string exe = @"D:\chrgu\OneDrive\Dokumente\Visual Studio 2022\Projekte\PonzianiSwiss\PonzianiSwissGui\bin\Release\net6.0-windows\PonzianiSwissGui.exe";
#else
        const string exe = @"D:\chrgu\OneDrive\Dokumente\Visual Studio 2022\Projekte\PonzianiSwiss\PonzianiSwissGui\bin\Debug\net6.0-windows\PonzianiSwissGui.exe";
#endif

        private FlaUI.Core.Application? app;

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
        public void TestRunTournament()
        {
            if (app == null) Assert.Fail();
            if (!File.Exists(tournament_file)) TestWithRealData();
            using var automation = new UIA2Automation();
            //Load Tournament from file
            var window = app.GetMainWindow(automation);
            LoadTournament(window, tournament_file);
            for (int i = 0; i<7; ++i)
               DrawAndSimulateRound(window, i+1);
        }

        private Tab DrawAndSimulateRound(Window window, int round)
        {
            Tab tsMain;
            var drawButton = window.FindFirstByXPath("/ToolBar/Button[6]").AsButton();
            drawButton.Click();
            app.WaitWhileBusy();
            Thread.Sleep(1000);
            tsMain = window.FindFirstByXPath("/Tab").AsTab();
            tsMain.FocusNative();
            for (int i = 0; i < round; ++i)
                Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.LEFT);
            Thread.Sleep(1000);
            tsMain = window.FindFirstByXPath("/Tab").AsTab();
            var lvPlayer = tsMain.FindFirstChild().AsListBox();
            Assert.IsTrue(lvPlayer?.Items?.Length > 100);
            Dictionary<ulong, string> participantNames = new();
            Dictionary<ulong, int> ratings = new();
            Dictionary<int, ulong> ids = new();
            foreach (var item in lvPlayer?.Items)
            {
                var subItems = item.FindAllChildren();
                if (subItems[3].AsListBoxItem().Text.Trim().Length == 0) continue;
                int id = int.Parse(subItems[3].AsListBoxItem().Text);
                ulong fideId = ulong.Parse(subItems[1].AsListBoxItem().Text);
                string name = subItems[0].AsListBoxItem().Text;
                int rating = int.Parse(subItems[2].AsListBoxItem().Text);
                participantNames.Add(fideId, name);
                ratings.Add(fideId, rating);
                ids.Add(id, fideId);
            }
            tsMain.FocusNative();
            for (int i = 0; i < round; ++i)
                Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.RIGHT);
            Thread.Sleep(1000);
            SimulateRound(window, ratings, ids);
            return tsMain;
        }

        private static void SimulateRound(Window window, Dictionary<ulong, int> ratings, Dictionary<int, ulong> ids)
        {
            Tab tsMain = window.FindFirstByXPath("/Tab").AsTab();
            var lvRound = tsMain.FindFirstChild().AsListBox();
            List<ulong[]> pairing = new List<ulong[]>();
            int indx = 0;
            lvRound.Select(indx);
            while (indx < lvRound?.Items.Count())
            {
                Wait.UntilInputIsProcessed();
                List<VirtualKeyShort> keys = new();
                Assert.IsFalse(lvRound.IsOffscreen);
                var subItems = lvRound.SelectedItem.FindAllChildren();
                int wid = 0;
                if (subItems[4].AsListBoxItem().Text.Trim() == "*")
                {
                    wid = int.Parse(subItems[0].AsListBoxItem().Text);
                    int bid = int.Parse(subItems[2].AsListBoxItem().Text);
                    Result r = Utils.Simulate(ratings[ids[wid]], ratings[ids[bid]]);
                    switch (r)
                    {
                        case Result.Win:
                            keys.AddRange(new[] { VirtualKeyShort.DOWN, VirtualKeyShort.DOWN });
                            break;
                        case Result.Loss:
                            keys.AddRange(new[] { VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.DOWN });
                            break;
                        case Result.Draw:
                            keys.AddRange(new[] { VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.DOWN });
                            break;
                        case Result.ForfeitWin:
                            keys.AddRange(new[] { VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.RIGHT });
                            break;
                        case Result.Forfeited:
                            keys.AddRange(new[] { VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.DOWN, VirtualKeyShort.RIGHT, VirtualKeyShort.DOWN });
                            break;
                        default:
                            Assert.Fail();
                            break;
                    }

                    Mouse.Position = lvRound.SelectedItem.AsListBoxItem().BoundingRectangle.Center();
                    Mouse.RightClick();
                    Wait.UntilInputIsProcessed();
                    if (keys.Count > 0)
                    {
                        Keyboard.Type(keys.ToArray());
                        Keyboard.Type(VirtualKeyShort.ENTER);
                    }
                }
                Wait.UntilInputIsProcessed();
                Keyboard.Type(VirtualKeyShort.DOWN);
                Assert.IsNotNull(lvRound.SelectedItem);
                subItems = lvRound.SelectedItem.FindAllChildren();
                Assert.AreNotEqual(wid, int.Parse(subItems[0].AsListBoxItem().Text));
                ++indx;
            }
        }

        [TestMethod]

        public void
            TestWithRealData()
        {
            if (app == null) Assert.Fail();
            using var automation = new UIA2Automation();
            var window = app.GetMainWindow(automation);
            Assert.AreEqual("Ponziani Swiss Pairing Program", window.Title);
            string tournamentTitle = "Test Tournament created from chessemy Open data";
            string city = "Reinstorf";
            string federation = "GER";
            int roundCount = 9;
            string chiefArbiter = "Dirk Ruetemann";
            bool accelerated = true;
            CreateTournament(window, tournamentTitle, city, federation, roundCount, chiefArbiter, accelerated);
            Assert.AreEqual(tournamentTitle, window.Title);
            AutomationElement playerDialog = OpenPlayerDialog(window);
            Wait.UntilInputIsProcessed();
            foreach (var entry in participants)
            {
                var inpFideId = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbFideId").First().AsTextBox();
                Assert.IsNotNull(inpFideId);
                inpFideId.Enter(entry.Key.ToString());
                Wait.UntilInputIsProcessed();
                var btnAdd = playerDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnAdd").First().AsButton();
                Assert.IsNotNull(btnAdd);
                btnAdd.Click();
            }
            var btnCancel = playerDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnCancel").First().AsButton();
            Assert.IsNotNull(btnCancel);
            btnCancel.Click();
            if (File.Exists(tournament_file)) File.Delete(tournament_file);
            SaveTournament(window, tournament_file);
            Retry.WhileNull(() => window.FindFirstByXPath("/ToolBar/Button[3]"));
        }

        private static readonly string tournament_file = Path.Combine(Path.GetTempPath(), "test1.tjson");

        [TestMethod]
        public void TestCreateTournamentSimple()
        {
            if (app == null) Assert.Fail();
            using var automation = new UIA2Automation();
            var window = app.GetMainWindow(automation);
            Assert.AreEqual("Ponziani Swiss Pairing Program", window.Title);
            string tournamentTitle = "Test Tournament created by FlaUI Automation Framework";
            string city = "Hockenheim";
            string federation = "GER";
            int roundCount = 7;
            string chiefArbiter = "Cüneyt Çakır";
            bool accelerated = true;
            CreateTournament(window, tournamentTitle, city, federation, roundCount, chiefArbiter, accelerated);
            Assert.AreEqual(tournamentTitle, window.Title);
            AddPlayerByFideId(window, 24651516);
            AddPlayerByName(window, "Carlsen, Magnus");
            AddPlayer(window, "Müller, Lena", 1623, 1985, "FRA", "SV 1930 Hockenheim", true);
            AddPlayerFromNationalDatabase(window, "Henze, Felix", PlayerBaseFactory.Base.GER);
            var tsMain = window.FindFirstByXPath("/Tab").AsTab();
            var lvPlayer = tsMain.FindFirstChild().AsListBox();
            Assert.AreEqual(4, lvPlayer?.Items?.Length);
            //Save tournament
            var tfile = Path.Combine(Path.GetTempPath(), "test.tjson");
            if (File.Exists(tfile)) File.Delete(tfile);
            SaveTournament(window, tfile);
            //Restart application
            window.Close();
            Teardown();
            Setup();
            window = app.GetMainWindow(automation);
            Assert.IsNotNull(window);
            LoadTournament(window, tfile);
            tsMain = window.FindFirstByXPath("/Tab").AsTab();
            lvPlayer = tsMain.FindFirstChild().AsListBox();
            Assert.AreEqual(4, lvPlayer?.Items?.Length);
            window.Close();
        }

        private static void LoadTournament(Window window, string tfile)
        {
            var btnLoad = window.FindFirstByXPath("/ToolBar/Button[2]");
            Assert.IsNotNull(btnLoad);
            btnLoad.Click();
            Wait.UntilInputIsProcessed();
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            var loadDialog = window.FindFirstByXPath("/Window").AsWindow();
            Assert.IsNotNull(loadDialog);
            var cbFilename = loadDialog.FindAllByXPath("//ComboBox").First().AsComboBox();
            cbFilename.Value = tfile;
            var btnOk = window.FindFirstByXPath("/Window/Button[1]");
            btnOk.Click();
        }

        private static void SaveTournament(Window window, string tfile)
        {
            Retry.WhileNull(() => window.FindFirstByXPath("/ToolBar/Button[3]"));
            var btnSave = window.FindFirstByXPath("/ToolBar/Button[3]").AsButton();
            Retry.WhileFalse(() => btnSave.IsEnabled, TimeSpan.FromSeconds(5));
            btnSave.Click();
            Wait.UntilInputIsProcessed();
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            var saveDialog = window.FindFirstByXPath("/Window").AsWindow();
            Assert.IsNotNull(saveDialog);
            var cbFilename = saveDialog.FindAllByXPath("//ComboBox").First().AsComboBox();
            cbFilename.Value = tfile;
            var btnSaveFile = saveDialog.FindFirstByXPath("/Button[1]");
            btnSaveFile.Click();
        }

        private static void AddPlayerFromNationalDatabase(Window window, string name, PlayerBaseFactory.Base playerBase = PlayerBaseFactory.Base.GER)
        {
            AutomationElement playerDialog = OpenPlayerDialog(window);
            var btnSearch = playerDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnSearch").First().AsButton();
            btnSearch.Click();
            Wait.UntilInputIsProcessed();
            Retry.WhileNull(() => playerDialog.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            var searchDialog = playerDialog.FindFirstByXPath("/Window");
            Assert.IsNotNull(searchDialog);
            var cbBase = searchDialog.FindAllByXPath("/ComboBox").Where(e => e.AutomationId == "cbDataSource").First().AsComboBox();
            Assert.IsNotNull(cbBase);
            int indx = PlayerBaseFactory.AvailableBases.FindIndex(0, p => p.Key == playerBase);
            cbBase.Value = PlayerBaseFactory.AvailableBases[indx].Value;
            var tbName = searchDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbName").First().AsTextBox();
            tbName.Enter(name);
            var btnOk = searchDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnOk").First().AsButton();
            Assert.IsNotNull(btnOk);
            btnOk.Click();
            ClosePlayerDialog(playerDialog);
        }

        private static void AddPlayer(Window window, string name, int rating, int? yearOfBirth, string? federation, string? club, bool isFemale = false)
        {
            AutomationElement playerDialog = OpenPlayerDialog(window);
            var inpName = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbName").First().AsTextBox();
            Assert.IsNotNull(inpName);
            inpName.Enter(name);
            var nudRating = playerDialog.FindAllByXPath("/Spinner").Where(e => e.AutomationId == "nudRating").First().AsSpinner();
            Assert.IsNotNull(nudRating);
            nudRating.Value = rating;
            if (yearOfBirth.HasValue && yearOfBirth.Value > 0)
            {
                var inp = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbYearOfBirth").First().AsTextBox();
                Assert.IsNotNull(inp);
                inp.Text = yearOfBirth.Value.ToString();
            }
            if (federation != null)
            {
                var cbFederation = playerDialog.FindAllByXPath("/ComboBox").Where(e => e.AutomationId == "cbFederation").First().AsComboBox();
                Assert.IsNotNull(cbFederation);
                cbFederation.Value = federation;
            }
            if (club != null)
            {
                var inpClub = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbClub").First().AsTextBox();
                Assert.IsNotNull(inpClub);
                inpClub.Text = club;
            }
            var cbFemale = playerDialog.FindAllByXPath("/CheckBox").Where(e => e.AutomationId == "cbFemale").First().AsCheckBox();
            Assert.IsNotNull(cbFemale);
            cbFemale.IsChecked = isFemale;
            ClosePlayerDialog(playerDialog);
        }

        private static void AddPlayerByName(Window window, string name)
        {
            AutomationElement playerDialog = OpenPlayerDialog(window);
            var inpName = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbName").First().AsTextBox();
            Assert.IsNotNull(inpName);
            inpName.Enter(name);
            ClosePlayerDialog(playerDialog);
        }

        private static void ClosePlayerDialog(AutomationElement playerDialog)
        {
            var btnOk = playerDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnOk").First().AsButton();
            Assert.IsNotNull(btnOk);
            btnOk.Click();
        }

        private static AutomationElement OpenPlayerDialog(Window window)
        {
            var btnAdd = window.FindFirstByXPath("/ToolBar/Button[5]");
            Assert.IsNotNull(btnAdd);
            btnAdd.Click();
            Wait.UntilInputIsProcessed();
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            var playerDialog = window.FindFirstByXPath("/Window");
            Assert.IsNotNull(playerDialog);
            return playerDialog;
        }

        private static void AddPlayerByFideId(Window window, ulong fideid, string? club = null)
        {
            AutomationElement playerDialog = OpenPlayerDialog(window);
            var inpFideId = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbFideId").First().AsTextBox();
            Assert.IsNotNull(inpFideId);
            inpFideId.Enter(fideid.ToString());
            Wait.UntilInputIsProcessed();
            var inpName = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbName").First().AsTextBox();
            inpName.Focus();
            Retry.WhileEmpty(() => inpName.Text, TimeSpan.FromSeconds(5));
            if (club != null)
            {
                var inpClub = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbClub").First().AsTextBox();
                Assert.IsNotNull(inpClub);
                inpClub.Text = club;
            }
            ClosePlayerDialog(playerDialog);
        }

        private static void CreateTournament(Window window, string tournamentTitle, string city, string federation, int roundCount, string chiefArbiter, bool accelerated)
        {
            Retry.WhileNull(() => window.FindFirstByXPath("/ToolBar/Button[1]"), TimeSpan.FromSeconds(5));
            var btnCreate = window.FindFirstByXPath("/ToolBar/Button[1]");
            Assert.IsNotNull(btnCreate);
            btnCreate.Click();
            Wait.UntilInputIsProcessed();
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            var tournamentDialog = window.FindFirstByXPath("/Window");
            Assert.IsNotNull(tournamentDialog);
            var inpName = tournamentDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbName").First().AsTextBox();
            Assert.IsNotNull(inpName);

            inpName.Text = tournamentTitle;
            var inpCity = tournamentDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbCity").First().AsTextBox();
            Assert.IsNotNull(inpCity);
            inpCity.Text = city;
            var cbFederation = tournamentDialog.FindAllByXPath("/ComboBox").Where(e => e.AutomationId == "cbFederation").First().AsComboBox();
            Assert.IsNotNull(cbFederation);
            cbFederation.Value = federation;
            var nudRounds = tournamentDialog.FindAllByXPath("/Spinner").Where(e => e.AutomationId == "nudRounds").First().AsSpinner();
            Assert.IsNotNull(nudRounds);
            int rounds = (int)nudRounds.Value;
            while (rounds > roundCount)
            {
                nudRounds.Decrement();
                --rounds;
            }
            while (rounds < roundCount)
            {
                nudRounds.Increment();
                ++rounds;
            }
            Assert.AreEqual(roundCount, (int)nudRounds.Value);
            var tbCa = tournamentDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbChiefArbiter").First().AsTextBox();
            Assert.IsNotNull(tbCa);
            tbCa.Text = chiefArbiter;
            var cbAcc = tournamentDialog.FindAllByXPath("/CheckBox").Where(e => e.AutomationId == "cbAcceleration").First().AsCheckBox();
            Assert.IsNotNull(cbAcc);
            cbAcc.IsChecked = accelerated;
            var btnOk = tournamentDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnOk").First().AsButton();
            Assert.IsNotNull(btnOk);
            btnOk.Click();
        }

        private static readonly Dictionary<ulong, string> participants = new()
        {
            { 1003720, "SC Ötigheim" },
            { 3206882, "" },
            { 753246, "" },
            { 4655192, "SC Viernheim 1934 e.V." },
            { 4694031, "SF Neuberg" },
            { 12993093, "SF Deizisau" },
            { 14112256, "SC Heimbach-Weis/Neuwied e.V." },
            { 1196448, "" },
            { 1270337, "SV 1947 Walldorf" },
            { 1404210, "Preetzer TSV" },
            { 24614343, "Hamburger SK von 1830 eV" },
            { 4606019, "Herforder Schachverein Königss" },
            { 4647823, "SF Schwerin" },
            { 16201132, "SAbt SV Werder Bremen" },
            { 4690745, "SF Neuberg" },
            { 1195360, "Schachfreunde Berlin 1903 e.V." },
            { 12996114, "HSK Lister Turm" },
            { 12956830, "SAbt SV Werder Bremen" },
            { 12961574, "SAbt SV Werder Bremen" },
            { 12990698, "Schachfreunde Lieme e. V." },
            { 12990809, "TSV Schönaich" },
            { 46674969, "HSK Lister Turm" },
            { 4618130, "Schachgemeinschaft Leipzig" },
            { 4633300, "Hamburger SK von 1830 eV" },
            { 12992577, "SC Turm Lüneburg e.V." },
            { 21816107, "" },
            { 12994219, "SV 1920 Hofheim" },
            { 24642860, "Schachfreunde Lieme e. V." },
            { 24611646, "SK Nordhorn-Blanke" },
            { 4631153, "FC ST.Pauli 1910 eV SAbt" },
            { 12993085, "SF Deizisau" },
            { 12912050, "SAbt SV Werder Bremen" },
            { 4678915, "SK Marmstorf GW Harburg" },
            { 24600636, "SK Doppelbauer Kiel von 1910" },
            { 4696255, "Schachverein Wattenscheid 1930" },
            { 16204271, "SV Königsjäger Süd-West e.V." },
            { 12970719, "SV Mattnetz Berlin e.V." },
            { 4693523, "SC Oberwinden 1957 e.V." },
            { 16234464, "SC Borussia Lichtenberg" },
            { 1234765, "" },
            { 1330667, "" },
            { 1069020, "" },
            { 1007033, "" },
            { 4616863, "Schachfreunde Lieme e. V." },
            { 24695084, "" },
            { 4635787, "SF Deizisau" },
            { 4665210, "" },
            { 4659503, "SC Turm Lüneburg e.V." },
            { 24619671, "Schachclub Oranienburg e.V." },
            { 4661699, "" },
            { 4614410, "Schachfreunde Essen-Katernberg" },
            { 16246861, "USV TU Dresden" },
            { 4696506, "SK 1911 Herzogenaurach e.V." },
            { 16232283, "Schachgemeinschaft Porz e. V." },
            { 16220390, "Schachgesellschaft Solingen e." },
            { 4690753, "Schachfreunde Schwedt 2000 e.V" },
            { 1059505, "" },
            { 4639472, "SK Marmstorf GW Harburg" },
            { 16216768, "FC Ergolding 1932 e.V." },
            { 12958611, "Bahn-Schachclub Wuppertal" },
            { 4691369, "Sfr. Erbach" },
            { 6303692, "Schachclub Unterhaching e.V." },
            { 1064860, "Aachener Schachverein 1856 e. " },
            { 4696719, "Schachklub Xanten e. V." },
            { 16204948, "Hamelner SV" },
            { 12972924, "SchVgg Blankenese von 1923 e.V" },
            { 34604804, "Bramfelder SK 1947 e.V." },
            { 4673280, "Hamburger SK von 1830 eV" },
            { 16252101, "Chemie Bitterfeld" },
            { 1707701, "" },
            { 5807204, "" },
            { 16231538, "Post SV Uelzen" },
            { 16214447, "" },
            { 24696277, "SchVgg Blankenese von 1923 e.V" },
            { 16264975, "SchVgg Blankenese von 1923 e.V" },
            { 4625447, "SV Großhansdorf" },
            { 16260368, "SAbt TuS Varrel" },
            { 16299337, "SC Schwarz-Weiß Nürnberg Süd e" },
            { 12977373, "SC Landskrone" },
            { 1192051, "" },
            { 16225864, "Sfr.Heimersheim" },
            { 12930709, "" },
            { 1471759, "" },
            { 4661818, "SG Turm Leipzig" },
            { 16216393, "Schachgesellschaft Bochum 1931" },
            { 4699955, "SF Anderssen Wetzlar" },
            { 4695496, "Post-Telekom SV 1925 Aachen e." },
            { 11623187, "" },
            { 4686586, "SC Bad Nauheim" },
            { 1270582, "Schachgemeinschaft Porz e. V." },
            { 1332350, "Karlsruher SF 1853" },
            { 16238044, "Düsseldorfer Schachverein 1854" },
            { 16201604, "Schachzwerge Magdeburg" },
            { 16245008, "Lübecker SV von 1873" },
            { 16217136, "SC Weisse Dame e.V." },
            { 12994103, "Schachgesellschaft Solingen e." },
            { 4699858, "St. Johannes Spelle" },
            { 16238290, "Schachfreunde Nordost Berlin" },
            { 16218388, "SK Lehrte von 1919 e. V." },
            { 24653470, "SK Gau-Algesheim" },
            { 12933538, "Bremer SG von 1877" },
            { 24633402, "Sfr. Erbach" },
            { 4669991, "Schachverein Turm Drolshagen 0" },
            { 34611525, "Schachverein Neukirchen-Vluyn " },
            { 16290151, "OSG Baden-Baden" },
            { 24624241, "Burger SK Schwarz-Weiß" },
            { 16200284, "TuS Coswig 1920" },
            { 1270441, "Sportvereinigung Sterkrade-Nor" },
            { 24677817, "SC Laufenburg" },
            { 16273680, "SK Nordhorn-Blanke" },
            { 4667751, "SF 59 Kornwestheim" },
            { 16249569, "SK Varel" },
            { 34614613, "SK Gräfelfing" },
            { 12987301, "SC Erlangen 48/88" },
            { 1270896, "SG Osnabrück" },
            { 4607830, "TSV Kücknitz von 1911" },
            { 34601791, "USV Potsdam e.V., Abt. Schach" },
            { 16299230, "" },
            { 16276078, "" },
            { 24513857, "SV 1926 Neu-Isenburg" },
            { 24695467, "Schachgemeinschaft Leipzig" },
            { 1270250, "Schachclub Oranienburg e.V." },
            { 16268784, "SC Turm Lüneburg e.V." },
            { 16202465, "Ilmenauer SV" },
            { 16219490, "SK Freilassing" },
            { 34607560, "SAbt SV Werder Bremen" },
            { 16258177, "SC Turm Lüneburg e.V." },
            { 34603590, "" },
            { 1270897, "SG Osnabrück" },
            { 16230523, "" },
            { 16295005, "SC Schleispringer Kappeln" },
            { 16242335, "SG Weiß-Blau Eilenriede" },
            { 16219600, "SK Freilassing" },
            { 16234820, "SC Turm Lüneburg e.V." },
            { 12958247, "Schach-Drachen Isernhagen" },
            { 16258320, "OSG Baden-Baden" },
            { 34601120, "Schachklub Altona v.1873/Finke" },
            { 16290160, "OSG Baden-Baden" },
            { 16287134, "SF Neuberg" },
            { 16249526, "SC Turm Lüneburg e.V." },
            { 16254716, "SC Turm Lüneburg e.V." },
            { 1270390, "SK Marburg 1931/72" },
            { 16201914, "SV Empor Erfurt" },
            { 16272331, "MSA Zugzwang 82 e.V." },
            { 12923869, "SC Einrich e.V." },
            { 16281390, "SV Oberursel" },
            { 16234065, "Schachfreunde Esch e. V." },
            { 34613005, "ESV Lok Meiningen" },
            { 16292740, "" },
            { 34602186, "TSG Oberschöneweide e.V." },
            { 34610006, "Sportgemeinschaft Suderwich 19" },
            { 16292537, "SV Babelsberg 03 Abt. Schach" },
            { 16258100, "SC Weisse Dame e.V." },
            { 25659260, "HSK Lister Turm" },
            { 16212452, "Schach-Drachen Isernhagen" },
            { 12907430, "SK Marburg 1931/72" },
            { 1471775, "" },
            { 34600256, "TSG Oberschöneweide e.V." },
            { 34605860, "Schachgemeinschaft Porz e. V." },
            { 34614737, "Schachgemeinschaft Porz e. V." },
            { 34611479, "SC Oberwinden 1957 e.V." },
            { 34611223, "" },
            { 34614885, "" },
            { 34605215, "TSG Oberschöneweide e.V." },
            { 12981591, "" },
            { 34604502, "SC Turm Lüneburg e.V." },
            { 34610332, "SV RUGIA Bergen e.V." },
            { 34605088, "" },
            { 34609172, "" }
        };
    }
}
