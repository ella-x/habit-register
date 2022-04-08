using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace habit_register
{
    [System.Runtime.InteropServices.Guid("AADDD6EC-CC4E-416D-8A29-D800978A72B7")]
    class Program
    {
        static string connectionString = @"Data Source = habitTracker.db";
        static void Main(string[] args)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var createTable = connection.CreateCommand();

                createTable.CommandText = @"CREATE TABLE IF NOT EXISTS sleeping_hours_per_day(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT,
                    MinutesOfSleepPerDay INTEGER
                )";

                createTable.ExecuteNonQuery();
                connection.Close();
            }

            GetUserInput();
        }
        //Managing the CRUD
        static void GetUserInput()
        {

            Console.Clear();
            bool closeApp = false;
            while (closeApp == false)
            {
                int averageHoursOfSleep = GetTotalHours();

                Console.WriteLine("\n\n                  MAIN MENU");
                Console.WriteLine("\n           What would you like to do");

                Console.WriteLine("\n         Type 0 to Close Application.");
                Console.WriteLine("\n         Type 1 to View All Records.");
                Console.WriteLine("\n         Type 2 to Insert Record.");
                Console.WriteLine("\n         Type 3 to Delete Record.");
                Console.WriteLine("\n         Type 4 to Update Record.");
                Console.WriteLine("----------------------------------------------------------\n");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "0":
                        Console.WriteLine("\nGoodbye!\n");
                        closeApp = true;
                        Environment.Exit(0);
                        break;
                    case "1":
                        GetAllRecords();
                        break;
                    case "2":
                        Insert();
                        break;
                    case "3":
                        Delete();
                        break;
                    case "4":
                        Update();
                        break;
                    default:
                        Console.WriteLine("\nInvalid input. Please type a number from 0 to 4.\n");
                        break;
                }
            }
        }

        private static void GetAllRecords()
        {
            Console.Clear();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var createTable = connection.CreateCommand();

                createTable.CommandText = $"SELECT * FROM sleeping_hours_per_day";

                List<HoursOfSleep> tableData = new();
                SqliteDataReader reader = createTable.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableData.Add(
                            new HoursOfSleep
                            {
                                Id = reader.GetInt32(0),
                                Date = DateTime.ParseExact(reader.GetString(1), "dd-mm-yy", new CultureInfo("en-US")),
                                MinutesOfSleepPerDay = reader.GetInt32(2)
                            }); ;
                    }
                }
                else
                {
                    Console.WriteLine("No rows found");
                }
                connection.Close();
                Console.WriteLine("----------------------------------------------------------\n\n");
                foreach (var dw in tableData)
                {
                    Console.WriteLine($"{dw.Id} - {dw.Date.ToString("dd-mm-yy")} - {dw.MinutesOfSleepPerDay}\n\n");
                }

                Console.WriteLine("----------------------------------------------------------\n");
            }
        }
        private static void Insert()
        {
            string date = GetDateInput();
            int minutes = GetNumberInput("\n\nPlease insert number of hours of sleep(no decimals are allowed)\n\n");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var createTable = connection.CreateCommand();

                createTable.CommandText = $"INSERT INTO sleeping_hours_per_day(Date, MinutesOfSleepPerDay) VALUES('{date}', {minutes})";

                createTable.ExecuteNonQuery();
                connection.Close();
            }
        }

        private static void Delete()
        {
            Console.Clear();
            GetAllRecords();

            var inputId = GetNumberInput("\n\nPlease type the Id of the record you want to delete or type 0 to return to Main Menu");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var createTable = connection.CreateCommand();

                createTable.CommandText = $"DELETE FROM sleeping_hours_per_day WHERE Id = {inputId}";
                int rowCount = createTable.ExecuteNonQuery();

                if (rowCount == 0)
                {
                    Console.WriteLine($"\n\nRecord with Id {inputId} doesn't exist. \n\n");
                    Delete();
                }

            }
        }

        private static void Update()
        {
            Console.Clear();
            GetAllRecords();

            var inputId = GetNumberInput("\n\nPlease type the Id of the record you want to update or type 0 to return to Main Menu");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var checkCmd = connection.CreateCommand();

                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM sleeping_hours_per_day WHERE Id = {inputId})";
                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (checkQuery == 0)
                {
                    Console.WriteLine($"\n\nRecord with Id {inputId} doesn't exist. \n\n");
                    connection.Close();
                    Update();
                }

                string date = GetDateInput();
                int minutes = GetNumberInput("\n\nPlease insert number of minutes of sleep(no decimals are allowed)\n\n");

                var createTable = connection.CreateCommand();
                createTable.CommandText = $"UPDATE sleeping_hours_per_day SET Date = '{date}', MinutesOfSleepPerDay = {minutes}  WHERE Id = {inputId}";

                createTable.ExecuteNonQuery();
                connection.Close();
            }

        }
        internal static int GetNumberInput(string message)
        {
            Console.WriteLine(message);
            string numberInput = Console.ReadLine();
            if (numberInput == "0") GetUserInput();

            while (!Int32.TryParse(numberInput, out _) || Convert.ToInt32(numberInput) < 0)
            {
                Console.WriteLine("\n\nInvalid number. Try again.\n\n");
                numberInput = Console.ReadLine();
            }
            int finalInput = Convert.ToInt32(numberInput);
            return finalInput;
        }
        internal static string GetDateInput()
        {
            DateTime startDate = new DateTime(2022, 1, 1, 8, 0, 1);
            DateTime endDate = new DateTime(2022, 6, 1, 8, 0, 1);
            DateTime currentDate = DateTime.Now;
            TimeSpan interval = currentDate - startDate;

            Console.WriteLine("\n\nPlease insert the date: (Format: dd-mm-yy).Type 0 to return to main menu.\n\n");
            string dateInput = Console.ReadLine();
            if (dateInput == "0") GetUserInput();
            while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
            {
                if (Convert.ToDateTime(dateInput) < startDate.Date || Convert.ToDateTime(dateInput) > endDate.Date)
                {
                    Console.WriteLine("\n\nInvalid date. Your sleeping hours tracer period is between 01/01/2002 to 01/06/2022 \n\n (Format: dd-mm-yy).Type 0 to return to main menu.\n\n");
                }
                    dateInput = Console.ReadLine();
            }
            return dateInput;
            //double totalDays = interval.Days;
        }

        private static int GetTotalHours()
        {
            int totalHoursTillNow;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var createTable = connection.CreateCommand();

                createTable.CommandText = $"SELECT ROUND(AVG(MinutesOfSleepPerDay)/60) from sleeping_hours_per_day";

                totalHoursTillNow = Convert.ToInt32(createTable.ExecuteScalar());
                connection.Close();
            }
            int totalHours = Convert.ToInt32(totalHoursTillNow);

            Console.WriteLine("\n\n\n\n----------------------------------------------------------\n");
            Console.WriteLine("\nYour average sleeping hours calculated till now: " + totalHours +"\n\n");
            Console.WriteLine("\n----------------------------------------------------------\n\n");
            if (totalHours < 7 || totalHours > 9)
            {
                Console.WriteLine("       Keep your sleeping hours on track\n\n");
                Console.WriteLine(" Most adults need 7 to 9 hours, although some people may need around 6 hours to 10 hours of sleep each day.");
                Console.WriteLine(" Older adults (ages 65 and older) need 7-8 hours of sleep each day.");
                Console.WriteLine(" Women in the first 3 months of pregnancy often need several more hours of sleep than usual");
            }
            return totalHours;
        }
    }
    public class HoursOfSleep
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int MinutesOfSleepPerDay { get; set; }
    }
}
