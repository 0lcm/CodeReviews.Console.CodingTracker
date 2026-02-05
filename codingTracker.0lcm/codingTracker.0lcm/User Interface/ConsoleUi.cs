using codingTracker._0lcm.CRUD_Controller;
using codingTracker._0lcm.Logging;
using codingTracker._0lcm.Models;
using codingTracker._0lcm.Services;
using codingTracker._0lcm.User_Input;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace codingTracker._0lcm.User_Interface
{
    internal class ConsoleUi
    {
        private static readonly ILogger Logger = AppLogger.CreateLogger<ConsoleUi>();

        private static readonly string DateIso = DateTimeFormats.DateIso;

        const string ReturnOption = "Return to Menu";
        const string FilterOption = "Filter Sessions";
        const string ClearFilterOption = "Clear Filters";

        //------- Main Menu -------
        internal static async Task MainMenu()
        {
            while (true)
            {
                try
                {
                    Console.Clear();
                    await HandleMainMenuChoice();
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }

        private static async Task HandleMainMenuChoice()
        {
            var mainMenuOption = DisplayHelper.DisplayMenu<Enums.MainMenuOption>();
            switch (mainMenuOption)
            {
                case Enums.MainMenuOption.NewSession:
                    CreateNewSession();
                    break;

                case Enums.MainMenuOption.ViewSessions:
                    ViewSessions();
                    break;

                case Enums.MainMenuOption.StartTimer:
                    await StartTimer();
                    break;

                case Enums.MainMenuOption.Exit:
                    ExitApplication();
                    break;
            }
        }

        private static void HandleException(Exception ex)
        {
            DisplayHelper.DisplayError("So Sorry, An Error Has Occured Somewhere:");
            Logger.LogError(ex, "An Error Ocurred In Main Menu, Or Subsequent Methods: ");

            DisplayHelper.DisplayWarning("Please Press Enter to Return to The Main Menu");
            Console.ReadLine();
        }

        private static void ExitApplication()
        {
            Console.Clear();
            DisplayHelper.DisplaySpinner("Closing Application...", 1500);
            Environment.Exit(0);
        }

        //------- CRUD Operations -------
        private static void CreateNewSession()
        {
            SqliteController.InsertCodingSession(UserInputHelper.GetSessionFromInput());

            DisplayHelper.DisplaySuccess("Succesfully Created Session!");
            DisplayHelper.DisplayInfo("Press <Enter> To Continue.");
            Console.ReadLine();
        }

        private static void UpdateSession(CodingSession session)
        {
            SqliteController.UpdateCodingSession(UserInputHelper.GetSessionFromInput(session));

            DisplayHelper.DisplaySuccess("Succesfully Updated Session!");
            DisplayHelper.DisplayInfo("Press <Enter> To Continue");
            Console.ReadLine();
        }

        private static void DeleteSession(CodingSession session)
        {
            if (AnsiConsole.Confirm($"[{DisplayHelper.Red}]Are You Sure You Want to Delete This Session?[/]"))
            {
                SqliteController.DeleteCodingSession(session);

                DisplayHelper.DisplaySuccess("Succesfully Deleted Session.");
                DisplayHelper.DisplayInfo("Press <Enter> To Continue.");
                Console.ReadLine();
            }
        }

        //------- View Sessions -------
        private static void ViewSessions()
        {
            DisplayHelper.DisplayInfo("Scroll With <Up> And <Down>. Press <Enter> To Choose An Option.");

            List<CodingSession>? filteredSessions = null;
            bool hasLoadedOnce = false;

            while (true)
            {
                Console.Clear();

                if (!hasLoadedOnce)
                {
                    DisplayHelper.DisplaySpinner("Loading Sessions...", 2000);
                    hasLoadedOnce = true;
                }

                var sessionMap = BuildSessionMap(filteredSessions);

                string choice = DisplayHelper.DisplayPrompt(
                    sessionMap.Keys.ToList(), 
                    title: "| ID \t| Date \t  | Duration |");

                if (choice == ReturnOption) return;
                if (choice == FilterOption)
                {
                    filteredSessions = FilterSessions();
                    continue;
                }
                if (choice == ClearFilterOption)
                {
                    filteredSessions = null;
                    continue;
                }

                LoadSpecificSession(sessionMap[choice]!);
            }
        }

        private static Dictionary<string, CodingSession> BuildSessionMap(List<CodingSession>? filteredSessions)
        {
            List<CodingSession> sessions = filteredSessions ?? SqliteController.GetAllSessions();
            var sessionMap = new Dictionary<string, CodingSession?>();

            sessionMap[ReturnOption] = null;
            sessionMap[FilterOption] = null;
            if (filteredSessions != null)
            {
                sessionMap[ClearFilterOption] = null;
            }

            foreach (var session in sessions)
            {
                string date = session.Date.ToString(DateIso);
                string display = $"ID: {session.Id}. {date} - {session.Duration}";
                sessionMap[display] = session;
            }

            return sessionMap;
        }

        private static void LoadSpecificSession(CodingSession session)
        {
            DisplayHelper.DisplaySpinner("Loading Session...", 1000);
            DisplayHelper.DisplaySuccess("Succesfully Loaded Session:\n");

            string date = session.Date.ToString(DateIso);
            var properties = new List<IRenderable>
            {
                new Markup($"[{DisplayHelper.White}]Date: [{DisplayHelper.Yellow}]{date}[/][/]"),
                new Markup($"[{DisplayHelper.White}]Start Time: [{DisplayHelper.Green}]{session.StartTime}[/][/]"),
                new Markup($"[{DisplayHelper.White}]End Time: [{DisplayHelper.Red}]{session.EndTime}[/][/]"),
                new Markup($"[{DisplayHelper.White}]Duration: [{DisplayHelper.Yellow}]{session.Duration}[/][/]")
            };
            DisplayHelper.DisplayRows(properties);
            DisplayHelper.DisplayMessage("\n");
            var choice = DisplayHelper.DisplayMenu<Enums.LoadSessionOption>();

            switch (choice)
            {
                case Enums.LoadSessionOption.Delete:
                    DeleteSession(session);
                    break;
                case Enums.LoadSessionOption.Update:
                    UpdateSession(session);
                    break;
                case Enums.LoadSessionOption.Return:
                    return;
            }
        }

        //------- Filter Operations -------
        private static List<CodingSession> FilterSessions()
        {
            var filterDateChoice = DisplayHelper.DisplayMenu<Enums.FilterSessionDateOption>
                (title: "Select 'Today' For Today's Date, 'Other' To Manually Insert a Date, and 'All' To See All Sessions.");

            var filterChoice = DisplayHelper.DisplayMenu<Enums.FilterSessionAscendingOption>
                (title: "Select 'Ascending' or 'Descending', or 'Default' For the Default Filtering.");

            DateOnly? filterDate = filterDateChoice switch
            {
                Enums.FilterSessionDateOption.Today => DateOnly.FromDateTime(DateTime.Today),
                Enums.FilterSessionDateOption.Other => UserInputHelper.GetDateInput(),
                Enums.FilterSessionDateOption.Default => null,
                _ => throw new NotImplementedException()
            };

            bool? ascending = filterChoice switch
            {
                Enums.FilterSessionAscendingOption.Ascending => true,
                Enums.FilterSessionAscendingOption.Descending => false,
                Enums.FilterSessionAscendingOption.Default => null,
                _ => throw new NotImplementedException()
            };

            return SessionService.GetFilteredSessions(filterDate, ascending);
        }

        //------- Timer -------
        private static async Task StartTimer()
        {
            var timer = new Models.Timer();

            var cts = new CancellationTokenSource();
            var task = SessionService.CreateTimerTask(timer, cts.Token);

            var spinnerTask = DisplayHelper.DisplayAsyncSpinner("Press 'Q' To Stop Timer", task);

            while (Console.ReadKey(true).Key != ConsoleKey.Q) { }
            cts.Cancel();

            await spinnerTask;

            SessionService.SaveTimer(timer);
            DisplayHelper.DisplaySpinner("Saving Coding Session...", 3500);
        }
    }
}