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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HttpClient;
using RestSharp;


namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Token token = new Token();
        User user = new User();
        public MainWindow(Token token,User user)
        {
            this.user = user;
            this.token = token;
            InitializeComponent();
            Init();
        }
        void Init() {
            var request = new RestRequest("/voting?username=" + user.username, Method.GET);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Authorization", "Bearer " + token.token);
            var response = HttpClient.HttpClient.MakeRequest(request);

            if (!response.IsSuccessful)
            {
                
            }
            else
            {
              
            }
        }

    }
}
