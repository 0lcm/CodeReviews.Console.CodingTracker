using codingTracker._0lcm.CRUD_Controller;
using codingTracker._0lcm.User_Interface;
using Microsoft.Extensions.Configuration;

namespace codingTracker._0lcm
{
    internal class Program
    {
        internal static readonly IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
            .Build();

        static async Task Main(string[] args)
        {
            SqliteController.CreateDatabase();
            await ConsoleUi.MainMenu();
        }
    }
}