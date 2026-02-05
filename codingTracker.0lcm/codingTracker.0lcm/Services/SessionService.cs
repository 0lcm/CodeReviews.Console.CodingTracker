using codingTracker._0lcm.CRUD_Controller;
using codingTracker._0lcm.Models;

namespace codingTracker._0lcm.Services
{
    internal class SessionService
    {
        internal static void CreateNewSession(DateTime startTime, DateTime endTime, TimeSpan duration)
        {
            CodingSession session = new(
                startTime: startTime,
                endTime: endTime,
                duration: duration
                );

            SqliteController.InsertCodingSession(session);
        }

        internal static Task CreateTimerTask(Models.Timer timer, CancellationToken cancellationToken)
        {
            Task runTimer = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    timer.Stop();
                }
            });
            return runTimer;
        }

        internal static void SaveTimer(Models.Timer timer)
        {
            CodingSession session = new(
                startTime: timer.StartTime,
                endTime: timer.EndTime,
                duration: timer.EndTime - timer.StartTime
                );

            SqliteController.InsertCodingSession(session);
        }

        internal static List<CodingSession> GetFilteredSessions(DateOnly? filterDate = null, bool? ascending = true)
        {
            var sessions = SqliteController.GetAllSessions();

            if (filterDate.HasValue)
            {
                sessions = sessions.Where(s => s.Date == filterDate.Value).ToList();
            }

            if (ascending != null)
            {
                sessions = (bool)ascending
                        ? sessions.OrderBy(s => s.Duration).ToList()
                        : sessions.OrderByDescending(s => s.Duration).ToList();
            }

            return sessions;
        }

        internal static bool CheckDateOnlyInSessions(DateOnly date)
        {
            var sessions = SqliteController.GetAllSessions();
            foreach (var session in sessions)
            {
                if (date == session.Date) return true;
            }
            return false;
        }
    }
}
