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
using RestSharp;

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
            request.AddBody(new HttpClient.User(loginBox.Text, passBox.Password));
            //"http://" + apiAddress + ":" + apiPort + "//newtable";
            var response = HttpClient.HttpClient.MakeRequest(request);

        }
    }
}
