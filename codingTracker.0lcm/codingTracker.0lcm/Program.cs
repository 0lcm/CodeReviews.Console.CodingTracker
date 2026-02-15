using codingTracker._0lcm.CRUD_Controller;
using codingTracker._0lcm.UserInterface;
using Microsoft.Extensions.Configuration;

namespace codingTracker._0lcm;

internal class Program
{
    internal static readonly IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
        .Build();

    static async Task Main()
    {
        SqliteController.CreateDatabase();
        await ConsoleUi.MainMenu();
    }
}