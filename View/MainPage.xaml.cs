using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AirlinePlanChanges_MailCenter.Model;
using AirlinePlanChanges_MailCenter.Utils;
using AirlinePlanChanges_MailCenter.ViewModel;
using Microsoft.Win32;
using MimeKit;

namespace AirlinePlanChanges_MailCenter.View
{

    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage
    {
        private readonly CurrentUserSettings _userSettings;
        private readonly MainWindow _mainWindow;
        private MailListToSendViewModel _mailListToSendView;
        private MailListToSendViewModel MailList => _mailListToSendView;
        private Dictionary<string, List<MimeMessage>> _messagesToLookup;

        public MainPage()
        {
            _mailListToSendView = new MailListToSendViewModel();
            InitializeComponent();
            DataContext = MailList.MailToSendCollection;
        }


        internal MainPage(MainWindow mainWindow, ref CurrentUserSettings currentUser)
        {
            _mailListToSendView = new MailListToSendViewModel();
            InitializeComponent();
            _userSettings = currentUser;
            _mainWindow = mainWindow;

        }

        internal MainPage(MainWindow mainWindow, CurrentUserSettings currentUser)
        {
            _mailListToSendView = new MailListToSendViewModel();
            InitializeComponent();
            _userSettings = currentUser;
            _mainWindow = mainWindow;
            MessagesToSendLabel.Content = MailList.MailToSendCollection.Count(c => c.Item2.IsSelected == true);
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void UpdateMailListBox_Button_Click(object sender, RoutedEventArgs e)
        {
            //from 02/03/2018 trying to get mails from imap server
            //Update();
            UpdateFromImap();
            MailsListBoxListBox.ItemsSource = MailList.MailToSendCollection;
            TotalMessagesInGivenPeriodLabel.Content = MailList.MailToSendCollection.Count.ToString();
            var success = MailList.MailToSendCollection.Count(t => t.Item2.IsChecked);
            var failed = MailList.MailToSendCollection.Count(t => !t.Item2.IsChecked);
            MessagesToSendLabel.Content = success;
            MessagesWithRedStatusLabel.Content = failed;
        }

        private void UpdateFromImap()
        {
            ThunderbirdMailGatherer mailGatherer = new ThunderbirdMailGatherer(_userSettings);
            Tuple<List<MimeMessage>, int> mails = null;
            try
            {
                mails = mailGatherer.GetMailsFromImap(_userSettings.MozillaFtcFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.HelpLink, "Error in loading gathering mails from thunderbird folder", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                Progress<int> progress = new Progress<int>(result =>
                {
                    ApplicationProgressProgressBar.Value = result;
                });

                _messagesToLookup = mailGatherer.GetMails(_mainWindow.BeginningSearchDateDatePicker.SelectedDate, _mainWindow.EndSearchDateDatePicker.SelectedDate, this, progress, mails?.Item1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.HelpLink, "Error in loading gathering mails to system", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            try
            {
                Progress<int> progress = new Progress<int>(result =>
                {
                    ApplicationProgressProgressBar.Value = result;
                });
                _mailListToSendView.MailToSendCollection = mailGatherer.SetMessagesForSend(_messagesToLookup, this, progress);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.HelpLink, "Error in setting mails to send", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendMails_Button_Click(object sender, RoutedEventArgs e)
        {
            ThunderbirdMailGatherer mailGatherer = new ThunderbirdMailGatherer(_userSettings);
            var messagesToSend = MailList.MailToSendCollection.Where(c => c.Item2.IsSelected == true).ToList();
            string report;
            if (_messagesToLookup != null)
            {
                report = mailGatherer.Send(this, _userSettings, messagesToSend, _messagesToLookup);
            }
            else
            {
                report = mailGatherer.Send(this, _userSettings, messagesToSend);
            }
            try
            {
                MailDetailPage detailPage = new MailDetailPage(report);
                MailsDetailInformationFrame.Navigate(detailPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.HelpLink, "Reporting", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MailsListBox_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (Tuple<MimeMessage, MessageToSendInfoModel>)MailsListBoxListBox.SelectedItem;
            int index = MailList.MailToSendCollection.IndexOf(item);
            if (!item.Item2.IsChecked)
            {
                MailAddDataPage addDataPage = new MailAddDataPage(this, index, _userSettings, ref _mailListToSendView);
                MailDetailPage detailPage = new MailDetailPage(item);
                AdditionalMailInformationFrame.Navigate(addDataPage);
                MailsDetailInformationFrame.Navigate(detailPage);
            }
            else
            {
                MailDetailPage detailPage = new MailDetailPage(item);
                MailsDetailInformationFrame.Navigate(detailPage);
                AdditionalMailInformationFrame.Navigate(null);
            }
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            MessagesToSendLabel.Content = MailList.MailToSendCollection.Count(c => c.Item2.IsSelected == true);
        }

        private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            MessagesToSendLabel.Content = MailList.MailToSendCollection.Count(c => c.Item2.IsSelected == true);
        }

        private void UpdateFromFile_Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            string file = string.Empty;
            if (dialog.ShowDialog() == true)
            {
                file = dialog.FileName;
            }
            if (String.IsNullOrWhiteSpace(file)) return;
            List<AtPnrHierarchy> text = new List<AtPnrHierarchy>();
            using (var reader = new StreamReader(file))
            {
                AtPnrHierarchy atPnr = null;
                bool flag = false;
                string str;
                while (true)
                {
                    str = reader.ReadLine()?.Replace(" ", "").Replace(".", "").Replace(",", "").Replace("#", "").Replace("*", "");
                    if (!String.IsNullOrWhiteSpace(str))
                    {
                        if (str.Length == 6)
                        {
                            if (flag) text.Add(atPnr);
                            flag = true;
                            atPnr = new AtPnrHierarchy
                            {
                                Pnr = str
                            };
                        }
                        else if (str.Equals("EOF"))
                        {
                            text.Add(atPnr);
                            break;
                        }
                        // ReSharper disable once RedundantJumpStatement
                        else if (str.Contains("\\") || str.Contains("/")) continue;
                        else
                        {
                            if (atPnr != null)
                            {
                                atPnr.InfoRows.Add(str);
                            }
                        }
                    }
                }
            }
            ThunderbirdMailGatherer gatherer = new ThunderbirdMailGatherer(_userSettings);
            _mailListToSendView.MailToSendCollection = gatherer.PrepareMailsFromAtFromFile(text);
            MailsListBoxListBox.ItemsSource = MailList.MailToSendCollection;
            TotalMessagesInGivenPeriodLabel.Content = MailList.MailToSendCollection.Count.ToString();
            var success = MailList.MailToSendCollection.Count(t => t.Item2.IsChecked);
            var failed = MailList.MailToSendCollection.Count(t => !t.Item2.IsChecked);
            MessagesToSendLabel.Content = success;
            MessagesWithRedStatusLabel.Content = failed;
        }
    }
}
