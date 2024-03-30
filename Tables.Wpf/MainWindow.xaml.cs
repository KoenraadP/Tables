using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Tables.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Random rng = new();
        private int answer;
        private int tableNumber;
        private int[] options;
        private int score;
        private readonly DispatcherTimer timer;
        private TimeSpan elapsedTime;

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            txtAnswer.IsEnabled = false;
            btnStart.Focus();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
            tbkTimer.Text = elapsedTime.ToString(@"hh\:mm\:ss");

            if (elapsedTime.TotalSeconds == 60)
            {
                timer.Stop();
                txtAnswer.Clear();
                txtAnswer.IsEnabled = false;
                btnStart.Visibility = Visibility.Visible;
                btnStart.Focus();
                lblQuestion.Visibility = Visibility.Hidden;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbSelection.Items.Add("Alles");
            options = new int[]{ 2, 4, 5, 8, 10 };
            foreach (int option in options)
            {
                cmbSelection.Items.Add(option);
            }

            cmbSelection.SelectedIndex = 0;

            txtAnswer.Visibility = Visibility.Hidden;
            lblQuestion.Visibility = Visibility.Hidden;
        }

        private void txtAnswer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(txtAnswer.Text))
            {                
                int userAnswer = int.Parse(txtAnswer.Text);
                if (userAnswer == answer)
                {
                    score++;
                    lblScore.Content = $"Score: {score}";
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
        }

        private void cmbSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                tableNumber = cmbSelection.SelectedItem.ToString() == "Alles"
                ? options[rng.Next(0, options.Length)]
                : (int) cmbSelection.SelectedItem;

                GenerateQuestion(tableNumber);
                lblQuestion.Background = Brushes.Transparent;
                Clear();       
        }

        private void GenerateQuestion(int tableNumber)
        {
            int decision = rng.Next(0, 2);
            int randomNr = rng.Next(1, 11);

            if (decision == 0)
            {
                // multiplication                
                answer = tableNumber * randomNr;                
                lblQuestion.Content = $"{randomNr} X {tableNumber}";
            }
            else
            {
                // division
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

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            tableNumber = cmbSelection.SelectedItem.ToString() == "Alles"
                ? options[rng.Next(0, options.Length)]
                : (int)cmbSelection.SelectedItem;

            GenerateQuestion(tableNumber);
            lblQuestion.Background = Brushes.Transparent;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            score = 0;
            elapsedTime = TimeSpan.Zero;
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
    }
}