using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using AirlinePlanChanges_MailCenter.AppData.Database;
using AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels;
using AirlinePlanChanges_MailCenter.Model;

namespace AirlinePlanChanges_MailCenter.View
{
    /// <summary>
    /// Interaction logic for BookingSearchPage.xaml
    /// </summary>
    public partial class BookingSearchPage
    {
        private readonly BookingsData _bookingsData;
        private readonly CurrentUserSettings _userSettings;
        internal BookingSearchPage(BookingsData bookingsData, CurrentUserSettings userSettings)
        {
            InitializeComponent();
            _bookingsData = bookingsData;
            _userSettings = userSettings;
            Init();
        }

        private void Init()
        {
            string searchWord = _bookingsData.SearchTextBlock.Text;
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
            CallCenterContext context=new CallCenterContext(stringBuilder.ToString());
            FullReport findedObject = null;
            if (_bookingsData.SeachByPnrCheckBox.IsChecked==true)
            {
                findedObject = context.FullReports.FirstOrDefault(c => c.FirstGdsBookingNumber == searchWord);
            }
            if (_bookingsData.SeachByCustomerCheckBox.IsChecked == true)
            {
                string firstName = searchWord.Substring(0, searchWord.IndexOf(' ')).ToUpper();
                string lastName=searchWord.Substring(searchWord.IndexOf(' ') + 1).ToUpper();
                findedObject = context.FullReports.FirstOrDefault(c => c.CustomerFirstName.Contains(firstName) && c.CustomerLastName.Contains(lastName));
            }
            if (_bookingsData.SeachByPassengerCheckBox.IsChecked == true)
            {
                findedObject = context.FullReports.FirstOrDefault(c => c.PassengerNames.Contains(searchWord.ToUpper()));
            }
            if (_bookingsData.SeachByReferenznummerCheckBox.IsChecked == true)
            {
                int searchInt = Int32.Parse(searchWord);
                findedObject = context.FullReports.FirstOrDefault(c => c.BookingNumber == searchInt);
            }

            if (findedObject != null)
            {
                RereferenznummerTextBox.Text = findedObject.BookingNumber.ToString();
                StatusTextBox.Text = findedObject.Status;
                AgencyTextBox.Text = findedObject.SalesPoint;
                GdsTextBox.Text = findedObject.Gds;
                PassengersTextBox.Text = findedObject.PassengerNames;
                PassenegersAmountTextBox.Text = findedObject.PassengerCount.ToString();
                VendorTextBox.Text = findedObject.FirstAirline;
                TicketNumberTextBox.Text = findedObject.Ticket;
                BookingGdsNumberTextBox.Text = findedObject.FirstGdsBookingNumber;
                BookingGdsAliasTextBox.Text = findedObject.FirstGdsBookingAlias;
                BookingDateTextBox.Text = findedObject.BookingDate.ToString("D");
                RouteTextBox.Text = findedObject.FirstRoute;
                NumberOfSegmentsTextBox.Text = findedObject.NumberOfSegments.ToString();
                ClearingTypeTextBox.Text = findedObject.ClearingType;
                ExchangeRatioTextBox.Text = findedObject.ExchangeRateToEuro.ToString(CultureInfo.InvariantCulture);
                BookingClassTextBox.Text = findedObject.BookingClass;
                FareBasisTextBox.Text = findedObject.FareBasis;
                CustomerTextBox.Text = findedObject.CustomerFirstName + " " + findedObject.CustomerLastName;
                PaymentMethodTextBox.Text = findedObject.PaymentMethod;
                CardTypeTextBox.Text = findedObject.CardType;
                CardHolderTextBox.Text = findedObject.CardHolder;
                SellingCurrencyTextBox.Text = findedObject.SellingCurrency;
                TariffTextBox.Text = findedObject.Tariff.ToString("C");
                TaxTextBox.Text = findedObject.Tax.ToString("C");
                FullScFlexTextBox.Text = findedObject.FullScFlex.ToString("C");
                BloPartScFlexTextBox.Text = findedObject.BloPartScFlex.ToString("C");
                PartnerPartScFlexTextBox.Text = findedObject.PartnerPartScFlex.ToString("C");
                BloFixScTextBox.Text = findedObject.BloFixSc.ToString("C");
                PartnerFixScTextBox.Text = findedObject.PartnerFixSc.ToString("C");
                TotalPriceTextBox.Text = findedObject.TotalPrice.ToString("C");
                CommissionTextBox.Text = findedObject.Commission;
            }
        }
    }
}
