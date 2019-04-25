using Dapper;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Windows;


namespace InternalTicketingSystem
{
    /// <summary>
    /// Interaction logic for AddUserWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["AppDB"].ConnectionString;
 
        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            //Return the user which has been added to the database (or null if no user has been added) via the static variable AdminWindow.addedUser
            AdminWindow.addedUser = new User();

            //Check if all required data has been entered and update Users and UserITInfo tables
            if (ReadData(AdminWindow.addedUser))
            {
                using (IDbConnection connection = new SqlConnection(connectionString))
                {
                    AdminWindow.addedUser.UserID = connection.QuerySingle<int>("[dbo].[InsertUser] @UserID, @FirstName, @LastName, @Address, @City, @Title", AdminWindow.addedUser);
                    connection.Execute("[dbo].[InsertUserITInfo] @UserID, @Username, @Password, @AdminFlag, @Salt", AdminWindow.addedUser);
                }

                MessageBox.Show("User successfully added!");
                this.Close();
            }
            else
            {
                AdminWindow.addedUser = null;
            }
        }

        private bool ReadData(User enteredUser)
        {
            ///Check if all data has been entered and populate the User properties from window textboxes. Show appropriate errors when necessary
            if (firstNameBox.Text == "")
            {
                ShowErrorMessage("First name must be entered!");
                return false;
            }
            else
            {
                enteredUser.FirstName = firstNameBox.Text;
            }
            if (lastNameBox.Text == "")
            {
                ShowErrorMessage("Last name must be entered!");
                return false;
            }
            else
            {
                enteredUser.LastName = lastNameBox.Text;
            }
            if (addressBox.Text == "")
            {
                ShowErrorMessage("Address must be entered!");
                return false;
            }
            else
            {
                enteredUser.Address = addressBox.Text;
            }
            if (cityBox.Text == "")
            {
                ShowErrorMessage("City must be entered!");
                return false;
            }
            else
            {
                enteredUser.City = cityBox.Text;
            }
            if (titleBox.Text == "")
            {
                ShowErrorMessage("User title must be entered!");
                return false;
            }
            else
            {
                enteredUser.Title = titleBox.Text;
            }
            if (usernameBox.Text == "")
            {
                ShowErrorMessage("Username must be entered!");
                return false;
            }
            else
            {
                User existingUser = new User();
                using (IDbConnection connection = new SqlConnection(connectionString))
                {
                    existingUser = connection.QuerySingleOrDefault<User>("[dbo].[GetLoginData] @UserID", enteredUser);
                }
                
                if (existingUser == null)
                {
                    enteredUser.Username = usernameBox.Text;
                }
                else
                {
                    MessageBox.Show("Username already in use!");
                    return false;
                }

            }
            if (passwordBox.Password == "" || passwordBox2.Password == "")
            {
                ShowErrorMessage("A password must be entered!");
                return false;
            }
            else if (passwordBox.Password != passwordBox2.Password)
            {
                ShowErrorMessage("Entered passwords do not match!");
                return false;
            }
            else
            {
                HashPassword(passwordBox.Password, enteredUser);
            }
            if (isAdminBox.IsChecked == true)
            {
                enteredUser.AdminFlag = 1;
            }
            else
            {
                enteredUser.AdminFlag = 0;
            }
            return true;
        }
        private void HashPassword(string password, User enteredUser)
        {
            //generate random salt
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[32]);

            //hash the password + salt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 5000);
            byte[] hash = pbkdf2.GetBytes(20);

            //Create the hashed database entry
            byte[] passwordHash = new byte[52];
            Array.Copy(salt, 0, passwordHash, 0, 32);
            Array.Copy(hash, 0, passwordHash, 32, 20);

            enteredUser.Password=Convert.ToBase64String(passwordHash);
            enteredUser.Salt = Convert.ToBase64String(salt);
        }
        private void ShowErrorMessage(string errorString)
        {
            MessageBox.Show(errorString, "Invalid entry", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
