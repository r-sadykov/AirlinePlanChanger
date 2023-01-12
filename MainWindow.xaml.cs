using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AirlinePlanChanges_MailCenter.Model;
using AirlinePlanChanges_MailCenter.View;

namespace AirlinePlanChanges_MailCenter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private CurrentUserSettings _currentUserSettings;

        public MainWindow()
        {
            InitializeComponent();

            Directory.GetCurrentDirectory();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "bcc.info");
            BinaryFormatter formatter = new BinaryFormatter();
            if (File.Exists(path))
            {
                using(Stream stream = File.OpenRead(path))
                {
                    _currentUserSettings = (CurrentUserSettings)formatter.Deserialize(stream);
                }
                EndSearchDateDatePicker.SelectedDate = _currentUserSettings.EndDate;
                BeginningSearchDateDatePicker.SelectedDate = _currentUserSettings.StartDate;
                UserNameTextBox.Text = _currentUserSettings.OperatorName;
            }
            else
            {
                _currentUserSettings = new CurrentUserSettings();
                EndSearchDateDatePicker.SelectedDate = DateTime.Now;
                _currentUserSettings.WindowsUserName = Environment.UserName;
                _currentUserSettings.WindowsMachineName = Environment.MachineName;
            }

            WindowsUserNameLabel.Content = Environment.UserName;
            WindowsComputerNameLabel.Content = Environment.MachineName;

            CurrentDateLabel.Content = $"{DateTime.Now:dd MMMM yyyy}";
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            MainPage mainPage = new MainPage(this,ref _currentUserSettings);
            MainWindowFrameFrame.Navigate(mainPage);
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e) {
            CurrentTimeLabel.Content = DateTime.Now.ToString("HH:mm:ss");
            CommandManager.InvalidateRequerySuggested();
        }

        public event EventHandler<PropertyChangedEventArgs> PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MoveToConfigurationPage_Button_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationPage configurationPage = new ConfigurationPage(this, ref _currentUserSettings);
            MainWindowFrameFrame.Navigate(configurationPage);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(!String.IsNullOrWhiteSpace(UserNameTextBox.Text)) _currentUserSettings.OperatorName = UserNameTextBox.Text;
            _currentUserSettings.StartDate = BeginningSearchDateDatePicker.SelectedDate;
            _currentUserSettings.EndDate = EndSearchDateDatePicker.SelectedDate;
            _currentUserSettings.Dispose();
            Application.Current.Shutdown();
        }

        private void ConfirmationPageButton_Button_Click(object sender, RoutedEventArgs e)
        {
            BookingsData bookingsData = new BookingsData(this, _currentUserSettings);
            MainWindowFrameFrame.Navigate(bookingsData);
        }

        private void HomePageButton_Button_Click(object sender, RoutedEventArgs e)
        {
            MainPage mainPage = new MainPage(this, _currentUserSettings);
            MainWindowFrameFrame.Navigate(mainPage);
        }

        private void PassengerReceipt_Button_OnClick(object sender, RoutedEventArgs e)
        {
            PassengerReceiptPage receiptPage=new PassengerReceiptPage(_currentUserSettings);
            MainWindowFrameFrame.Navigate(receiptPage);
        }
    }
}
