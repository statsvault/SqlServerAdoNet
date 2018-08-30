using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace StatKings.SqlServerAdoNet.IntegrationTests
{
    internal static class Helper
    {
        private static readonly string _onetimeSetupScriptTemp = "SqlServerOneTimeSetup_Temp.sql";

        /// <summary>
        /// Create test database.
        /// </summary>
        public static void CreateDatabase()
        {
            // Start the LocalDB instance.
            RunProcess("SqlLocalDB.exe", "start");

            // Get the one-time setup script and update it to point to the database directory.
            var databaseDirectory = Helper.GetDatabaseDirectory();
            var scriptPath = Path.Combine(databaseDirectory, "SqlServerOneTimeSetup.sql");
            var script = string.Format(File.ReadAllText(scriptPath), databaseDirectory);

            //// Save the updated script to a new file and run it.
            var tempScriptPath = Path.Combine(databaseDirectory, _onetimeSetupScriptTemp);
            File.WriteAllText(tempScriptPath, script);
            RunDatabaseScript(tempScriptPath);
        }

        /// <summary>
        /// Drop test database.
        /// </summary>
        public static void DropDatabase()
        {
            // Run the script to delete the database.
            var databaseDirectory = Helper.GetDatabaseDirectory();
            var scriptPath = Path.Combine(databaseDirectory, "SqlServerOneTimeTearDown.sql");
            RunDatabaseScript(scriptPath);

            // Delete the temp script file.
            var tempScriptPath = Path.Combine(databaseDirectory, _onetimeSetupScriptTemp);
            File.Delete(tempScriptPath);

            // Stop the LocalDB instance.
            RunProcess("SqlLocalDB.exe", "stop");
        }

        /// <summary>
        /// Run database script.
        /// </summary>
        /// <param name="scriptPath">Path of script to run.</param>
        public static void RunDatabaseScript(string scriptPath)
        {
            var args = string.Format("-S (localdb)\\MSSQLLocalDB -E  -i {0}", scriptPath);
            Helper.RunProcess("sqlcmd", args);
        }

        /// <summary>
        /// Get the full path of the Database directory.
        /// </summary>
        /// <returns>string</returns>
        public static string GetDatabaseDirectory()
        {
            var dirPath = Assembly.GetExecutingAssembly().Location;
            dirPath = Path.GetDirectoryName(dirPath);
            dirPath = dirPath.Substring(0, dirPath.IndexOf("\\bin"));
            return Path.GetFullPath(Path.Combine(dirPath, "Database"));
        }

        /// <summary>
        /// Run a process.
        /// </summary>
        /// <param name="fileName">Name of file to run.</param>
        /// <param name="arguments">Command line arguments.</param>
        public static void RunProcess(string fileName, string arguments)
        {
            var startInfo = new ProcessStartInfo(fileName, arguments);
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Verb = "runas";
            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
            }
        }

        /// <summary>
        /// Create a unit of work.
        /// </summary>
        /// <returns>IUnitOfWork</returns>
        public static IUnitOfWork CreateUnitOfWork()
        {
            return UnitOfWork.Create("Server=(localdb)\\MSSQLLocalDB;Database=AdoNetTest;Integrated Security=true;");
        }
    }
}
