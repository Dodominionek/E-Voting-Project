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
using SpecialVoting;
using RestSharp;
namespace Client
{
    /// <summary>
    /// Logika interakcji dla klasy AddVoting.xaml
    /// </summary>
    /// Validation rules
    /// At least 2 answers
    /// Start date cannot be in the past
    /// Check date time format
    public partial class AddVoting : Window
    {
        private readonly Token token = new Token();
        private readonly HttpClient.User user = new HttpClient.User();
        public AddVoting()
        {
            InitializeComponent();
        }
        public AddVoting(Token token, HttpClient.User user)
        {
            this.user = user;
            this.token = token;
            InitializeComponent();
        }

        private bool CheckAnswers()
        {
            if(answerBBox.IsEnabled&& answerBBox.Text.Length!=0 && answerABox.Text.Length!=0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
       

        private void CreateVoting(object sender, RoutedEventArgs e)
        {
            if(CheckAnswers() && CheckDates() && votingContentBox.Text.Length!=0)
            {
                SendVoting();
            }
            else
            {
                MessageBox.Show("Sprawdź dane");
            }
        }

        #region Voting Content
        private void VerifyVotingContent(object sender, RoutedEventArgs e)
        {
            VerifyVotingContent();
        }
        private void VerifyVotingContent(object sender, TextChangedEventArgs e)
        {
            VerifyVotingContent();
        }
        private void VerifyVotingContent()
        {
            if (votingContentBox.Text.Length == 0)
            {
                if (answerABox.IsEnabled)
                {
                    answerABox.IsEnabled = false;
                }
            }
            else
            {
                if(!answerABox.IsEnabled)
                {
                    answerABox.IsEnabled = true;
                }
            }
        }
        #endregion
        #region Answers
        private void AnswerALostFocus(object sender, RoutedEventArgs e)
        {
            if(answerABox.Text.Length==0)
            {
                if(answerBBox.IsEnabled)
                {
                    answerBBox.IsEnabled = false;
                }
                if (answerCBox.IsEnabled)
                {
                    answerCBox.IsEnabled = false;
                }
                if (answerDBox.IsEnabled)
                {
                    answerDBox.IsEnabled = false;
                }
            }
            else
            {
                if (!answerBBox.IsEnabled)
                {
                    answerBBox.IsEnabled = true;
                }
            }
        }
        private void AnswerBLostFocus(object sender, RoutedEventArgs e)
        {
            if (answerBBox.Text.Length == 0)
            {
                if (answerCBox.IsEnabled)
                {
                    answerCBox.IsEnabled = false;
                }
                if (answerDBox.IsEnabled)
                {
                    answerDBox.IsEnabled = false;
                }
            }
            else
            {
                if (!answerCBox.IsEnabled)
                {
                    answerCBox.IsEnabled = true;
                }
            }
        }
        private void AnswerCLostFocus(object sender, RoutedEventArgs e)
        {
            if (answerCBox.Text.Length == 0)
            {
                if (answerDBox.IsEnabled)
                {
                    answerDBox.IsEnabled = false;
                }
            }
            else
            {
                if (!answerDBox.IsEnabled)
                {
                    answerDBox.IsEnabled = true;
                }
            }
        }
        #endregion
        #region Time handling
        private void UseCurrentTime(object sender, RoutedEventArgs e)
        {
            DateTime localDate = DateTime.Now;
            var date = localDate.ToString();
            var split = date.Split(" ");
            try
            {
              
                var tempdate = split[0].Replace(".", "-");
                var dateSplit = tempdate.Split("-");
                startDateBox.Text = dateSplit[2] + "-" + dateSplit[1] + "-" + dateSplit[0];
                startTimeBox.Text = split[1];
                startDateBox.IsEnabled = false;
                startTimeBox.IsEnabled = false;

            }
            catch (IndexOutOfRangeException ex) { }
            
        }
        private void DateUnchecked(object sender, RoutedEventArgs e)
        {
            startDateBox.IsEnabled = true;
            startTimeBox.IsEnabled = true;
        }
        private static bool CheckDate(string inputDate)
        { // 2022-12-15
            //if (Regex.IsMatch(inputDate, @"^(([0-9]{4})-(0[0-9]|1[0-2])-([0-2][0-9]|[3[0-1]))$"))
            if (Regex.IsMatch(inputDate, @"^(([0-2][0-9]|[3[0-1]))-(0[0-9]|1[0-2])-([0-9]{4})$"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
     
        private bool CheckDates()
        {
            if (CheckDate(endDateBox.Text) && CheckDate(startDateBox.Text) && CheckTime(startTimeBox.Text) && CheckTime(endTimeBox.Text)) 
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
        private void CheckTime(object sender, RoutedEventArgs e)
        {
            TextBox tempBox = (TextBox)sender;
            if (!CheckTime(tempBox.Text))
            {
                tempBox.Text = "";
            }
        }
        private static bool CheckTime(string inputTime)
        {
            if (Regex.IsMatch(inputTime, @"^(([0-1][0-9])|(2[0-3]))(:[0-5][0-9]){2}$"))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        private void CheckDate(object sender, RoutedEventArgs e)
        {
            TextBox tempBox = (TextBox)sender;
            if (!CheckDate(tempBox.Text))
            {
                tempBox.Text = "";
            }
        }
        #endregion
        private void SendVoting()
        {
            var startDate = startDateBox.Text;
            var endDate = endDateBox.Text;
            var tmp = startDate.Split("-");
            //"timeStart": "2021-12-10 19:58:56.550604",
            startDate = tmp[2] + "-" + tmp[1] + "-" + tmp[0];
            tmp = endDate.Split("-");
            endDate = tmp[2] + "-" + tmp[1] + "-" + tmp[0];
            var request = new RestRequest("/voting", Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Authorization", "Bearer " + token.token);
            var voting = new SpecialVoting.Voting(answerABox.Text,answerBBox.Text,answerCBox.Text,answerDBox.Text,votingContentBox.Text,startDate+" "+ startTimeBox.Text+".000001", endDate + " " + endTimeBox.Text + ".000001");
            request.AddJsonBody(voting);
            var response = HttpClient.HttpClient.MakeRequest(request);
            if (!response.IsSuccessful)
            {
                MessageBox.Show("Wystąpił błąd podczas wysyłania głosowania");
            }
            else
            {
                MessageBox.Show("Dodano nowe głosowanie");
                MainWindow mainWindow = new MainWindow(token, user);
                mainWindow.Show();
                this.Close();

            }

        }

        private void ReturnButtonClicked(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(token, user);
            mainWindow.Show();
            this.Close();
        }
        private void Logout(object sender, EventArgs e)
        {
            var request = new RestRequest("/logout", Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Authorization", "Bearer " + token.token);
           // var response = HttpClient.HttpClient.MakeRequest(request);
        }
    }

}
