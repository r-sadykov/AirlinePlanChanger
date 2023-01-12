using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AirlinePlanChanges_MailCenter.AppData.Database;
using AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels;
using AirlinePlanChanges_MailCenter.Model;
using AirlinePlanChanges_MailCenter.ViewModel;
using MimeKit;

namespace AirlinePlanChanges_MailCenter.View
{
    /// <summary>
    /// Логика взаимодействия для MailAddDataPage.xaml
    /// </summary>
    public partial class MailAddDataPage
    {
        private readonly int _index;
        private readonly MailListToSendViewModel _mailList;
        private readonly string _connection;
        private readonly MainPage _mainPage;

        public MailAddDataPage()
        {
            InitializeComponent();
        }

        public MailAddDataPage(int index, ref MailListToSendViewModel mailList)
        {
            InitializeComponent();
            _index = index;
            _mailList = mailList;
            PnrTextBlock.Text = mailList.MailToSendCollection[index].Item2.Pnr;
            EMailAddressTextBox.Text = mailList.MailToSendCollection[index].Item2.CustomerMail;
            ReferenznummerTextBlock.Text = mailList.MailToSendCollection[index].Item2.BookingNumber.ToString();
        }

        public MailAddDataPage(MainPage page, int index, CurrentUserSettings userSettings, ref MailListToSendViewModel mailListToSendView)
        {
            InitializeComponent();
            _index = index;
            _mailList = mailListToSendView;
            PnrTextBlock.Text = _mailList.MailToSendCollection[index].Item2.Pnr;
            EMailAddressTextBox.Text = _mailList.MailToSendCollection[index].Item2.CustomerMail;
            ReferenznummerTextBlock.Text = _mailList.MailToSendCollection[index].Item2.BookingNumber.ToString();
            _connection= (new SqlConnectionStringBuilder
            {
                DataSource = userSettings.DatabaseSetting.DatabaseDataSource,
                AttachDBFilename = userSettings.DatabaseSetting.DatabaseLocalPathString,
                InitialCatalog = userSettings.DatabaseSetting.DatabaseInitialCatalog,
                UserID = userSettings.DatabaseSetting.DatabaseUserName,
                Password = userSettings.DatabaseSetting.DatabasePassword,
                IntegratedSecurity = userSettings.DatabaseSetting.DatabaseIntegratedSecurity
            }).ToString();
            _mainPage = page;
        }

        private void ApplyMailChanges_Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(EMailAddressTextBox.Text)&&String.IsNullOrWhiteSpace(ReferenznummerTextBlock.Text))
            {
                if (String.IsNullOrWhiteSpace(EMailAddressTextBox.Text))
                {
                    EMailAddressTextBox.Text = "Please enter customer mail address";
                    EMailAddressTextBox.Foreground = Brushes.Red;
                    EMailAddressTextBox.FontWeight=FontWeights.Bold;
                }
                if (String.IsNullOrWhiteSpace(ReferenznummerTextBlock.Text))
                {
                    EMailAddressTextBox.Text = "Please enter customer booking number";
                    EMailAddressTextBox.Foreground = Brushes.Red;
                    EMailAddressTextBox.FontWeight = FontWeights.Bold;
                }
            } else if (!EMailAddressTextBox.Text.Contains("Please enter")||!ReferenznummerTextBlock.Text.Contains("Please enter"))
            {
                _mailList.MailToSendCollection[_index].Item1.To.Add(new MailboxAddress(EMailAddressTextBox.Text));
                _mailList.MailToSendCollection[_index].Item2.CustomerMail = EMailAddressTextBox.Text;
                try
                {
                    _mailList.MailToSendCollection[_index].Item2.BookingNumber =
                        Int32.Parse(ReferenznummerTextBlock.Text);
                    if (_mailList.MailToSendCollection[_index].Item1.Subject.Contains("Referenznummer"))
                    {
                        string replace = _mailList.MailToSendCollection[_index].Item1.Subject
                            .Replace("0", ReferenznummerTextBlock.Text);
                        _mailList.MailToSendCollection[_index].Item1.Subject = replace;
                        CallCenterContext context=new CallCenterContext(_connection);
                        try
                        {
                            string pnr = _mailList.MailToSendCollection[_index].Item2.Pnr;
                            var result = context.FullReports.FirstOrDefault(c =>
                                  c.FirstGdsBookingNumber == pnr);
                            if (result != null)
                            {
                                if (String.IsNullOrWhiteSpace(result.CustomerEmail))
                                {
                                    result.CustomerEmail = EMailAddressTextBox.Text;
                                }
                                if (!(result.BookingNumber > 0))
                                {
                                    result.BookingNumber = Int32.Parse(ReferenznummerTextBlock.Text);
                                }
                                context.Entry(result).State = EntityState.Modified;
                                context.SaveChanges();
                            }
                            else
                            {
                                FullReport report = new FullReport();
                                report.BookingNumber= Int32.Parse(ReferenznummerTextBlock.Text);
                                report.CustomerEmail= EMailAddressTextBox.Text.Replace(" ","").Replace(",","");
                                report.FirstGdsBookingNumber = pnr;
                                report.FirstGdsBookingAlias = pnr;
                                report.ReturnDate = DateTime.Now;
                                report.DepartureDate = DateTime.Now;
                                report.BookingDate = DateTime.Now;
                                context.FullReports.Add(report);
                                context.Entry(report).State = EntityState.Added;
                                context.SaveChanges();
                            }
                        } catch(Exception ex)
                        {
                            MessageBox.Show("There are no PNR in Database", "Error!", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                            Console.WriteLine(ex.Message);
                        }

                    }
                    _mainPage.MailsListBoxListBox.Items.Refresh();
                    var success = _mailList.MailToSendCollection.Count(t => t.Item2.IsChecked);
                    var failed = _mailList.MailToSendCollection.Count(t => t?.Item2.IsChecked == false);
                    _mainPage.MessagesToSendLabel.Content = success;
                    _mainPage.MessagesWithRedStatusLabel.Content = failed;
                    _mainPage.AdditionalMailInformationFrame.Navigate(null);
                    _mainPage.MailsDetailInformationFrame.Navigate(null);
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Wrong format for booking number", "Error!", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Console.Error.WriteLine(exc.ToString());
                }
            }
            else
            {
            }
        }
    }
}
