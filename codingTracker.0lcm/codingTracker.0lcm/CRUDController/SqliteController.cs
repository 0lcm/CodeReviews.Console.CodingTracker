using codingTracker._0lcm.Logging;
using codingTracker._0lcm.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace codingTracker._0lcm.CRUD_Controller;

internal class SqliteController
{
    private static readonly ILogger Logger = AppLogger.CreateLogger<SqliteController>();

    private static readonly string fullDateWithTimeFormat = DateTimeFormats.FullDateWithTimeFormat;
    private static readonly string dateIso = DateTimeFormats.DateIso;

    //------- Connection Factory -------
    private static SqliteConnection CreateOpenConnection()
    {
        string? connectionString = Program.configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }

    //------- Execution Helpers -------
    private static void Execute(string sql, object? parameters = null)
    {
        using var connection = CreateOpenConnection();
        connection.Execute(sql, parameters);
    }

    //------- Initlize Database -------
    internal static void CreateDatabase()
    {
        const string CreateTableQuery = @"
                CREATE TABLE IF NOT EXISTS codingSessions (
                id          INTEGER NOT NULL UNIQUE,
                startTime   TEXT NOT NULL,
                endTime     TEXT NOT NULL,
                duration    TEXT NOT NULL,
                date        TEXT NOT NULL,
                PRIMARY KEY(id AUTOINCREMENT)
                );";

        try
        {
            Execute(CreateTableQuery);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating database table.");
            throw;
        }
    }

    //------- CRUD Operations -------
    internal static void InsertCodingSession(CodingSession session)
    {
        const string InsertCommand = @"
            INSERT INTO codingSessions (startTime, endTime, duration, date)
            VALUES (@StartTime, @EndTime, @Duration, @Date);
            SELECT last_insert_rowid();
            ";

        try
        {
            using var connection = CreateOpenConnection();
            int sessionId = connection.ExecuteScalar<int>(InsertCommand, new
            {
                StartTime = session.StartTime.ToString(fullDateWithTimeFormat),
                EndTime = session.EndTime.ToString(fullDateWithTimeFormat),
                Duration = session.Duration.ToString(@"hh\:mm\:ss"),
                Date = session.Date.ToString(dateIso)
            });

            session.Id = sessionId;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error inserting coding session.");
            throw;
        }
    }

    internal static void DeleteCodingSession(CodingSession session)
    {
        const string DeleteCommand = "DELETE FROM codingSessions WHERE id = @Id";

        try
        {
            Execute(DeleteCommand, session);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while trying to delete a session.");
            throw;
        }
    }

    internal static void UpdateCodingSession(CodingSession session)
    {
        const string UpdateCommand = @"
            UPDATE codingSessions
            SET startTime = @StartTime, endTime = @EndTime, duration = @Duration
            WHERE id = @Id
            ";

        try
        {
            Execute(UpdateCommand, new
            {
                Id = session.Id,
                StartTime = session.StartTime.ToString(fullDateWithTimeFormat),
                EndTime = session.EndTime.ToString(fullDateWithTimeFormat),
                Duration = session.Duration.ToString(@"hh\:mm\:ss")
            });
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not update session");
            throw;
        }
    }

    //------- Queries -------
    internal static List<CodingSession> GetAllSessions()
    {
        const string Query = "SELECT * FROM codingSessions";

        using var connection = CreateOpenConnection();
        var results = connection.Query<dynamic>(Query).ToList();

        return results.Select(row => new CodingSession
        {
            Id = (int)row.id,
            StartTime = DateTime.Parse(row.startTime),
            EndTime = DateTime.Parse(row.endTime),
            Duration = TimeSpan.Parse(row.duration),
            Date = DateOnly.Parse(row.date)
        }).ToList();
    }
}