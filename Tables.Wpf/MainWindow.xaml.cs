﻿using System.IO;
using System.Media;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Tables.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region globals

        private readonly Random rng = new();

        private int answer;
        private int tableNumber;
        private string? selectedExercise;
        private int[]? options;        

        private readonly DispatcherTimer timer;
        private TimeSpan elapsedTime;

        private string user;

        private int score;
        private int highScore;
        private string[]? allHighScores;
        private string? highScorePath;        

        #endregion

        #region constructor

        public MainWindow(string selectedUser)
        {
            InitializeComponent();
            user = selectedUser;
            timer = new DispatcherTimer();
        }

        #endregion

        #region events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {   
            // timer settings
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            // add options to combobox
            Seed();

            // no entries until exercise starts
            txtAnswer.IsEnabled = false;
            txtAnswer.Visibility = Visibility.Hidden;

            // select start button so exercise can be started as soon as enter is hit
            btnStart.Focus();          

            // show greeting on load
            lblQuestion.Content = $"Dag {user} !";

            // load highscore for user (for "alles")
            string highScoreDir = Environment.CurrentDirectory + @"\highscores\";
            string userScoreFile = $"{user}.score";
            highScorePath = Path.Combine(highScoreDir, userScoreFile);

            // make a new high score file if one doesn't exist yet
            if (!File.Exists(highScorePath))
            {
                CreateScoreFile();
            }

            allHighScores = ReadHighScores();
            LoadHighScore();

            // hide last score info at start
            lblLastScoreTitle.Visibility = Visibility.Hidden;
            lblLastScore.Visibility = Visibility.Hidden;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {                     
            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));

            // default setting: one minute to provide as many answers as possible
            if (elapsedTime.TotalSeconds == 60)
            {
                // update highscore if score > highscore
                CheckScore(score);

                lblLastScore.Visibility = Visibility.Visible;
                lblLastScoreTitle.Visibility = Visibility.Visible;
                lblLastScore.Content = score;

                timer.Stop();
                txtAnswer.Clear();
                txtAnswer.IsEnabled = false;
                lblQuestion.Visibility = Visibility.Hidden;

                SystemSounds.Exclamation.Play();

                //var result = MessageBox.Show($"Je score was {score} - wil je nog eens proberen?", "Gedaan!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                var popup = new CustomMessageBox($"Je score was {score} - wil je nog eens proberen?");
                popup.ShowDialog();

                if (popup.Result)
                {
                    btnStart.Visibility = Visibility.Visible;
                    btnStart.Focus();
                    
                }
                else
                {
                    Close();
                }
            }
        }                

        private void TxtAnswer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(txtAnswer.Text))
            {
                try
                {
                    int userAnswer = int.Parse(txtAnswer.Text);
                    if (userAnswer == answer && options != null)
                    {
                        score++;
                        txtAnswer.Clear();
                        // filter out 10 voor "Alles" --> too easy
                        tableNumber = cmbSelection.SelectedItem.ToString() == "Alles"
                        ? options[rng.Next(0, options.Length-1)]
                        : (int)cmbSelection.SelectedItem;

                        GenerateQuestion(tableNumber);
                        lblQuestion.Background = Brushes.Transparent;
                    }
                    else
                    {
                        lblQuestion.Background = Brushes.Red;
                        Clear();
                    }
                }
                catch
                {

                }                            
            }
        }

        private void CmbSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {           
            if (options != null)
            {
                // filter out 10 for "Alles" --> too easy
                tableNumber = cmbSelection.SelectedItem.ToString() == "Alles"
                ? options[rng.Next(0, options.Length-1)]
                : (int)cmbSelection.SelectedItem;

                // load high score for selected table
                LoadHighScore();

                GenerateQuestion(tableNumber);
                lblQuestion.Background = Brushes.Transparent;
                Clear();
            }  
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            // reset values
            score = 0;
            elapsedTime = TimeSpan.Zero;

            // start timer
            timer.Start();

            btnStart.Visibility = Visibility.Hidden;
            txtAnswer.Visibility = Visibility.Visible;
            lblQuestion.Visibility = Visibility.Visible;
            txtAnswer.IsEnabled = true;
            txtAnswer.Focus();

            GenerateQuestion(tableNumber);
            lblQuestion.Background = Brushes.Transparent;
            Clear();
        }

        #endregion

        #region methods

        private void Seed()
        {
            cmbSelection.Items.Add("Alles");
            options = new int[] { 2, 4, 5, 8, 10 };
            foreach (int option in options)
            {
                cmbSelection.Items.Add(option);
            }

            cmbSelection.SelectedIndex = 0;
        }

        private void LoadHighScore()
        {            
            selectedExercise = cmbSelection.SelectedItem.ToString();
            string tableHighScore = "0";

            if (allHighScores != null)
            {
                // select high score for current table number
                foreach (var item in allHighScores)
                {
                    string[] itemSplit = item.Split(';');

                    for (int i = 0; i < itemSplit.Length-1; i++)
                    {
                        if (itemSplit[i] == selectedExercise)
                        {
                            tableHighScore = itemSplit[1];
                        }
                    }

                    if (string.IsNullOrEmpty(tableHighScore))
                    {
                        tableHighScore = itemSplit[itemSplit.Length-1];
                    }
                }
            }

            highScore = int.Parse(tableHighScore);
            lblHighScore.Content = tableHighScore;
        }

        private void GenerateQuestion(int tableNumber)
        {
            // flip a coin for multiplication/division
            int decision = rng.Next(0, 2);

            int randomNr;

            // if "alles" --> filter out 10, too easy
            if (selectedExercise == "Alles")
            {
                randomNr = rng.Next(2, 10);
            }
            else
            {
                randomNr = rng.Next(2, 11);
            }

            if (decision == 0)
            {
                // multiplication
                answer = tableNumber * randomNr;                
                lblQuestion.Content = $"{randomNr} X {tableNumber}";
            }
            else
            {
                // division
                // filter out 1 as the answer --> too easy
                while (randomNr == tableNumber)
                {
                    randomNr = rng.Next(2, 11);
                }

                answer = randomNr;
                lblQuestion.Content = $"{randomNr*tableNumber} : {tableNumber}";
            }

            lblQuestion.Background = Brushes.Transparent;
            Clear();
        }

        // empty input and put focus on it
        private void Clear()
        {            
            txtAnswer.Clear();
            txtAnswer.Focus();
        }        

        // read high scores from file
        private string[]? ReadHighScores()
        {
            if (highScorePath != null)
            {
                allHighScores = File.ReadAllLines(highScorePath);
                return allHighScores;
            }

            return null;
        }

        // write new high score to file
        private void WriteHighScores()
        {
            if (highScorePath != null && allHighScores != null) 
            {
                File.WriteAllLines(highScorePath, allHighScores);
            }            
        }

        // check if last score is a new high score
        private void CheckScore(int score)
        {
            if (score > highScore)
            {
                highScore = score;
                EditHighScore();
                WriteHighScores();
            }
        }

        // change highscore entry for current table
        private void EditHighScore()
        {
            if (allHighScores != null)
            {
                for (int i = 0; i < allHighScores.Length - 1; i++)
                {
                    string[] itemSplit = allHighScores[i].Split(';');
                    if (itemSplit[0] == selectedExercise)
                    {
                        allHighScores[i] = $"{selectedExercise};{highScore}";
                    }
                }

                if (selectedExercise == "Alles")
                {
                    allHighScores[allHighScores.Length - 1] = $"{selectedExercise};{highScore}";
                }
            }
        }

        // make a new high score file if it doesn't exist yet
        private void CreateScoreFile()
        {            
            if (options != null)
            {
                int arraySize = options.Length + 1;

                allHighScores = new string[arraySize];

                for (int i = 0; i < arraySize - 1; i++)
                {
                    allHighScores[i] = options[i] + ";0";
                }

                allHighScores[arraySize - 1] = "Alles;0";

                WriteHighScores();
            }            
        }

        #endregion
    }
}