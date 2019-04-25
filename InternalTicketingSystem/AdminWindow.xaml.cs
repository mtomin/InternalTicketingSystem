using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InternalTicketingSystem
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["AppDB"].ConnectionString;
        List<User> userList = new List<User>();
        List<UserTicketDetails> openedTicketsList = new List<UserTicketDetails>();
        internal static User addedUser;

        public AdminWindow(User currentUser)
        {
            InitializeComponent();
            this.Title = String.Format("Logged in as {0} {1}", currentUser.FirstName, currentUser.LastName);

            LoadOpenTickets();

            //Hide the remove user button until user list is populated
            removeUserButton.Visibility = Visibility.Hidden;
        }

        private void LoadOpenTickets()
        {
            ///Loads all tickets currently opened by all users and binds the userTickets listview to the list of opened tickets
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                openedTicketsList = connection.Query<UserTicketDetails>("[dbo].[GetOpenTickets]").ToList();
            }

            openTicketsListView.DataContext = openedTicketsList;
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            ///Spawns a new instance of AddUserWindow
            AddUserWindow addUserWindow = new AddUserWindow();
            addUserWindow.ShowDialog();
            //If user was added, get data from AddUserWindow via static variable addedUser and update the list of users
            if (addedUser != null)
            {
                userList.Add(addedUser);
                usersListView.Items.Refresh();
            }
        }

        private void UserTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (openTicketsListView.SelectedIndex!=-1)
            {
                //Get the currently selected ticket details
                UserTicketDetails selectedUserTicket = (UserTicketDetails)openTicketsListView.SelectedItem;

                //If the user who opened the ticket was deleted and the ticket kept open, set the username for the "opened by *" message as (deleted)
                string userFirstName;
                string userLastName;
                if (selectedUserTicket.FirstName != null)
                {
                    userFirstName = selectedUserTicket.FirstName;
                }
                else
                {
                    userFirstName = "";
                }
                if (selectedUserTicket.LastName != null)
                {
                    userLastName = selectedUserTicket.LastName;
                }
                else
                {
                    userLastName = "(deleted)";
                }
                
                //Show the user who opened the ticket, date and ticket description
                ticketDescriptionTextbox.Text = String.Format("Submitted on {0} by {1} {2}\n\n", selectedUserTicket.Date.ToString("MM/dd/yyyy hh:mm"), userFirstName, userLastName);
                ticketDescriptionTextbox.Text += selectedUserTicket.IssueDescription;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ///Spawn a new LoginWindow and close current instance of AdminWindow
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void LoadUsersButton_Click(object sender, RoutedEventArgs e)
        {
            ///Populate the contents of the user list and enable the "Remove user" button
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                userList = connection.Query<User>("[dbo].[GetListOfUsers]").ToList();
            }

            usersListView.DataContext = userList;
            removeUserButton.Visibility = Visibility.Visible;
        }

        private void RemoveUserButton_Click(object sender, RoutedEventArgs e)
        {
            //Check if an user has been selected
            if (usersListView.SelectedIndex == -1)
                MessageBox.Show("No user selected!");
            else
            {
                User selectedUser = (User)usersListView.SelectedItem;
                
                MessageBoxResult messageBoxResult = MessageBox.Show(String.Format("Are you sure you want to remove user {0} {1} (UserID {2})?", selectedUser.FirstName, selectedUser.LastName, selectedUser.UserID), "Remove user?", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    //Remove user from database
                    using (IDbConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Execute("[dbo].[DeleteUser] @UserID", selectedUser);
                    }

                    //remove deleted results from user interface
                    userList.Remove(selectedUser);

                    //delete tickets opened by user?
                    messageBoxResult = MessageBox.Show(String.Format("Do you want to delete all tickets opened by user {0} {1}?", selectedUser.FirstName, selectedUser.LastName, selectedUser.UserID), "Remove user tickets?", MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        using (IDbConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Execute("[dbo].[DeleteUserTickets] @UserID", selectedUser);
                        }

                        //remove deleted results from user interface
                        var removeFromTicketList = openedTicketsList.AsEnumerable().Where(t => t.UserID == selectedUser.UserID).ToList();
                        foreach (var ticket in removeFromTicketList)
                        {
                            openedTicketsList.Remove(ticket);
                        }
                        openTicketsListView.Items.Refresh();
                    }
                    MessageBox.Show("Done!", "User successfully removed");
                    usersListView.Items.Refresh();
                }
            }
        }

        private void CloseTicketButton_Click(object sender, RoutedEventArgs e)
        {
            openTicketsListView.DataContext = openedTicketsList;
            if (openTicketsListView.SelectedIndex == -1)
            {
                MessageBox.Show("No ticket selected!");
            }
            else
            {
                //probaj sa selecteditem
                UserTicketDetails selectedTicket = (UserTicketDetails)openTicketsListView.SelectedItem;

                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to close this ticket?", "Close ticket", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    using (IDbConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Execute("[dbo].[CloseTicket] @TicketID", selectedTicket);
                    }

                    //remove deleted results from user interface
                    var removeFromTicketList = openedTicketsList.AsEnumerable().Where(t => t.TicketID == selectedTicket.TicketID).ToList();
                    foreach (var ticketToRemove in removeFromTicketList)
                    {
                        openedTicketsList.Remove(ticketToRemove);
                    }
                    openTicketsListView.Items.Refresh();
                    ticketDescriptionTextbox.Text = "";
                }
            }
        }
    }
}
