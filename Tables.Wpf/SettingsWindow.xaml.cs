using System.IO;
using System.Windows;

namespace Tables.Wpf
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        private string? _selectedExercise;
        private string? _settingsDirectory;
        private string? _fileName;
        private string? _settingsFile;
        private string[]? _settings;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        #region events

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // settings file options
            _settingsDirectory = Environment.CurrentDirectory;
            _fileName = "settings.cfg";
            _settingsFile = Path.Combine(_settingsDirectory, _fileName);

            // read settings file           
            _settings = File.ReadAllLines(_settingsFile);

            // populate combo box
            int[] exerciseOptions = new int[] {2, 3, 4, 5, 6, 7, 8, 9, 10};
            foreach (int option in exerciseOptions) { cmbExerciseOptions.Items.Add(option); }
            cmbExerciseOptions.Items.Add("Alles");

            // select currently saved option
            _selectedExercise = _settings[0];
            cmbExerciseOptions.SelectedItem = _selectedExercise;
        }

        private void CmbExerciseOptions_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedExercise = cmbExerciseOptions.SelectedItem.ToString();            

            if (_selectedExercise != null && _settings != null)
            {
                _settings[0] = _selectedExercise;
                WriteSettings(_settings);
            }
                    
        }

        #endregion

        #region methods

        private void WriteSettings(string[] settings)
        {
            if (_settingsFile != null && _settings != null)
            {
                File.WriteAllLines(_settingsFile, settings);
            }            
        }

        #endregion        
    }
}
