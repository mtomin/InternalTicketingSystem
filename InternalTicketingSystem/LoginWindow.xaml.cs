using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
    /// Interaction logic for LoginWindow.xaml
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
            string username = usernameBox.Text;
            string password = passwordBox.Password;

            if (username == "" || password == "")
            {
                MessageBox.Show("Please enter both username and password!");
            }
            else
            {
                string query = "SELECT " +
                                "UserID, Username, Password, AdminFlag, Salt " +
                                "FROM UserITInfo " +
                                "WHERE Username=@userName";
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@userName", username);
                    DataTable userInfo = new DataTable();
                    adapter.Fill(userInfo);

                        if (userInfo.Rows.Count==0)
                    {
                        MessageBox.Show("Incorrect username or password");
                    }
                    else
                    {
                        byte[] salt = Convert.FromBase64String((string)userInfo.Rows[0]["Salt"]);
                        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 5000);
                        byte[] hash = pbkdf2.GetBytes(20);
                        byte[] passwordHash = new byte[52];
                        Array.Copy(salt, 0, passwordHash, 0, 32);
                        Array.Copy(hash, 0, passwordHash, 32, 20);
                        if (Convert.ToBase64String(passwordHash) != (string)userInfo.Rows[0]["Password"])
                        {
                            MessageBox.Show("Incorrect username or password");
                        }
                        else
                        {
                            if ((int)userInfo.Rows[0]["AdminFlag"] == 1)
                            {
                                AdminWindow adminWindow = new AdminWindow((int)userInfo.Rows[0]["UserID"]);
                                adminWindow.Show();
                            }
                            else
                            {
                                UserWindow userWindow = new UserWindow((int)userInfo.Rows[0]["UserID"]);
                                userWindow.Show();
                            }
                            this.Close();
                        }
                    }
                }
            }
        }
    }
}
