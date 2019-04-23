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
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["AppDB"].ConnectionString;
        User currentUser = new User();
        public UserWindow(int userID)
        {
            //TODO: Replace this with mapping - dapper supports it!
            LoadUserData(userID);
            
        }

        private void SubmitTicketButton_Click(object sender, RoutedEventArgs e)
        {
            if (ticketDescriptionTextbox.Text=="" || ticketHeaderTextbox.Text == "")
            {
                MessageBox.Show("Please enter both ticket title and description!");
            }
            else
            {
                string query = "INSERT INTO Tickets " +
                                "(UserID, IssueDescription, Date, IssueHeader)" +
                                "VALUES (@userID, @description, @date, @header)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@userID", currentUser.ID);
                    command.Parameters.AddWithValue("@description", ticketDescriptionTextbox.Text);
                    command.Parameters.AddWithValue("@date", DateTime.Now);
                    command.Parameters.AddWithValue("@header", ticketHeaderTextbox.Text);

                    command.ExecuteNonQuery();
                }
                LoadUserData(currentUser.ID);
                //userTickets.Items.Refresh();
            }
        }
        private void LoadUserData(int userID)
        {
            string query = "SELECT " +
                            "FirstName, LastName, IssueHeader, Date " +
                            "FROM Users LEFT JOIN Tickets on Users.UserID=Tickets.UserID " +
                            "WHERE Users.UserID=@userID " +
                            "ORDER BY Date DESC";

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

            userTickets.DataContext = userInfo;
        }
    }
}
