using codingTracker._0lcm.Models;
using codingTracker._0lcm.Services;
using codingTracker._0lcm.UserInterface;
using Spectre.Console;

namespace codingTracker._0lcm.User_Input
{
    internal class UserInputHelper
    {
        /// <summary>
        /// Asks user to manually input a valid start and end time and returns a new CodingSession
        /// or updates and returns a pre-existing CodingSession if one is passed through.
        /// </summary>
        internal static CodingSession GetSessionFromInput(CodingSession? session = null)
        {
            while (true)
            {
                Console.Clear();
                
                DisplayHelper.DisplayInfo("Please Follow a 24hr HH:mm Format (00:00-23:59), and a yyyy-MM-dd format. Press <Enter> to Submit Input.");
                DisplayHelper.DisplayInfo("Please Note That If Your End Time Is Before Your Start Time It Will Automatically Be Counted as a Cross-Midnight Session.\n");

                string startTimeInput = DisplayHelper.DisplayQuestion("Please Enter a Start Time:");
                string endTimeInput = DisplayHelper.DisplayQuestion("Please Enter an End Time:");
                string dateInput = DisplayHelper.DisplayQuestion("Please enter a date:");

                TimeSpan duration = TimeSpan.Zero;
                string? errorMessage = null;

                if (TimeValidationService.TryValidateStartAndEndTimes(startTimeInput, endTimeInput,
                    out DateTime startTime, out DateTime endTime, out errorMessage) &&
                    TimeValidationService.TryValidateDateTime(dateInput, out DateOnly date, out errorMessage))
                {
                    duration = endTime - startTime;
                    if (duration.TotalHours >= 10)
                    {
                        if (!AnsiConsole.Confirm($"[{DisplayHelper.Red}]This Session Is {duration.TotalHours:F1} Hours Long. Is That Right?[/]")) continue;
                    }

                    if (session != null)
                    {
                        session.StartTime = startTime;
                        session.EndTime = endTime;
                        session.Duration = duration;
                        session.Date = date;

                        return session;
                    }

                    return new CodingSession(
                            startTime: startTime,
                            endTime: endTime,
                            duration: endTime - startTime,
                            date: date
                        );
                }

                

                DisplayHelper.DisplayUrgent(errorMessage ?? "Invalid Input.");
                DisplayHelper.DisplayInfo("Press <Enter> To Re-enter Time Selections.");
                Console.ReadLine();
            }
        }

        internal static DateOnly GetDateInput()
        {
            while (true)
            {
                Console.Clear();

                DisplayHelper.DisplayInfo("Please Follow YYYY-MM-dd Format");

                string dateInput = DisplayHelper.DisplayQuestion("Please enter a date:");

                if (TimeValidationService.TryValidateDateTime(dateInput, out DateOnly date, out string? errorMessage)
                    && SessionService.CheckDateOnlyInSessions(date))
                {
                    return date;
                }
                else if (errorMessage == null && !SessionService.CheckDateOnlyInSessions(date))
                {
                    errorMessage = "Date Is Not In Recorded Sessions. Please Choose A New Date.";
                }

                DisplayHelper.DisplayUrgent(errorMessage ?? "Invalid Input.");
                DisplayHelper.DisplayInfo("Press <Enter> To Re-Enter Date Selection.");
                Console.ReadLine();
            }
        }
    }
}
