using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.UIA2;
using PonzianiPlayerBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiSwissGuiUtils
{
    public class Walkthrough
    {
        const string project_root = @"D:\chrgu\OneDrive\Dokumente\Visual Studio 2022\Projekte\PonzianiSwiss\PonzianiSwissGui\";

#if RELEASE
        const string exe = project_root + @"bin\Release\net6.0-windows\PonzianiSwissGui.exe";
#else
        const string exe = project_root + @"bin\Debug\net6.0-windows\PonzianiSwissGui.exe";
#endif

        public string CaptureDirectory { get; set; } = project_root + @"Doc\img";
        public bool UpdatePlayerBase { get; set; } = false;

        private int captureIndex = 0;

        public void Go()
        {
            //Start application
            FlaUI.Core.Application app = FlaUI.Core.Application.Launch(exe);            
            using var automation = new UIA2Automation();
            //Get main window   
            FlaUI.Core.AutomationElements.Window? window = app.GetMainWindow(automation);
            //Capture main window
            window.CaptureToFile(Path.Combine(CaptureDirectory, $"walktrough{++captureIndex}.png"));
            //Get main menu
            var menu = window.FindFirstChild(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuBar)).AsMenu();
            menu.IsWin32Menu = true;
            //Trigger Fide Player Base update
            if (UpdatePlayerBase)
            {
                var miPlayerBase = menu.Items[3];
                var miUpdate = miPlayerBase.Items[0];
                var miFide = miUpdate.Items[0].Invoke();
                var statusStrip = window.FindFirstChild(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.StatusBar));
                var statusLabel = statusStrip.FindFirstChild(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text)).AsLabel();
                Retry.WhileTrue(() => statusLabel.Text.Trim().Length > 0, TimeSpan.FromSeconds(600));
            }
            //Create Tournament
            CreateTournament(window, "Demo Tournament", "Chesstown", "FIDE", 7, "Garry Collina", false);
            //Add player by Fide ID
            AddPlayerByFideId(window, 4100018);
            //Add player by name
            AddPlayerByName(window, "Kramnik, Vladimir");
            //Add player unknown to player databases
            AddPlayer(window, "Magath, Felix", 0, 1953, "GER", "Hertha BSC");
            //Add player from national player base
            AddNationalPlayer(window, PlayerBaseFactory.Base.GER, "Jäger,Mario");
            app.Close();
        }

        private static AutomationElement OpenPlayerDialog(Window window)
        {
            var btnAdd = window.FindFirstByXPath("/ToolBar/Button[5]");
            btnAdd.Click();
            Wait.UntilInputIsProcessed();
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            var playerDialog = window.FindFirstByXPath("/Window");
            return playerDialog;
        }

        private void AddNationalPlayer(Window window, PlayerBaseFactory.Base pbase, string name)
        {
            AutomationElement playerDialog = OpenPlayerDialog(window);
            playerDialog.FindAllByXPath("/Button").Where(btn => btn.AutomationId == "btnSearch").First().Click();
            Retry.WhileNull(() => playerDialog.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            var searchDialog = playerDialog.FindFirstByXPath("/Window");
            Thread.Sleep(1000);
            Retry.WhileNull(() => searchDialog.FindFirstByXPath("/ComboBox"));
            var cbDataSource = searchDialog.FindFirstByXPath("/ComboBox").AsComboBox();
            cbDataSource.Value = PlayerBaseFactory.AvailableBases[(int)pbase].Value;
            Wait.UntilInputIsProcessed();
            var inpName = searchDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbName").First().AsTextBox();
            inpName.Focus();
            foreach (char c in name) Keyboard.Type(c);
            Wait.UntilInputIsProcessed();
            searchDialog.CaptureToFile(Path.Combine(CaptureDirectory, $"walktrough{++captureIndex}.png"));
            searchDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnOk").First().AsButton().Click();
            playerDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnOk").First().AsButton().Click();
        }

        private static void AddPlayer(Window window, string name, int rating, int? yearOfBirth, string? federation, string? club, bool isFemale = false)
        {
            AutomationElement playerDialog = OpenPlayerDialog(window);
            var inpName = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbName").First().AsTextBox();
            inpName.Enter(name);
            var nudRating = playerDialog.FindAllByXPath("/Spinner").Where(e => e.AutomationId == "nudRating").First().AsSpinner();
            nudRating.Value = rating;
            if (yearOfBirth.HasValue && yearOfBirth.Value > 0)
            {
                var inp = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbYearOfBirth").First().AsTextBox();
                inp.Text = yearOfBirth.Value.ToString();
            }
            if (federation != null)
            {
                var cbFederation = playerDialog.FindAllByXPath("/ComboBox").Where(e => e.AutomationId == "cbFederation").First().AsComboBox();
                cbFederation.Value = federation;
            }
            if (club != null)
            {
                var inpClub = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbClub").First().AsTextBox();
                inpClub.Text = club;
            }
            var cbFemale = playerDialog.FindAllByXPath("/CheckBox").Where(e => e.AutomationId == "cbFemale").First().AsCheckBox();
            cbFemale.IsChecked = isFemale;
            playerDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnOk").First().AsButton().Click();
        }

        private void AddPlayerByFideId(Window window, ulong fideid, string? club = null)
        {
            AutomationElement playerDialog = OpenPlayerDialog(window);
            var inpFideId = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbFideId").First().AsTextBox();
            inpFideId.Enter(fideid.ToString());
            Wait.UntilInputIsProcessed();
            var inpName = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbName").First().AsTextBox();
            inpName.Focus();
            Retry.WhileEmpty(() => inpName.Text, TimeSpan.FromSeconds(5));
            if (club != null)
            {
                var inpClub = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbClub").First().AsTextBox();
                inpClub.Text = club;
            }
            playerDialog.CaptureToFile(Path.Combine(CaptureDirectory, $"walktrough{++captureIndex}.png"));
            playerDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnOk").First().AsButton().Click();
        }

        private static void AddPlayerByName(Window window, string name)
        {
            AutomationElement playerDialog = OpenPlayerDialog(window);
            var inpName = playerDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbName").First().AsTextBox();
            inpName.Enter(name);
            playerDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnOk").First().AsButton().Click();
        }

        private void CreateTournament(Window window, string tournamentTitle, string city, string federation, int roundCount, string chiefArbiter, bool accelerated)
        {
            Retry.WhileNull(() => window.FindFirstByXPath("/ToolBar/Button[1]"), TimeSpan.FromSeconds(5));
            var btnCreate = window.FindFirstByXPath("/ToolBar/Button[1]");
            btnCreate.Click();
            Wait.UntilInputIsProcessed();
            Retry.WhileNull(() => window.FindFirstByXPath("/Window"), TimeSpan.FromSeconds(5));
            var tournamentDialog = window.FindFirstByXPath("/Window");
            var inpName = tournamentDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbName").First().AsTextBox();

            inpName.Text = tournamentTitle;
            var inpCity = tournamentDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbCity").First().AsTextBox();
            inpCity.Text = city;
            var cbFederation = tournamentDialog.FindAllByXPath("/ComboBox").Where(e => e.AutomationId == "cbFederation").First().AsComboBox();
            cbFederation.Value = federation;
            var nudRounds = tournamentDialog.FindAllByXPath("/Spinner").Where(e => e.AutomationId == "nudRounds").First().AsSpinner();
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
            var tbCa = tournamentDialog.FindAllByXPath("/Edit").Where(e => e.AutomationId == "tbChiefArbiter").First().AsTextBox();

            tbCa.Text = chiefArbiter;
            var cbAcc = tournamentDialog.FindAllByXPath("/CheckBox").Where(e => e.AutomationId == "cbAcceleration").First().AsCheckBox();
            cbAcc.IsChecked = accelerated;
            tournamentDialog.CaptureToFile(Path.Combine(CaptureDirectory, $"walktrough{++captureIndex}.png"));
            var btnOk = tournamentDialog.FindAllByXPath("/Button").Where(e => e.AutomationId == "btnOk").First().AsButton();
            btnOk.Click();
        }

    }
}
