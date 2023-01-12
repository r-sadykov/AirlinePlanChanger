using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AirlinePlanChanges_MailCenter.AppData.Database;
using AirlinePlanChanges_MailCenter.Migrations;
using AirlinePlanChanges_MailCenter.Model;
using AirlinePlanChanges_MailCenter.Utils;
using Microsoft.Win32;

namespace AirlinePlanChanges_MailCenter.View
{
    /// <summary>
    /// Логика взаимодействия для ConfigurationPage.xaml
    /// </summary>
    public partial class ConfigurationPage
    {
        private readonly MainWindow _mainWindow;
        private CurrentUserSettings _userSettings;
        private readonly DispatcherTimer _timer;
        private bool _showEmailPass;
        private bool _showDatabasePass;

        internal ConfigurationPage(MainWindow mainWindow, ref CurrentUserSettings currentUserSettings)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _userSettings = currentUserSettings;
            SecureConnectionOnServerListView.ItemsSource = EnumUtility.GetValuesAndDescriptions(typeof(ServerMailConnectionParameters));
            UserNameOnServerTextBox.IsEnabled = false;
            _timer = new DispatcherTimer();
            _timer.Tick += DispatcherTimer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 5);
            try
            {
                Init();
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.HelpLink, "Error in loading Configuration page", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ConfigurationPage(ConfigurationPage configurationPage, MainWindow mainWindow)
        {
            InitializeComponent();

            _mainWindow = mainWindow;
            _userSettings = configurationPage._userSettings;

            Init();
        }

        private void Init() {
            if (_userSettings.HaveAllSettingsUp)
            {
                EMailUserNameTextBox.Text = _userSettings.EmailSetting.EmailUserName;
                EMailUserNameTextBox.IsEnabled = false;
                EMailAddressTextBox.Text = _userSettings.EmailSetting.EmailAddress;
                EMailAddressTextBox.IsEnabled = false;
                EMailPasswordPasswordBox.Password = _userSettings.EmailSetting.EmailPassword;
                EMailPasswordPasswordBox.IsEnabled = false;
                ServerNameOfIncomeMessageTextBox.Text = _userSettings.EmailSetting.ServerNameOfIncomeMessages;
                ServerNameOfIncomeMessageTextBox.IsEnabled = false;
                IncomeMessagesPortTextBox.Text = _userSettings.EmailSetting.ServerPortOfIncomeMessages.ToString();
                IncomeMessagesPortTextBox.IsEnabled = false;
                ServerNameOfOutcomeMessageTextBox.Text = _userSettings.EmailSetting.ServerNameOfOutcomeMessages;
                ServerNameOfOutcomeMessageTextBox.IsEnabled = false;
                OutcomeMessagesPortTextBox.Text = _userSettings.EmailSetting.ServerPortOfOutcomeMessages.ToString();
                OutcomeMessagesPortTextBox.IsEnabled = false;
                SecureConnectionOnServerListView.SelectedValue = _userSettings.EmailSetting.ServerSecureConnectionParameters;
                SecureConnectionOnServerListView.IsEnabled = false;
                UseNameAndPasswordCheckBox.IsChecked = _userSettings.EmailSetting.UseNameAndPassword;
                UseNameAndPasswordCheckBox.IsEnabled = false;
                UserNameOnServerTextBox.Text = _userSettings.EmailSetting.UsedNameForEmailConnection;
                UserNameOnServerTextBox.IsEnabled = false;
                UseSecureAuthenticationCheckBox.IsChecked = _userSettings.EmailSetting.UseSecureAuthentication;
                UseSecureAuthenticationCheckBox.IsEnabled = false;
                ShowEmailPasswordButton.IsEnabled = false;

                DatabaseDataSourceTextBox.Text = _userSettings.DatabaseSetting.DatabaseDataSource;
                DatabaseDataSourceTextBox.IsEnabled = false;
                DatabaseFileInLocalFolderTextBox.Text = _userSettings.DatabaseSetting.DatabaseLocalPathString;
                DatabaseFileInLocalFolderTextBox.IsEnabled = false;
                DatabaseInitialCatalogTextBox.Text = _userSettings.DatabaseSetting.DatabaseInitialCatalog;
                DatabaseInitialCatalogTextBox.IsEnabled = false;
                DatabaseIntegratedSecurityCheckBox.IsChecked = _userSettings.DatabaseSetting.DatabaseIntegratedSecurity;
                DatabaseIntegratedSecurityCheckBox.IsEnabled = false;
                DatabasePasswordPasswordBox.Password = _userSettings.DatabaseSetting.DatabasePassword;
                DatabasePasswordPasswordBox.IsEnabled = false;
                DatabaseUserNameTextBox.Text = _userSettings.DatabaseSetting.DatabaseUserName;
                DatabaseUserNameTextBox.IsEnabled = false;
                ShowDatabasePasswordButton.IsEnabled = false;
                OpenDatabaseFolderButton.IsEnabled = false;
                MozillaThunderbirdInboxLocalFolderTextBox.Text = _userSettings.MozillaThunderbirdInboxFilePath;
                MozillaThunderbirdInboxLocalFolderTextBox.IsEnabled = false;
                OpenInboxFolderButton.IsEnabled = false;
                MozillaThunderbirdFtcMessagesLocalFolderTextBox.Text = _userSettings.MozillaFtcFilePath;
                OpenFtcFolderFolderButton.IsEnabled = false;
                MozillaThunderbirdFtcCompletedMessagesLocalFolderTextBox.Text =
                    _userSettings.MozillaFtcCompletedFilePath;
                OpenFtcCompletedFolderFolderButton.IsEnabled = false;
            }
        }

        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            MainPage mainPage = new MainPage(_mainWindow, ref _userSettings);
            _mainWindow.MainWindowFrameFrame.Navigate(mainPage);
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            bool status = true;
            if (String.IsNullOrWhiteSpace(EMailUserNameTextBox.Text))
            {
                EMailUserNameTextBox.Text = "Please fill this field";
                EMailUserNameTextBox.Foreground = new SolidColorBrush(Colors.Red);
                status = false;
            }
            if (String.IsNullOrWhiteSpace(EMailAddressTextBox.Text))
            {
                EMailAddressTextBox.Text = "Please fill this field";
                EMailAddressTextBox.Foreground = new SolidColorBrush(Colors.Red);
                status = false;
            }
            if (String.IsNullOrWhiteSpace(EMailPasswordPasswordBox.Password))
            {
                EMailPasswordPasswordBox.Background = new SolidColorBrush(Colors.Red);
                status = false;
            }
            if (String.IsNullOrWhiteSpace(ServerNameOfIncomeMessageTextBox.Text))
            {
                ServerNameOfIncomeMessageTextBox.Text = "Please fill this field";
                ServerNameOfIncomeMessageTextBox.Foreground = new SolidColorBrush(Colors.Red);
                status = false;
            }
            int incomePort =0;
            if (String.IsNullOrWhiteSpace(IncomeMessagesPortTextBox.Text) || !Int32.TryParse(IncomeMessagesPortTextBox.Text, out incomePort))
            {
                IncomeMessagesPortTextBox.Background = new SolidColorBrush(Colors.Red);
                status = false;
            }
            if (String.IsNullOrWhiteSpace(ServerNameOfOutcomeMessageTextBox.Text))
            {
                ServerNameOfOutcomeMessageTextBox.Text = "Please fill this field";
                ServerNameOfOutcomeMessageTextBox.Foreground = new SolidColorBrush(Colors.Red);
                status = false;
            }
            int outcomePort =0;
            if (String.IsNullOrWhiteSpace(OutcomeMessagesPortTextBox.Text)||!Int32.TryParse(OutcomeMessagesPortTextBox.Text,out outcomePort))
            {
                OutcomeMessagesPortTextBox.Background = new SolidColorBrush(Colors.Red);
                status = false;
            }

            if (String.IsNullOrWhiteSpace(DatabaseDataSourceTextBox.Text))
            {
                DatabaseDataSourceTextBox.Text = "Please fill this field";
                DatabaseDataSourceTextBox.Foreground = new SolidColorBrush(Colors.Red);
                status = false;
            }
            if (String.IsNullOrWhiteSpace(DatabaseFileInLocalFolderTextBox.Text) && String.IsNullOrWhiteSpace(DatabaseInitialCatalogTextBox.Text))
            {
                DatabaseFileInLocalFolderTextBox.Text = "Please fill this field or field in DB Initial Catalog";
                DatabaseFileInLocalFolderTextBox.Foreground = new SolidColorBrush(Colors.Red);
                DatabaseInitialCatalogTextBox.Text = "Please fill this field or field in DB File Path";
                DatabaseInitialCatalogTextBox.Foreground = new SolidColorBrush(Colors.Red);
                status = false;
            }
            if (String.IsNullOrWhiteSpace(DatabasePasswordPasswordBox.Password) && !String.IsNullOrWhiteSpace(DatabaseInitialCatalogTextBox.Text))
            {
                DatabasePasswordPasswordBox.Background = new SolidColorBrush(Colors.Red);
                status = false;
            }
            if (String.IsNullOrWhiteSpace(DatabaseUserNameTextBox.Text) && !String.IsNullOrWhiteSpace(DatabaseInitialCatalogTextBox.Text))
            {
                DatabaseUserNameTextBox.Text = "Please fill this field";
                DatabaseUserNameTextBox.Foreground = new SolidColorBrush(Colors.Red);
                status = false;
            }

            if (String.IsNullOrWhiteSpace(MozillaThunderbirdInboxLocalFolderTextBox.Text))
            {
                MozillaThunderbirdInboxLocalFolderTextBox.Text = "Please fill this field";
                MozillaThunderbirdInboxLocalFolderTextBox.Foreground = new SolidColorBrush(Colors.Red);
                status = false;
            }

            if (status)
            {
                EmailSettings email = new EmailSettings
                {
                    EmailUserName = EMailUserNameTextBox.Text,
                    EmailAddress = EMailAddressTextBox.Text,
                    EmailPassword = EMailPasswordPasswordBox.Password,
                    ServerNameOfIncomeMessages = ServerNameOfIncomeMessageTextBox.Text,
                    ServerPortOfIncomeMessages = incomePort,
                    ServerNameOfOutcomeMessages = ServerNameOfOutcomeMessageTextBox.Text,
                    ServerPortOfOutcomeMessages = outcomePort,
                    ServerSecureConnectionParameters = (ServerMailConnectionParameters)SecureConnectionOnServerListView.SelectedValue
                };
                if (UseNameAndPasswordCheckBox.IsChecked == true)
                {
                    email.UseNameAndPassword = true;
                    email.UsedNameForEmailConnection = UserNameOnServerTextBox.Text;
                }
                else
                {
                    email.UseNameAndPassword = false;
                }
                email.UseSecureAuthentication = UseSecureAuthenticationCheckBox.IsChecked == true;
                email.IsFullAvailable = true;
                DatabaseSettings database = new DatabaseSettings
                {
                    DatabaseDataSource = DatabaseDataSourceTextBox.Text
                };
                if (String.IsNullOrWhiteSpace(DatabaseFileInLocalFolderTextBox.Text)) database.DatabaseLocalPathString = String.Empty;
                else database.DatabaseLocalPathString = DatabaseFileInLocalFolderTextBox.Text;
                if (String.IsNullOrWhiteSpace(DatabaseInitialCatalogTextBox.Text)) database.DatabaseInitialCatalog = String.Empty;
                else database.DatabaseInitialCatalog = DatabaseInitialCatalogTextBox.Text;
                if (String.IsNullOrWhiteSpace(DatabasePasswordPasswordBox.Password)) database.DatabasePassword = String.Empty;
                else database.DatabasePassword = DatabasePasswordPasswordBox.Password;
                if (String.IsNullOrWhiteSpace(DatabaseUserNameTextBox.Text)) database.DatabaseUserName = String.Empty;
                else database.DatabaseUserName = DatabaseUserNameTextBox.Text;
                database.DatabaseIntegratedSecurity = DatabaseIntegratedSecurityCheckBox.IsChecked == true;
                database.IsFullAvailable = true;
                _userSettings.DatabaseSetting = database;
                _userSettings.EmailSetting = email;
                _userSettings.MozillaThunderbirdInboxFilePath = MozillaThunderbirdInboxLocalFolderTextBox.Text;
                _userSettings.MozillaFtcFilePath = MozillaThunderbirdFtcMessagesLocalFolderTextBox.Text;
                _userSettings.MozillaFtcCompletedFilePath =
                    MozillaThunderbirdFtcCompletedMessagesLocalFolderTextBox.Text;

                EMailUserNameTextBox.IsEnabled = false;
                EMailAddressTextBox.IsEnabled = false;
                EMailPasswordPasswordBox.IsEnabled = false;
                ServerNameOfIncomeMessageTextBox.IsEnabled = false;
                IncomeMessagesPortTextBox.IsEnabled = false;
                ServerNameOfOutcomeMessageTextBox.IsEnabled = false;
                OutcomeMessagesPortTextBox.IsEnabled = false;
                SecureConnectionOnServerListView.IsEnabled = false;
                UseNameAndPasswordCheckBox.IsEnabled = false;
                UserNameOnServerTextBox.IsEnabled = false;
                UseSecureAuthenticationCheckBox.IsEnabled = false;
                ShowEmailPasswordButton.IsEnabled = false;
                DatabaseDataSourceTextBox.IsEnabled = false;
                DatabaseFileInLocalFolderTextBox.IsEnabled = false;
                DatabaseInitialCatalogTextBox.IsEnabled = false;
                DatabaseIntegratedSecurityCheckBox.IsEnabled = false;
                DatabasePasswordPasswordBox.IsEnabled = false;
                DatabaseUserNameTextBox.IsEnabled = false;
                ShowDatabasePasswordButton.IsEnabled = false;
                OpenDatabaseFolderButton.IsEnabled = false;
                MozillaThunderbirdInboxLocalFolderTextBox.IsEnabled = false;
                OpenInboxFolderButton.IsEnabled = false;
                MozillaThunderbirdFtcMessagesLocalFolderTextBox.IsEnabled = false;
                OpenFtcFolderFolderButton.IsEnabled = false;
                MozillaThunderbirdFtcCompletedMessagesLocalFolderTextBox.IsEnabled = false;
                OpenFtcCompletedFolderFolderButton.IsEnabled = false;
            }
        }

        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            EMailUserNameTextBox.IsEnabled = true;
            EMailAddressTextBox.IsEnabled = true;
            EMailPasswordPasswordBox.IsEnabled = true;
            ServerNameOfIncomeMessageTextBox.IsEnabled = true;
            IncomeMessagesPortTextBox.IsEnabled = true;
            ServerNameOfOutcomeMessageTextBox.IsEnabled = true;
            OutcomeMessagesPortTextBox.IsEnabled = true;
            SecureConnectionOnServerListView.IsEnabled = true;
            UseNameAndPasswordCheckBox.IsEnabled = true;
            UserNameOnServerTextBox.IsEnabled = UseNameAndPasswordCheckBox.IsChecked == true;
            UserNameOnServerTextBox.IsEnabled = true;
            UseSecureAuthenticationCheckBox.IsEnabled = true;
            ShowEmailPasswordButton.IsEnabled = true;
            DatabaseDataSourceTextBox.IsEnabled = true;
            DatabaseFileInLocalFolderTextBox.IsEnabled = true;
            DatabaseInitialCatalogTextBox.IsEnabled = true;
            DatabaseIntegratedSecurityCheckBox.IsEnabled = true;
            DatabasePasswordPasswordBox.IsEnabled = true;
            DatabaseUserNameTextBox.IsEnabled = true;
            DatabasePasswordPasswordBox.IsEnabled = true;
            ShowDatabasePasswordButton.IsEnabled = true;
            OpenDatabaseFolderButton.IsEnabled = true;
            MozillaThunderbirdInboxLocalFolderTextBox.IsEnabled = true;
            MozillaThunderbirdFtcMessagesLocalFolderTextBox.IsEnabled = true;
            OpenFtcFolderFolderButton.IsEnabled = true;
            MozillaThunderbirdFtcCompletedMessagesLocalFolderTextBox.IsEnabled = true;
            OpenFtcCompletedFolderFolderButton.IsEnabled = true;
            OpenInboxFolderButton.IsEnabled = true;
        }

        private void ShowEmailPassword_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(EMailPasswordPasswordBox.Password))
            {
                EMailPasswordPasswordBox.Visibility = Visibility.Collapsed;
                EMailPasswordTextBox.Visibility = Visibility.Visible;
                EMailPasswordTextBox.Text = EMailPasswordPasswordBox.Password;
                EMailPasswordTextBox.Focus();
                var uriSource = new Uri(@"..\AppData\Images\OpenedEyeImage.png", UriKind.Relative);
                ShowEmailImageLogoImage.Source= new BitmapImage(uriSource);
                _showEmailPass = true;
                if (!_timer.IsEnabled) _timer.IsEnabled = true;
                _timer.Start();
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e) {
            if (_showEmailPass)
            {
                EMailPasswordTextBox.Visibility = Visibility.Collapsed;
                EMailPasswordPasswordBox.Visibility = Visibility.Visible;
                EMailPasswordPasswordBox.Focus();
                var uriSource = new Uri(@"..\AppData\Images\ClosedEyesImage.png", UriKind.Relative);
                ShowEmailImageLogoImage.Source = new BitmapImage(uriSource);
                _showEmailPass = false;
                _timer.Stop();
            }
            if (_showDatabasePass)
            {
                DatabasePasswordTextBox.Visibility = Visibility.Collapsed;
                DatabasePasswordPasswordBox.Visibility = Visibility.Visible;
                DatabasePasswordPasswordBox.Focus();
                var uriSource = new Uri(@"..\AppData\Images\ClosedEyesImage.png", UriKind.Relative);
                ShowDatabaseImageLogoImage.Source = new BitmapImage(uriSource);
                _showDatabasePass = false;
                _timer.Stop();
            }
            _timer.IsEnabled = false;
        }

        private void OpenDatabaseFolder_Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog()==true)
            {
                if(!String.IsNullOrWhiteSpace(dialog.FileName))
                    DatabaseFileInLocalFolderTextBox.Text = dialog.FileName;
            }
        }

        private void ShowDatabasePassword_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(DatabasePasswordPasswordBox.Password))
            {
                DatabasePasswordPasswordBox.Visibility = Visibility.Collapsed;
                DatabasePasswordTextBox.Visibility = Visibility.Visible;
                DatabasePasswordTextBox.Text = DatabasePasswordPasswordBox.Password;
                DatabasePasswordTextBox.Focus();
                var uriSource = new Uri(@"..\AppData\Images\OpenedEyeImage.png", UriKind.Relative);
                ShowDatabaseImageLogoImage.Source = new BitmapImage(uriSource);
                _showDatabasePass = true;
                if (!_timer.IsEnabled) _timer.IsEnabled = true;
                _timer.Start();
            }
        }

        private void OpenInboxFolder_Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                if (!String.IsNullOrWhiteSpace(dialog.FileName))
                    MozillaThunderbirdInboxLocalFolderTextBox.Text = dialog.FileName;
            }
        }

        //ToDo Fix More
        private void TestDatabaseConnection_Button_Click(object sender, RoutedEventArgs e)
        {
            SqlConnectionStringBuilder stringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = DatabaseDataSourceTextBox.Text
            };
            stringBuilder.AttachDBFilename = (String.IsNullOrWhiteSpace(DatabaseFileInLocalFolderTextBox.Text)) ? String.Empty : DatabaseFileInLocalFolderTextBox.Text;
            stringBuilder.InitialCatalog = (String.IsNullOrWhiteSpace(DatabaseInitialCatalogTextBox.Text)) ? String.Empty : DatabaseInitialCatalogTextBox.Text;
            stringBuilder.UserID = (String.IsNullOrWhiteSpace(DatabaseUserNameTextBox.Text)) ? String.Empty : DatabaseUserNameTextBox.Text;
            stringBuilder.Password = (String.IsNullOrWhiteSpace(DatabasePasswordPasswordBox.Password)) ? String.Empty : DatabasePasswordPasswordBox.Password;
            stringBuilder.IntegratedSecurity = DatabaseIntegratedSecurityCheckBox.IsChecked == true;

            CallCenterContext context = new CallCenterContext(stringBuilder.ToString());

            try
            {
                context.Database.Connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.HelpLink, "ERROR in connection with database", MessageBoxButton.OK, MessageBoxImage.Error);
                Configuration cfg = new Configuration();
                cfg.TargetDatabase = new DbConnectionInfo(stringBuilder.ToString(), "System.Data.SqlClient");
                DbMigrator db = new DbMigrator(cfg);
                db.Update();
            }

            if (context.Database.Connection.State == ConnectionState.Open) MessageBox.Show("Connection with Database is SUCCESS!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            else MessageBox.Show("Connection with Database is FAILED!!!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            try
            {
                context.Database.Connection.Close();
            }
            catch
            {
                // ignored
            }
        }

        private void UseNameAndPassword_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UserNameOnServerTextBox.IsEnabled = true;
        }

        private void UseNameAndPassword_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UserNameOnServerTextBox.IsEnabled = false;
        }

        private void SecureConnectionOnServer_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void OpenFtcCompletedFolderFolder_Button_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                if (!String.IsNullOrWhiteSpace(dialog.FileName))
                   MozillaThunderbirdFtcCompletedMessagesLocalFolderTextBox.Text = dialog.FileName;
            }
        }

        private void OpenFtcFolderFolder_Button_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                if (!String.IsNullOrWhiteSpace(dialog.FileName))
                    MozillaThunderbirdFtcMessagesLocalFolderTextBox.Text = dialog.FileName;
            }
        }

        private void AddMailFilter_Button_OnClick(object sender, RoutedEventArgs e)
        {
            MailFilters filters=new MailFilters(this, _mainWindow, (new SqlConnectionStringBuilder
            {
                DataSource = _userSettings.DatabaseSetting.DatabaseDataSource,
                AttachDBFilename = _userSettings.DatabaseSetting.DatabaseLocalPathString,
                InitialCatalog = _userSettings.DatabaseSetting.DatabaseInitialCatalog,
                UserID = _userSettings.DatabaseSetting.DatabaseUserName,
                Password = _userSettings.DatabaseSetting.DatabasePassword,
                IntegratedSecurity = _userSettings.DatabaseSetting.DatabaseIntegratedSecurity
            }).ToString());
            _mainWindow.MainWindowFrameFrame.Navigate(filters);
        }
    }
}
