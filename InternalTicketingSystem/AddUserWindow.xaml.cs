using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
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
            User enteredUser = new User();

            if (ReadData(enteredUser))
            {
                string query = "INSERT INTO Users" +
                    "(FirstName, LastName, Address, City, Title) " +
                    "VALUES (@firstName, @lastName, @address, @city, @title); " +
                    "SELECT CAST(SCOPE_IDENTITY() AS INT)";
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@firstName", enteredUser.FirstName);
                    command.Parameters.AddWithValue("@lastName", enteredUser.LastName);
                    command.Parameters.AddWithValue("@address", enteredUser.Address);
                    command.Parameters.AddWithValue("@city", enteredUser.City);
                    command.Parameters.AddWithValue("@title", enteredUser.Title);
                    enteredUser.ID = (int)command.ExecuteScalar();
                }

                query = "INSERT INTO UserITInfo" +
                    "(UserID, Username, Password, AdminFlag, Salt) " +
                    "VALUES (@id, @username, @password, @adminFlag, @salt);";
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@id", enteredUser.ID);
                    command.Parameters.AddWithValue("@username", enteredUser.Username);
                    command.Parameters.AddWithValue("@password", enteredUser.Password);
                    command.Parameters.AddWithValue("@adminFlag", enteredUser.AdminFlag);
                    command.Parameters.AddWithValue("@salt", enteredUser.Salt);
                    command.ExecuteNonQuery();
                }
                MessageBox.Show("User successfully added!");
                this.Close();
            }
        }

        private bool ReadData(User enteredUser)
        {
            if (firstNameBox.Text == "")
            {
                MessageBox.Show("First name must be entered!");
                return false;
            }
            else
            {
                enteredUser.FirstName = firstNameBox.Text;
            }
            if (lastNameBox.Text == "")
            {
                MessageBox.Show("Last name must be entered!");
                return false;
            }
            else
            {
                enteredUser.LastName = lastNameBox.Text;
            }
            if (addressBox.Text == "")
            {
                MessageBox.Show("Address must be entered!");
                return false;
            }
            else
            {
                enteredUser.Address = addressBox.Text;
            }
            if (cityBox.Text == "")
            {
                MessageBox.Show("City must be entered!");
                return false;
            }
            else
            {
                enteredUser.City = cityBox.Text;
            }
            if (titleBox.Text == "")
            {
                MessageBox.Show("User title must be entered!");
                return false;
            }
            else
            {
                enteredUser.Title = titleBox.Text;
            }
            if (usernameBox.Text == "")
            {
                MessageBox.Show("Username must be entered!");
                return false;
            }
            else
            {
                string query = "SELECT Username " +
                               "FROM UserITInfo " +
                               "Where Username=@currentUsername";
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@currentUsername", usernameBox.Text);
                    connection.Open();
                    string existingUsernames = (string)command.ExecuteScalar();
                    if (existingUsernames == null)
                    {
                        enteredUser.Username = usernameBox.Text;
                    }
                    else
                    {
                        MessageBox.Show("Username already in use!");
                        return false;
                    }
                }
            }
            if (passwordBox.Password == "" || passwordBox2.Password == "")
            {
                MessageBox.Show("A password must be entered!");
                return false;
            }
            else if (passwordBox.Password != passwordBox2.Password)
            {
                MessageBox.Show("Entered passwords do not match!");
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
    }
}
