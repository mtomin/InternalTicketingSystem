using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Windows;
using Dapper;

namespace InternalTicketingSystem
{
    /// <summary>
    /// A simple login window which takes username and password and checks the database for validity. Passwords are salted and hashed.
    /// </summary>
    public partial class LoginWindow : Window
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["AppDB"].ConnectionString;
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string userName = usernameBox.Text;
            string password = passwordBox.Password;

            //Check if username and password have been entered
            if (userName == "" || password == "")
            {
                MessageBox.Show("Please enter both username and password!");
            }
            else
            {
                //Check if the user exists in the database
                User userLoginData = new User();
                using (IDbConnection connection = new SqlConnection(connectionString))
                {
                    userLoginData = connection.QuerySingleOrDefault<User>("[dbo].[GetLoginData] @Username", new { Username = userName });
                }

                if (userLoginData == null)
                {
                    MessageBox.Show("Incorrect username or password");
                }
                else
                {
                    //User exists; check if the password is correct
                    if (CheckPassword(password, userLoginData))
                    {
                        if (userLoginData.AdminFlag == 1)
                        {
                            AdminWindow adminWindow = new AdminWindow(userLoginData);
                            adminWindow.Show();
                        }
                        else
                        {
                            UserWindow userWindow = new UserWindow(userLoginData);
                            userWindow.Show();
                        }
                        this.Close();
                    }
                }
            }
        }

        public bool CheckPassword(string password, User userLoginData)
        {
            byte[] salt = Convert.FromBase64String(userLoginData.Salt);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 5000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] passwordHash = new byte[52];
            Array.Copy(salt, 0, passwordHash, 0, 32);
            Array.Copy(hash, 0, passwordHash, 32, 20);
            if (Convert.ToBase64String(passwordHash) != userLoginData.Password)
            {
                MessageBox.Show("Incorrect username or password");
                return false;
            }
            else
            {
                return true;
            }
            
        }
    }
}
