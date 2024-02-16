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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbSelection.Items.Add(2);
            cmbSelection.Items.Add(4);
            cmbSelection.Items.Add(5);
            cmbSelection.Items.Add(10);

            txtAnswer.Focus();
        }

        private void txtAnswer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                
            }
        }

        private void cmbSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int tableNumber = (int)cmbSelection.SelectedItem;
            lblQuestion.Content = GenerateQuestion(tableNumber);
        }

        private string GenerateQuestion(int tableNumber)
        {
            int randomNr = random.Next(1, 11);
            answer = tableNumber * randomNr;
            return $"{randomNr} X {tableNumber}";
        }
    }
}