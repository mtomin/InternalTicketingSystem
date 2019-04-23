using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InternalTicketingSystem
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["AppDB"].ConnectionString;
        User currentUser = new User();
        DataTable ticketInfo = new DataTable();
        public AdminWindow(int userID)
        {
            //TODO: Replace this with mapping - dapper supports it!
            string query = "SELECT " +
                            "FirstName, LastName " +
                            "FROM Users "+
                            "WHERE UserID=@userID ";

            DataTable userInfo = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                connection.Open();
                command.Parameters.AddWithValue("@userID", userID);

                adapter.Fill(userInfo);
            }

            currentUser.ID = userID;
            currentUser.LastName = (string)userInfo.Rows[0]["LastName"];
            currentUser.FirstName = (string)userInfo.Rows[0]["FirstName"];
            InitializeComponent();
            this.Title = String.Format("Logged in as {0} {1}", currentUser.FirstName, currentUser.LastName);

            query = "SELECT " +
                    "Date, FirstName, LastName, IssueHeader, IssueDescription " +
                    "FROM Users INNER JOIN Tickets on Users.UserID=Tickets.UserID " +
                    "WHERE TicketClosedFlag=0";

            ticketInfo = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                connection.Open();
                command.Parameters.AddWithValue("@userID", userID);

                adapter.Fill(ticketInfo);
            }

            userTickets.DataContext = ticketInfo;
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            AddUserWindow addUserWindow = new AddUserWindow();
            addUserWindow.Show();
        }

        private void UserTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRow selectedDataRow = ticketInfo.Rows[userTickets.SelectedIndex];
            DateTime date = (DateTime)(selectedDataRow["Date"]);
            string userFirstName = (string)selectedDataRow["FirstName"];
            string userLastName = (string)selectedDataRow["LastName"];
            ticketDescriptionTextbox.Text = String.Format("Submitted on {0} by {1} {2}\n\n", date.ToString("MM/dd/yyyy hh:mm"), userFirstName, userLastName);
            ticketDescriptionTextbox.Text += selectedDataRow["IssueDescription"].ToString();
        }
    }
}
