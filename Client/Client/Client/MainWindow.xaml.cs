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
using Newtonsoft.Json;

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
        List<Voting> votingsList;
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
            votingsList = JsonConvert.DeserializeObject<List<Voting>>(response.Content);
            if (!response.IsSuccessful)
            {
                if (MessageBox.Show("Dane nie zostały poprawnie wczytane. Wczytać ponownie?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Init();
                }
                else
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }
            else
            {
                foreach(Voting elem in votingsList)
                {
                    votings.Items.Add(elem.id+". "+elem.question);
                }
                
            }
        }
        
        private void DisplaySelected(object sender, SelectionChangedEventArgs e)
        {
            var item = (ListBox)sender;
            var votingName = item.SelectedItem.ToString();
            var votingIndex = votingName[0];
            foreach (Voting elem in votingsList)
            {
                if (elem.id.ToString().Equals(votingIndex))
                {
                    votingDescription.Text = elem.question;
                    answerA.Content = elem.answerA;
                    answerB.Content = elem.answerB;
                    answerC.Content = elem.answerC;
                    answerD.Content = elem.answerD;

                }
            }
        }
    }
}
