using System.IO;
using System.Media;
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
        private int[]? options;
        private int score;
        private readonly DispatcherTimer timer;
        private TimeSpan elapsedTime;
        private string user;
        private int highScore;

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

            // load highscore for user
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

                var result = MessageBox.Show($"Je score was {score} - wil je nog eens proberen?", "Gedaan!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
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
                    if (userAnswer == answer)
                    {
                        score++;
                        lblQuestion.Background = Brushes.Green;
                        btnNext.IsEnabled = true;
                        txtAnswer.Clear();
                        btnNext.Focus();
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
                tableNumber = cmbSelection.SelectedItem.ToString() == "Alles"
                ? options[rng.Next(0, options.Length)]
                : (int) cmbSelection.SelectedItem;

                GenerateQuestion(tableNumber);
                lblQuestion.Background = Brushes.Transparent;
                Clear();       
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

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            tableNumber = cmbSelection.SelectedItem.ToString() == "Alles"
                ? options[rng.Next(0, options.Length)]
                : (int)cmbSelection.SelectedItem;

            GenerateQuestion(tableNumber);
            lblQuestion.Background = Brushes.Transparent;
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
            highScore = ReadScore();
            lblHighScore.Content = highScore;
        }

        private void GenerateQuestion(int tableNumber)
        {
            int decision = rng.Next(0, 2);
            int randomNr = rng.Next(2, 11);

            if (decision == 0)
            {
                // multiplication                
                answer = tableNumber * randomNr;                
                lblQuestion.Content = $"{randomNr} X {tableNumber}";
            }
            else
            {
                // division
                while (randomNr == tableNumber)
                {
                    randomNr = rng.Next(2, 11);
                }

                answer = randomNr;
                lblQuestion.Content = $"{randomNr*tableNumber} : {tableNumber}";
            }

            lblQuestion.Background = Brushes.Transparent;
            Clear();
            btnNext.IsEnabled = false;
        }

        private void Clear()
        {            
            txtAnswer.Clear();
            txtAnswer.Focus();
        }        

        private int ReadScore()
        {
            string path = Environment.CurrentDirectory + @"\highscores\";
            string fileName = $"{user}.score";

            string fullPath = Path.Combine(path, fileName);

            if (File.Exists(fullPath))
            {
                highScore = int.Parse(File.ReadAllText(fullPath));
                return highScore;
            }

            return 0;
        }

        private void WriteScore()
        {
            string path = Environment.CurrentDirectory + @"\highscores\";
            string fileName = $"{user}.score";

            string fullPath = Path.Combine(path, fileName);

            File.WriteAllText(fullPath, highScore.ToString());
        }

        private void CheckScore(int score)
        {
            if (score > highScore)
            {
                highScore = score;
                WriteScore();
            }
        }

        #endregion
    }
}