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

namespace Tables.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random random = new Random();
        int answer;
        int tableNumber;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbSelection.Items.Add("Alles");
            cmbSelection.Items.Add(2);
            cmbSelection.Items.Add(4);
            cmbSelection.Items.Add(5);
            cmbSelection.Items.Add(10);

            txtAnswer.Focus();

            cmbSelection.SelectedIndex = 0;
        }

        private void txtAnswer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(txtAnswer.Text))
            {                
                int userAnswer = int.Parse(txtAnswer.Text);
                if (userAnswer == answer)
                {
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
            if(cmbSelection.SelectedIndex != 0)
            {
                tableNumber = (int)cmbSelection.SelectedItem;
                GenerateQuestion(tableNumber);
                lblQuestion.Background = Brushes.Transparent;
                Clear();
            }            
        }

        private void GenerateQuestion(int tableNumber)
        {
            int randomNr = random.Next(1, 11);
            answer = tableNumber * randomNr;
            lblQuestion.Background = Brushes.Transparent;
            Clear();
            btnNext.IsEnabled = false;
            lblQuestion.Content = $"{randomNr} X {tableNumber}";
        }

        private void Clear()
        {            
            txtAnswer.Clear();
            txtAnswer.Focus();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            GenerateQuestion(tableNumber);
            lblQuestion.Background = Brushes.Transparent;
        }
    }
}