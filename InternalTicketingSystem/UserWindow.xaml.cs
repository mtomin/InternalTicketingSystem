using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using Dapper;

namespace InternalTicketingSystem
{
    /// <summary>
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["AppDB"].ConnectionString;
        User currentUser = new User();
        List<UserTicketDetails> userOpenTickets = new List<UserTicketDetails>();

        public UserWindow(User currentUser)
        {
            //Pull user data and display the "Logged in as *user* window title
            InitializeComponent();
            this.currentUser = currentUser;
            this.Title = String.Format("Logged in as {0} {1}", currentUser.FirstName, currentUser.LastName);
            LoadUserTickets(currentUser.UserID);
        }

        private void SubmitTicketButton_Click(object sender, RoutedEventArgs e)
        {
            Ticket ticket = new Ticket { IssueDescription = ticketDescriptionTextbox.Text, IssueHeader = ticketHeaderTextbox.Text };
            if (ticket.IssueHeader == "" || ticket.IssueDescription == "")
            {
                MessageBox.Show("Please enter both ticket title and description!");
            }
            else
            {
                ticket.Date = DateTime.Now;
                ticket.UserID = currentUser.UserID;

                using (IDbConnection connection = new SqlConnection(connectionString))
                {
                    connection.Execute("[dbo].[InsertTicket] @UserID, @IssueDescription, @Date, @IssueHeader", ticket);
                }

                UserTicketDetails newTicket = new UserTicketDetails { FirstName = currentUser.FirstName, LastName = currentUser.LastName, IssueHeader = ticket.IssueHeader, Date = ticket.Date };
                userOpenTickets.Insert(0, newTicket);
                userTickets.Items.Refresh();

                MessageBox.Show("Ticket submitted successfully!");
                ticketDescriptionTextbox.Text = "";
                ticketHeaderTextbox.Text = "";
            }
        }
        private void LoadUserTickets(int userID)
        {
            List<UserTicketDetails> userInfo = new List<UserTicketDetails>();

            //Query the DB once, pulling all tickets from DB. Using left join in GetUserTickets to get user first and last name even if no tickets exist.
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                userInfo = connection.Query<UserTicketDetails>("[dbo].[GetUserTickets] @UserID", new { UserID = userID }).ToList();
            }

           //Filter out the closed tickets with LINQ to avoid querying the database two times. If no tickets exist, set listbox datacontext to null.
           //All tickets are grabbed for future implementation of "My closed tickets" listbox
            userOpenTickets = userInfo.Where(u => u.TicketClosedFlag == 0 && u.TicketID != 0).OrderByDescending(u => u.Date).ToList();
            if (userOpenTickets != null)
                userTickets.DataContext = userOpenTickets;
            else
                userTickets.DataContext = null;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new login window and close the existing instance of UserWindow
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
