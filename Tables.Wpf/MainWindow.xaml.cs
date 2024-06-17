using System;
using System.IO;
using System.Media;
using System.Printing;
using System.Text.RegularExpressions;
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

        private readonly Random _rng = new();

        private int _answer;
        private int _tableNumber;
        private string? _selectedExercise;
        private int[]? _options;        

        private readonly DispatcherTimer _timer;
        private TimeSpan _elapsedTime;

        private string _user;

        private int _score;
        private int _highScore;
        private string[]? _allHighScores;
        private string? _highScorePath;

        // list to keep track of questions that have already appeared
        private List<string>? _usedQuestions;

        // bool to check whether all tables were selected
        private bool _allTables;

        #endregion

        #region constructor

        public MainWindow(string selectedUser)
        {
            InitializeComponent();
            _user = selectedUser;
            _timer = new DispatcherTimer();
        }

        #endregion

        #region events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {   
            // initialise _usedQuestions
            _usedQuestions = new List<string>();
            
            // timer settings
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            // add exercise options
            Seed();

            // read and use settings
            Settings();

            // no entries until exercise starts
            txtAnswer.IsEnabled = false;
            txtAnswer.Visibility = Visibility.Hidden;

            // select start button so exercise can be started as soon as enter is hit
            btnStart.Focus();          

            // show greeting on load
            lblQuestion.Content = $"Dag {_user} !";

            // load highscore for user (for "alles")
            string highScoreDir = Environment.CurrentDirectory + @"\highscores\";
            string userScoreFile = $"{_user}.score";
            _highScorePath = Path.Combine(highScoreDir, userScoreFile);

                // make a new high score file if one doesn't exist yet
                if (!File.Exists(_highScorePath))
                {
                    CreateScoreFile();
                }

            _allHighScores = ReadHighScores();
            LoadHighScore();

            // hide last score info at start
            lblLastScoreTitle.Visibility = Visibility.Hidden;
            lblLastScore.Visibility = Visibility.Hidden;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {                     
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));

            // default setting: one minute to provide as many answers as possible
            if (_elapsedTime.TotalSeconds == 60)
            {
                _timer.Stop();

                // play sound to alert user
                SystemSounds.Exclamation.Play();

                // update highscore if score > highscore
                CheckScore(_score);

                // show score of last attempt
                lblLastScore.Visibility = Visibility.Visible;
                lblLastScoreTitle.Visibility = Visibility.Visible;
                lblLastScore.Content = _score;

                // disable input 
                txtAnswer.Clear();
                txtAnswer.IsEnabled = false;

                // hide question
                lblQuestion.Visibility = Visibility.Hidden;                

                // popup to let user opt to go again or quit
                var popup = new CustomMessageBox($"Je score was {_score} - wil je nog eens proberen?");
                popup.ShowDialog();

                // if user clicks yes to go again
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

        // allow only numbers to be entered
        private void TxtAnswer_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextAllowed(e.Text);
        }

        private void TxtAnswer_KeyDown(object sender, KeyEventArgs e)
        {
            // user input with enter key
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(txtAnswer.Text))
            {
                try
                {
                    // convert entered text to int and remove spaces
                    int userAnswer = int.Parse(txtAnswer.Text.Trim());
                    if (userAnswer == _answer && _options != null)
                    {
                        _score++;
                        txtAnswer.Clear();
                        // filter out 10 voor "Alles" --> too easy
                        _tableNumber = _selectedExercise == "Alles"
                        ? _options[_rng.Next(0, _options.Length-1)]
                        : int.Parse(_selectedExercise);

                        GenerateQuestion(_tableNumber);
                        lblQuestion.Background = Brushes.Transparent;
                    }
                    else
                    {
                        lblQuestion.Background = Brushes.Pink;
                        Clear();
                    }
                }
                catch (Exception ex) 
                {
                    MessageBox.Show(ex.Message);
                }                            
            }
        }

        private void FirstQuestion()
        {
            if (_options != null)
            {
                // filter out 10 for "Alles" --> too easy
                _tableNumber = _selectedExercise == "Alles"
                ? _options[_rng.Next(0, _options.Length - 1)]
                : int.Parse(_selectedExercise);

                // load high score for selected table
                LoadHighScore();

                lblQuestion.Background = Brushes.Transparent;
                Clear();
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            // reset values
            _score = 0;
            _elapsedTime = TimeSpan.Zero;
            _usedQuestions?.Clear();

            // start timer
            _timer.Start();

            btnStart.Visibility = Visibility.Hidden;
            txtAnswer.Visibility = Visibility.Visible;
            lblQuestion.Visibility = Visibility.Visible;
            txtAnswer.IsEnabled = true;
            txtAnswer.Focus();

            FirstQuestion();
            GenerateQuestion(_tableNumber);
            lblQuestion.Background = Brushes.Transparent;
            Clear();
        }

        // open settings window
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        #endregion

        #region methods

        private void Seed()
        {
            _options = new int[] { 2, 3, 4, 5, 6, 8, 9, 10 };
        }

        private void Settings()
        {
            // read settings file
            string directory = Environment.CurrentDirectory;
            string fileName = "settings.cfg";
            string settingsFile = Path.Combine(directory, fileName);
            string[] settings = File.ReadAllLines(settingsFile);

            // first line in settings = table number
            _selectedExercise = settings[0];            

            // future setting --> timer length
        }

        private void LoadHighScore()
        {            
            string tableHighScore = "0";

            if (_allHighScores != null)
            {
                // select high score for current table number
                foreach (var item in _allHighScores)
                {
                    string[] itemSplit = item.Split(';');

                    for (int i = 0; i < itemSplit.Length-1; i++)
                    {
                        if (itemSplit[i] == _selectedExercise)
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

            _highScore = int.Parse(tableHighScore);
            lblHighScore.Content = tableHighScore;
        }

        private void GenerateQuestion(int tableNumber)
        {
            // flip a coin for multiplication/division
            int decision = _rng.Next(0, 2);

            int randomNr;
            string question;

            // generate random number for question
            randomNr = GenerateRandom();

            if (decision == 0)
            {
                // multiplication
                question = $"{randomNr} X {tableNumber}";
                // ensure no repeat questions
                while (_usedQuestions != null && _usedQuestions.Contains(question))
                {
                    randomNr = GenerateRandom();                    
                    question = $"{randomNr} X {tableNumber}";
                }
                _answer = tableNumber * randomNr;
                lblQuestion.Content = question;
            }
            else
            {
                // division
                question = $"{randomNr*tableNumber} : {tableNumber}";
                // ensure no repeat questions
                while (_usedQuestions != null && _usedQuestions.Contains(question))
                {
                    randomNr = GenerateRandom();
                    question = $"{randomNr*tableNumber} : {tableNumber}";
                }
                _answer = randomNr;                
                lblQuestion.Content = question;
            }

            _usedQuestions?.Add(question);

            lblQuestion.Background = Brushes.Transparent;
            Clear();
        }

        // generate random number for exercises
        // if not training mode, filter out numbers that are too easy
        private int GenerateRandom()
        {
            if (_selectedExercise == "Alles")
            {
                return _rng.Next(2, 10);
            }
            else
            {
                return _rng.Next(2, 11);
            }
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
            if (_highScorePath != null)
            {
                _allHighScores = File.ReadAllLines(_highScorePath);
                return _allHighScores;
            }

            return null;
        }

        // write new high score to file
        private void WriteHighScores()
        {
            if (_highScorePath != null && _allHighScores != null) 
            {
                File.WriteAllLines(_highScorePath, _allHighScores);
            }            
        }

        // check if last score is a new high score
        private void CheckScore(int score)
        {
            if (score > _highScore)
            {
                _highScore = score;
                EditHighScore();
                WriteHighScores();
            }
        }

        // change highscore entry for current table
        private void EditHighScore()
        {
            if (_allHighScores != null)
            {
                for (int i = 0; i < _allHighScores.Length - 1; i++)
                {
                    string[] itemSplit = _allHighScores[i].Split(';');
                    if (itemSplit[0] == _selectedExercise)
                    {
                        _allHighScores[i] = $"{_selectedExercise};{_highScore}";
                    }
                }

                if (_selectedExercise == "Alles")
                {
                    _allHighScores[_allHighScores.Length - 1] = $"{_selectedExercise};{_highScore}";
                }
            }
        }

        // make a new high score file if it doesn't exist yet
        private void CreateScoreFile()
        {            
            if (_options != null)
            {
                int arraySize = _options.Length + 1;

                _allHighScores = new string[arraySize];

                for (int i = 0; i < arraySize - 1; i++)
                {
                    _allHighScores[i] = _options[i] + ";0";
                }

                _allHighScores[arraySize - 1] = "Alles;0";

                WriteHighScores();
            }            
        }

        private bool TextAllowed(string text)
        {
            // use a regular expression to match only digits
            Regex regex = new Regex("[^0-9]+");
            return !regex.IsMatch(text);
        }

        #endregion        
    }
}
