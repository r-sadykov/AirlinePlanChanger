using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using AirlinePlanChanges_MailCenter.AppData.Database;
using AirlinePlanChanges_MailCenter.Model;
using AirlinePlanChanges_MailCenter.Utils;

namespace AirlinePlanChanges_MailCenter.View
{
    /// <summary>
    /// Interaction logic for BookingStatisticsPage.xaml
    /// </summary>
    public partial class BookingStatisticsPage
    {
        private readonly CurrentUserSettings _userSettings;
        internal BookingStatisticsPage(CurrentUserSettings userSettings)
        {
            InitializeComponent();
            _userSettings = userSettings;
            Init();
        }

        private void Init()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "frrs.dat");
            BinaryFormatter formatter = new BinaryFormatter();
            FullReportStatistics statistics=null;
            if (File.Exists(path))
            {
                using (Stream stream = File.OpenRead(path))
                {
                    try
                    {
                        statistics = (FullReportStatistics) formatter.Deserialize(stream);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            else
            {
                CallCenterContext context=new CallCenterContext((new SqlConnectionStringBuilder
                {
                    DataSource = _userSettings.DatabaseSetting.DatabaseDataSource,
                    AttachDBFilename = _userSettings.DatabaseSetting.DatabaseLocalPathString,
                    InitialCatalog = _userSettings.DatabaseSetting.DatabaseInitialCatalog,
                    UserID = _userSettings.DatabaseSetting.DatabaseUserName,
                    Password = _userSettings.DatabaseSetting.DatabasePassword,
                    IntegratedSecurity = _userSettings.DatabaseSetting.DatabaseIntegratedSecurity
                }).ToString());

                    if (context.FullReports.Any())
                    {
                        FullReportStatisticsGatherer gatherer=new FullReportStatisticsGatherer((new SqlConnectionStringBuilder
                        {
                            DataSource = _userSettings.DatabaseSetting.DatabaseDataSource,
                            AttachDBFilename = _userSettings.DatabaseSetting.DatabaseLocalPathString,
                            InitialCatalog = _userSettings.DatabaseSetting.DatabaseInitialCatalog,
                            UserID = _userSettings.DatabaseSetting.DatabaseUserName,
                            Password = _userSettings.DatabaseSetting.DatabasePassword,
                            IntegratedSecurity = _userSettings.DatabaseSetting.DatabaseIntegratedSecurity
                        }).ToString());
                        using (FullReportStatistics st = gatherer.GetStatistics(DateTime.Now))
                        {
                            statistics = st;
                        }
                    }
                }

            if (statistics!=null)
            {
                if (statistics.LastMonth!=DateTime.Now.AddMonths(-1))
                {
                    FullReportStatisticsGatherer gatherer=new FullReportStatisticsGatherer((new SqlConnectionStringBuilder
                    {
                        DataSource = _userSettings.DatabaseSetting.DatabaseDataSource,
                        AttachDBFilename = _userSettings.DatabaseSetting.DatabaseLocalPathString,
                        InitialCatalog = _userSettings.DatabaseSetting.DatabaseInitialCatalog,
                        UserID = _userSettings.DatabaseSetting.DatabaseUserName,
                        Password = _userSettings.DatabaseSetting.DatabasePassword,
                        IntegratedSecurity = _userSettings.DatabaseSetting.DatabaseIntegratedSecurity
                    }).ToString());
                    using (FullReportStatistics st= gatherer.GetStatistics(DateTime.Now))
                    {
                        statistics = st;
                    }
                }
                LastMonthAndYearTextBlock.Text = statistics.LastMonth.ToString("MMMM, yyyy");
                TopSalesGdsNameTextBlock.Text = statistics.GdsTopResultses[0].Name + Environment.NewLine +
                                                statistics.GdsTopResultses[1].Name + Environment.NewLine +
                                                statistics.GdsTopResultses[2].Name;
                TopSalesGdsFinanceResultTextBlock.Text= statistics.GdsTopResultses[0].FinanceResult + Environment.NewLine +
                                                        statistics.GdsTopResultses[1].FinanceResult + Environment.NewLine +
                                                        statistics.GdsTopResultses[2].FinanceResult;
                TopSalesGdsSegmentsAmountTextBlock.Text= statistics.GdsTopResultses[0].SegmentsResult + Environment.NewLine +
                                                         statistics.GdsTopResultses[1].SegmentsResult + Environment.NewLine +
                                                         statistics.GdsTopResultses[2].SegmentsResult;
                TopSalesVendorNameTextBlock.Text = statistics.VendorTopResultses[0].Name + Environment.NewLine +
                                                statistics.VendorTopResultses[1].Name + Environment.NewLine +
                                                statistics.VendorTopResultses[2].Name;
                TopSalesVendorFinanceResultTextBlock.Text = statistics.VendorTopResultses[0].FinanceResult + Environment.NewLine +
                                                         statistics.VendorTopResultses[1].FinanceResult + Environment.NewLine +
                                                         statistics.VendorTopResultses[2].FinanceResult;
                TopSalesVendorSegmentsAmountTextBlock.Text = statistics.VendorTopResultses[0].SegmentsResult + Environment.NewLine +
                                                          statistics.VendorTopResultses[1].SegmentsResult + Environment.NewLine +
                                                          statistics.VendorTopResultses[2].SegmentsResult;
                TopSalesAgencyNameTextBlock.Text = statistics.AgenciesTopResultses[0].Name + Environment.NewLine +
                                                statistics.AgenciesTopResultses[1].Name + Environment.NewLine +
                                                statistics.AgenciesTopResultses[2].Name;
                TopSalesAgencyFinanceResultTextBlock.Text = statistics.AgenciesTopResultses[0].FinanceResult + Environment.NewLine +
                                                         statistics.AgenciesTopResultses[1].FinanceResult + Environment.NewLine +
                                                         statistics.AgenciesTopResultses[2].FinanceResult;
                TopSalesAgencySegmentsAmountTextBlock.Text = statistics.AgenciesTopResultses[0].SegmentsResult + Environment.NewLine +
                                                          statistics.AgenciesTopResultses[1].SegmentsResult + Environment.NewLine +
                                                          statistics.AgenciesTopResultses[2].SegmentsResult;
                TotalSalesInMonthFincanceResultTextBlock.Text =
                    statistics.CommonTopResultses[0].FinanceResult.ToString("C");
                TotalSalesInMonthSegmentsAmountTextBlock.Text =
                    statistics.CommonTopResultses[0].SegmentsResult.ToString();
                TotalSalesAtBeginningYearFinanceResultTextBlock.Text= statistics.CommonTopResultses[1].FinanceResult.ToString("C");
                TotalSalesAtBeginningYearSegmentsAmountTextBlock.Text= statistics.CommonTopResultses[1].SegmentsResult.ToString();
                BestMonthInSalesNameTextBlock.Text = statistics.CommonTopResultses[2].Name;
                BestMonthInSalesFinanceResultTextBlock.Text = statistics.CommonTopResultses[2].FinanceResult.ToString("C");
                BestMonthInSalesSegmentsAmountTextBlock.Text = statistics.CommonTopResultses[2].SegmentsResult.ToString();
            }
        }
    }
}
