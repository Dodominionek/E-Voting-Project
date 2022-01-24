using System;
using System.Collections.Generic;
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
using HttpClient;
using RestSharp;
using Newtonsoft.Json;

namespace Client
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var request = new RestRequest("login",Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            var password = HttpClient.HttpClient.HashPassword(passBox.Password);
            var user = new User(loginBox.Text, password);
            request.AddJsonBody(user);
            var response = HttpClient.HttpClient.MakeRequest(request);
            if(!response.IsSuccessful)
            {
                incorrectLabel.Visibility = Visibility.Visible;
            }
            else
            {

                incorrectLabel.Visibility = Visibility.Hidden;
                var token = JsonConvert.DeserializeObject<Token>(response.Content);
                Console.WriteLine(token);
                MainWindow mainWindow = new MainWindow(token, user);
                mainWindow.Show();
                this.Close();
            }

        }
        private void RegisterButtonClick(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }

    }
}
