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
using System.Text.RegularExpressions;
using HttpClient;
using Newtonsoft.Json;

using RestSharp;

namespace Client
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if(CheckData())
            {
                if(Register())
                {
                    MessageBox.Show("Rejestracja przebiegła pomyślnie");
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd");
                }
            }
            
        }
        private bool CheckData()
        {
            var regex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
            bool isValid = Regex.IsMatch(email.Text, regex);
            if (repPass.Password!=pass.Password)
            {
                MessageBox.Show("Hasła są różne");
               return false;
            }
            if (pass.Password.Length==0 || username.Text.Length==0 || email.Text.Length==0)
            {
                MessageBox.Show("Wypełnij wszystkie pola");
                return false;
            }
            if(!isValid)
            {
                MessageBox.Show("Podano niepoprawny email");
                return false;
            }
            return true;
        }
        private bool Register()
        {
            var request = new RestRequest("signup", Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            var user = new SpecialVoting.User(username.Text, pass.Password, email.Text) ;
            request.AddJsonBody(user);
            var response = HttpClient.HttpClient.MakeRequest(request);
            if (!response.IsSuccessful)
            {
                return false;
            }
            return true;

        }
        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
