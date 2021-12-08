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
                UpdateList(); 
            }
        }
        private void UpdateList()
        {
            votings.Items.Clear();
            foreach (Voting elem in votingsList)
            {
                votings.Items.Add(elem.id + ". " + elem.question);
            }
        }
        /*
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var request = new RestRequest("login", Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            var user = new User(loginBox.Text, passBox.Password);
            request.AddJsonBody(user);
            var response = HttpClient.HttpClient.MakeRequest(request);
            if (!response.IsSuccessful)
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
        */
        private void Vote(char answer)
        {
            var votingName = votings.SelectedItem.ToString();
            int votingId;
            var converted = int.TryParse(votingName.Split('.')[0], out votingId);
            if(converted)
            {
                var request = new RestRequest("vote", Method.POST);
                request.RequestFormat = RestSharp.DataFormat.Json;
                var vote = new Vote(votingId, answer);
                request.AddJsonBody(vote);
                var response = HttpClient.HttpClient.MakeRequest(request);
                request.AddHeader("Authorization", "Bearer " + token.token);
                if (response.IsSuccessful)
                {
                    MessageBox.Show("Pomyślnie oddano głos");
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd przy oddawaniu głosowania");
                }
            }
        }
        private void DisplaySelected(object sender, SelectionChangedEventArgs e)
        {
            var item = (ListBox)sender;
            var votingName = item.SelectedItem.ToString();
            var votingIndex = votingName.Split('.')[0];
            foreach (Voting elem in votingsList)
            {
                if (String.Equals(elem.id.ToString(), votingIndex))
                {
                    votingDescription.Text = elem.question;
                    answerA.Content = elem.answerA;
                    answerB.Content = elem.answerB;
                    answerC.Content = elem.answerC;
                    answerD.Content = elem.answerD;
                    answerA.IsEnabled = true;
                    answerB.IsEnabled = true;
                    answerC.IsEnabled = true;
                    answerD.IsEnabled = true;
                    answerA.Visibility = Visibility.Visible;
                    answerB.Visibility = Visibility.Visible;
                    answerC.Visibility = Visibility.Visible;
                    answerD.Visibility = Visibility.Visible;
                    if (elem.answerA.Length == 0)
                    {
                        answerA.IsEnabled = false;
                        answerA.Visibility = Visibility.Hidden;
                    }
                    if (elem.answerB.Length == 0)
                    {
                        answerB.IsEnabled = false;
                        answerB.Visibility = Visibility.Hidden;
                    }
                    if (elem.answerC.Length == 0)
                    {
                        answerC.IsEnabled = false;
                        answerC.Visibility = Visibility.Hidden;
                    }
                    if (elem.answerD.Length == 0)
                    {
                        answerD.IsEnabled = false;
                        answerD.Visibility = Visibility.Hidden;
                    }
                    break;

                }
            }
        }

        private void AButtonClicked(object sender, RoutedEventArgs e)
        {
            Vote('A');
        }

        private void BButtonClicked(object sender, RoutedEventArgs e)
        {
            Vote('B');
        }

        private void CButtonClicked(object sender, RoutedEventArgs e)
        {
            Vote('C');
        }

        private void DButtonClicked(object sender, RoutedEventArgs e)
        {
            Vote('D');
        }
    }
}
