using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;

namespace Tables.Classes
{
    public partial class CustomMessageBox : Window
    {
        private readonly DispatcherTimer timer;

        public CustomMessageBox(string message)
        {
            InitializeComponent();
            MessageTextBlock.Text = message;

            // Initialize the timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            OKButton.IsEnabled = true;
            timer.Stop();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
