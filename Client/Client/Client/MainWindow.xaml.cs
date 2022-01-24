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
            EnableButtons();
            ClearVoting();
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

        private void Vote(string answer)
        {
            var votingName = votings.SelectedItem.ToString();
            int votingId;
            var converted = int.TryParse(votingName.Split('.')[0], out votingId);
            if(converted)
            {
                var request = new RestRequest("vote", Method.POST);
                request.RequestFormat = RestSharp.DataFormat.Json;

                request.AddHeader("Authorization", "Bearer " + token.token);
                var vote = new Vote(votingId, answer);
                request.AddJsonBody(vote);
                var response = HttpClient.HttpClient.MakeRequest(request);
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
            if(item.SelectedItem==null)
            {
                return;
            }
            var votingName = item.SelectedItem.ToString();
            var votingIndex = votingName.Split('.')[0];
            foreach (Voting elem in votingsList)
            {
                if(elem.status=="Ended")
                {
                    DisableButtons();
                }
                else
                {
                    EnableButtons();
                }
                if (String.Equals(elem.id.ToString(), votingIndex))
                {
                    votingDescription.Text = elem.question;
                    answerA.Content = elem.answerA;
                    answerB.Content = elem.answerB;
                    answerC.Content = elem.answerC;
                    answerD.Content = elem.answerD;
   
                    answerA.Visibility = Visibility.Visible;
                    answerB.Visibility = Visibility.Visible;
                    answerC.Visibility = Visibility.Visible;
                    answerD.Visibility = Visibility.Visible;
                    if (elem.answerA.Length == 0)
                    {
                        answerA.Visibility = Visibility.Hidden;
                    }
                    if (elem.answerB.Length == 0)
                    {
                        answerB.Visibility = Visibility.Hidden;
                    }
                    if (elem.answerC.Length == 0)
                    {
                        answerC.Visibility = Visibility.Hidden;
                    }
                    if (elem.answerD.Length == 0)
                    {
                        answerD.Visibility = Visibility.Hidden;
                    }
                    break;

                }
            }
        }

        private void AButtonClicked(object sender, RoutedEventArgs e)
        {
            Vote("A");
            DisableButtons();
        }

        private void BButtonClicked(object sender, RoutedEventArgs e)
        {
            Vote("B");
            DisableButtons();
        }

        private void CButtonClicked(object sender, RoutedEventArgs e)
        {
            Vote("C");
            DisableButtons();
        }

        private void DButtonClicked(object sender, RoutedEventArgs e)
        {
            Vote("D");
            DisableButtons();
        }

        private void GetAvailableVotings(object sender, RoutedEventArgs e)
        {
            EnableButtons();
            ClearVoting();
            var request = new RestRequest("/voting?showUnvoted=True", Method.GET);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Authorization", "Bearer " + token.token);
            var response = HttpClient.HttpClient.MakeRequest(request);
            var templist = JsonConvert.DeserializeObject<List<Voting>>(response.Content);
            //votingsList
            votingsList.Clear();
            foreach(Voting vote in templist)
            {
                if(vote.status!="Ended")
                {
                    votingsList.Add(vote);
                }
            }
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
        private void GetEndedVotings(object sender, RoutedEventArgs e)
        {
            DisableButtons();
            ClearVoting();
            var request = new RestRequest("/voting?username=", Method.GET);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Authorization", "Bearer " + token.token);
            var response = HttpClient.HttpClient.MakeRequest(request);
            var templist = JsonConvert.DeserializeObject<List<Voting>>(response.Content);
            //votingsList
            votingsList.Clear();
            foreach (Voting vote in templist)
            {
                if (vote.status == "Ended")
                {
                    votingsList.Add(vote);
                }
            }
            AddResults();
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
        private void AddResults() 
        {
            foreach(Voting voting in votingsList)
            {
                var result = GetResult(voting.id);
                voting.question = "[Z]"+voting.question;
                var votes = result.answerA + result.answerB + result.answerC + result.answerD;
                if(votes!=0)
                {
                    voting.answerA = voting.answerA + "   " + result.answerA.ToString() + " - " + ((result.answerA * 100) / (votes)).ToString() + "%";
                    voting.answerB = voting.answerB + "   " + result.answerB.ToString() + " - " + ((result.answerB * 100) / (votes)).ToString() + "%";
                    voting.answerC = voting.answerC + "   " + result.answerC.ToString() + " - " + ((result.answerC * 100) / (votes)).ToString() + "%";
                    voting.answerD = voting.answerD + "   " + result.answerD.ToString() + " - " + ((result.answerD * 100) / (votes)).ToString() + "%";
                }
               
            }
        }
        private Result GetResult(int id)
        {
            var request = new RestRequest("/result?votingId=" + id.ToString(), Method.GET);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Authorization", "Bearer " + token.token);
            var response = HttpClient.HttpClient.MakeRequest(request);
            var result = JsonConvert.DeserializeObject<Result>(response.Content);
            return result;
        }
        private void GetActiveVotings(object sender, RoutedEventArgs e)
        {
            EnableButtons();
            ClearVoting();
            var request = new RestRequest("/voting?username=", Method.GET);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Authorization", "Bearer " + token.token);
            var response = HttpClient.HttpClient.MakeRequest(request);
            var templist = JsonConvert.DeserializeObject<List<Voting>>(response.Content);
            //votingsList
            votingsList.Clear();
            foreach (Voting vote in templist)
            {
                if (vote.status != "Ended")
                {
                    votingsList.Add(vote);
                }
            }
            if (!response.IsSuccessful)
            {
                if (MessageBox.Show("Dane nie zostały poprawnie wczytane. Wczytać ponownie?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    GetActiveVotings(sender, e);
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
        private void GetAllVotings(object sender, RoutedEventArgs e)
        {
            Init();
        }
        private void ClearVoting()
        {
            answerA.Visibility = Visibility.Hidden;
            answerB.Visibility = Visibility.Hidden;
            answerC.Visibility = Visibility.Hidden;
            answerD.Visibility = Visibility.Hidden;
            votingDescription.Text = "";

        }
        private void EnableButtons()
        {
            answerA.IsEnabled = true;
            answerB.IsEnabled = true;
            answerC.IsEnabled = true;
            answerD.IsEnabled = true;
        }
        private void DisableButtons()
        {
            answerA.IsEnabled = false;
            answerB.IsEnabled = false;
            answerC.IsEnabled = false;
            answerD.IsEnabled = false;
        }
        private void CreateNewVoting(object sender, RoutedEventArgs e)
        {
            AddVoting addVoting = new AddVoting(token, user);
            addVoting.Show();
            this.Close();
        }

        private void GetUsersVotings(object sender, RoutedEventArgs e)
        {
            
            ClearVoting();
            var request = new RestRequest("/voting?onlyOwned=True", Method.GET);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Authorization", "Bearer " + token.token);
            var response = HttpClient.HttpClient.MakeRequest(request);
            var templist = JsonConvert.DeserializeObject<List<Voting>>(response.Content);
            //votingsList
            votingsList.Clear();
            foreach (Voting vote in templist)
            {
                if (vote.status != "Ended")
                {
                    votingsList.Add(vote);
                }
            }
            if (!response.IsSuccessful)
            {
                if (MessageBox.Show("Dane nie zostały poprawnie wczytane. Wczytać ponownie?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    GetUsersVotings(sender, e);
                }
                else
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }
            else
            {
                endVotingButton.Visibility = Visibility.Visible;
                UpdateList();
            }
        }

        private void Logout(object sender, EventArgs e)
        {
            var request = new RestRequest("/logout", Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Authorization", "Bearer " + token.token);
            var response = HttpClient.HttpClient.MakeRequest(request);
        }
        private void GetMyVotings()
        {

            
        }

        private void EndVoting(object sender, RoutedEventArgs e)
        {

        }
    }
}
