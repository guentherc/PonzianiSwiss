using FlaUI.Core.AutomationElements;
using FlaUI.UIA2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiPlayerBase;
using System;
using System.Collections.Generic;
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
        const string exe = @"D:\chrgu\OneDrive\Dokumente\Visual Studio 2022\Projekte\PonzianiSwiss\PonzianiSwissGui\bin\Release\net6.0-windows\PonzianiSwissGui.exe";
#else
        const string exe = @"D:\chrgu\OneDrive\Dokumente\Visual Studio 2022\Projekte\PonzianiSwiss\PonzianiSwissGui\bin\Debug\net6.0-windows\PonzianiSwissGui.exe";
#endif

        private FlaUI.Core.Application? app;

        [TestInitialize]
        public void Setup()
        {
            app = FlaUI.Core.Application.Launch(@"D:\chrgu\OneDrive\Dokumente\Visual Studio 2022\Projekte\PonzianiSwiss\PonzianiSwissGui\bin\Release\net6.0-windows\PonzianiSwissGui.exe");
        }

        [TestCleanup]
        public void Teardown()
        {
            app?.Close();
        }

        [TestMethod]
        public void TestCreateTournamentSimple()
        {
            if (app == null) Assert.Fail();
            using (var automation = new UIA2Automation())
            {
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
            }
        }

        private static void AddPlayerFromNationalDatabase(Window window,string name, PlayerBaseFactory.Base playerBase = PlayerBaseFactory.Base.GER)
        {
            AutomationElement playerDialog = OpenPlayerDialog(window);
            var btnSearch = playerDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnSearch").First().AsButton();
            btnSearch.Click();
            var searchDialog = playerDialog.FindFirstByXPath("/Window");
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
            Thread.Sleep(500);
            var playerDialog = window.FindFirstByXPath("/Window");
            Assert.IsNotNull(playerDialog);
            return playerDialog;
        }

        private static void AddPlayerByFideId(Window window, ulong fideid)
        {
            AutomationElement playerDialog = OpenPlayerDialog(window);
            var inpFideId = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbFideId").First().AsTextBox();
            Assert.IsNotNull(inpFideId);
            inpFideId.Enter(fideid.ToString());
            ClosePlayerDialog(playerDialog);
        }

        private static void CreateTournament(Window window, string tournamentTitle, string city, string federation, int roundCount, string chiefArbiter, bool accelerated)
        {
            var btnCreate = window.FindFirstByXPath("/ToolBar/Button[1]");
            Assert.IsNotNull(btnCreate);
            btnCreate.Click();
            Thread.Sleep(500);
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
    }
}
