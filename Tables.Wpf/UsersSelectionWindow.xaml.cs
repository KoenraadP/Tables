using System.IO;
using System.Windows;

namespace Tables.Wpf
{
    /// <summary>
    /// Interaction logic for UsersSelectionWindow.xaml
    /// </summary>
    public partial class UsersSelectionWindow : Window
    {
        public string user;

        public UsersSelectionWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path = Environment.CurrentDirectory;
            string fileName = "users.cfg";

            string[] users = File.ReadAllLines(Path.Combine(path, fileName));

            foreach (string user in users) 
            {
                lstUsers.Items.Add(user);            
            }

            lstUsers.SelectedIndex = 0;
        }

        private void btnSelectUser_Click(object sender, RoutedEventArgs e)
        {
            user = lstUsers.SelectedItem.ToString();

            MainWindow mainWindow = new MainWindow(user);
            mainWindow.Show();
            Close();
            //DialogResult = true;
            //Close();
        }
    }
}
