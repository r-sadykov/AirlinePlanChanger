using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using AirlinePlanChanges_MailCenter.AppData.Database;
using AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels;
using AirlinePlanChanges_MailCenter.Model;
using AirlinePlanChanges_MailCenter.Utils;
using Microsoft.Win32;

namespace AirlinePlanChanges_MailCenter.View
{
    /// <summary>
    /// Interaction logic for BookingsData.xaml
    /// </summary>
    public partial class BookingsData
    {
        private readonly CurrentUserSettings _userSettings;
        private readonly MainWindow _mainWindow;
        private string _fileFullReportExcel;
        private readonly FullReportFileInfo _fullReport;
        internal BookingsData(MainWindow mainWindow, CurrentUserSettings userSettings)
        {
            InitializeComponent();
            _userSettings = userSettings;
            _mainWindow = mainWindow;
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "frimp.dat");
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                if (File.Exists(path))
                {
                    using (Stream stream = File.OpenRead(path))
                    {
                        try
                        {
                            _fullReport = (FullReportFileInfo)formatter.Deserialize(stream);
                            Init();
                        }
                        catch
                        {
                            LastDateOfFileUploadTextBlock.Text = String.Empty;
                            LastDateOfFileModificationTextBlock.Text = String.Empty;
                            LastDateOfBookingLoadTextBlock.Text = String.Empty;
                            LastPnrAddedTextBlock.Text = String.Empty;
                        }
                    }
                }
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.HelpLink, "Error in loading Confirmation page", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private BookingsData(BookingsData data)
        {
            InitializeComponent();
            _userSettings = data._userSettings;
            _mainWindow = data._mainWindow;
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "frimp.dat");
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                if (File.Exists(path))
                {
                    using (Stream stream = File.OpenRead(path))
                    {
                        try
                        {
                            _fullReport = (FullReportFileInfo)formatter.Deserialize(stream);
                            Init();
                        }
                        catch
                        {
                            LastDateOfFileUploadTextBlock.Text = String.Empty;
                            LastDateOfFileModificationTextBlock.Text = String.Empty;
                            LastDateOfBookingLoadTextBlock.Text = String.Empty;
                            LastPnrAddedTextBlock.Text = String.Empty;
                        }
                    }
                }
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.HelpLink, "Error in loading Confirmation page", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Init() {
            LastDateOfFileUploadTextBlock.Text = _fullReport.FileUpload.ToString("dd MMMM yyyy, HH:mm");
            LastDateOfFileModificationTextBlock.Text = _fullReport.FileModification.ToString("dd MMMM yyyy, HH:mm");
            LastDateOfBookingLoadTextBlock.Text = _fullReport.BookingDate.ToString("dd MMMM yyyy, HH:mm");
            LastPnrAddedTextBlock.Text = _fullReport.Pnr;
        }

        private void BookingSearchButton_Button_Click(object sender, RoutedEventArgs e)
        {
            BookingSearchPage bookingSearchPage = new BookingSearchPage(this, _userSettings);
            BookingDetailsFrame.Navigate(bookingSearchPage);
        }

        private void UploadFullReportFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            
            if (dialog.ShowDialog() == true)
            {
                if (!String.IsNullOrWhiteSpace(dialog.FileName))
                    _fileFullReportExcel = dialog.FileName;
            }
            if (_fileFullReportExcel.Equals("") || _fileFullReportExcel == string.Empty) return;

            using (FullReportFileInfo fileInfo = new FullReportFileInfo())
            {
                ReadExcel readExcel = new ReadExcel(_fileFullReportExcel);
                List<FullReport> list=null;
                try
                {
                    list = readExcel.Read();
                } catch(Exception ex)
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + ex.HelpLink, "Error in reading Full Report from Excel file", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                    SqlConnectionStringBuilder stringBuilder =
                        new SqlConnectionStringBuilder
                        {
                            DataSource = _userSettings.DatabaseSetting.DatabaseDataSource,
                            AttachDBFilename = _userSettings.DatabaseSetting.DatabaseLocalPathString,
                            InitialCatalog = _userSettings.DatabaseSetting.DatabaseInitialCatalog,
                            UserID = _userSettings.DatabaseSetting.DatabaseUserName,
                            Password = _userSettings.DatabaseSetting.DatabasePassword,
                            IntegratedSecurity = _userSettings.DatabaseSetting.DatabaseIntegratedSecurity
                        };

                if (list != null && list.Count!=0)
                {
                    try
                    {
                        var stream = new FileInfo(_fileFullReportExcel);
                        fileInfo.FileUpload = DateTime.Now;
                        fileInfo.FileModification = stream.LastWriteTime;
                    } catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error in getting file info", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                AddToDatabase database=new AddToDatabase(stringBuilder.ToString());
                try
                {
                    if (database.AddToFullReport(list))
                    {
                        CallCenterContext context = new CallCenterContext(stringBuilder.ToString());
                        if (context.FullReports != null)
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            fileInfo.BookingDate = context.FullReports.OrderByDescending(c => c.Id).FirstOrDefault().BookingDate;
                            // ReSharper disable once PossibleNullReferenceException
                            fileInfo.Pnr = context.FullReports.OrderByDescending(c => c.Id).FirstOrDefault().FirstGdsBookingNumber;
                        }
                    }
                } catch(Exception ex)
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + ex.HelpLink, "Error in Database (main window)", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            BookingsData data=new BookingsData(this);
            _mainWindow.MainWindowFrameFrame.Navigate(data);
        }

        private void SeachByPnrCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //SeachByReferenznummerCheckBox.IsChecked = false;
            //SeachByCustomerCheckBox.IsChecked = false;
            //SeachByPassengerCheckBox.IsChecked = false;
        }

        private void SeachByCustomerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //SeachByReferenznummerCheckBox.IsChecked = false;
            //SeachByPnrCheckBox.IsChecked = false;
            //SeachByPassengerCheckBox.IsChecked = false;
        }

        private void SeachByReferenznummerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //SeachByPnrCheckBox.IsChecked = false;
            //SeachByCustomerCheckBox.IsChecked = false;
            //SeachByPassengerCheckBox.IsChecked = false;
        }

        private void SeachByPassengerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //SeachByReferenznummerCheckBox.IsChecked = false;
            //SeachByCustomerCheckBox.IsChecked = false;
            //SeachByPnrCheckBox.IsChecked = false;
        }
    }
}
