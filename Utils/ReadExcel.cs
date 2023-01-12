using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using AirlinePlanChanges_MailCenter.AppData.Database;
using AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels;

namespace AirlinePlanChanges_MailCenter.Utils
{
    internal class ReadExcel
    {
        private readonly string _connection;
        private readonly bool _go;

        public ReadExcel(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                _go = false;
            }
            else
            {
                _connection = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path +
                              ";Extended Properties='Excel 12.0 Xml;HDR=YES;'";
                _go = true;
            }
        }

        public List<FullReport> Read()
        {
            if (!_go)
            {
                return null;
            }
            List<FullReport> fullReports = new List<FullReport>();

            using (OleDbConnection con = new OleDbConnection(_connection))
            using (CallCenterContext context = new CallCenterContext())
            {
                con.Open();
                OleDbCommand command = new OleDbCommand("select * from [full$]", con);
                List<int> bookingNumber;
                try
                {
                    DateTime condition = DateTime.Now.AddYears(-1);
                    bookingNumber = context.FullReports.Where(c => c.BookingDate >= condition)
                        .Select(c => c.BookingNumber).ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    bookingNumber = null;
                }
                using (OleDbDataReader dr = command.ExecuteReader())
                {
                    while (dr != null && dr.Read())
                    {
                        FullReport fullReport = new FullReport {BookingNumber = Int32.Parse(dr[0].ToString())};

                        if (bookingNumber?.Contains(fullReport.BookingNumber) == true)
                        {
                            continue;
                        }
                        fullReport.Status = dr[1].ToString().ToUpper();
                        fullReport.SystemAgency = dr[3].ToString();
                        fullReport.DatevAgencyAccount = dr[5].ToString();
                        fullReport.Gds = dr[6].ToString().ToUpper();
                        fullReport.PassengerNames = dr[7].ToString().ToUpper();
                        Int32.TryParse(dr[8].ToString(), out int tempInt);
                        fullReport.PassengerCount = tempInt;
                        fullReport.FirstAirline = dr[9].ToString();
                        fullReport.Ticket = dr[10].ToString();
                        fullReport.FirstGdsBookingNumber = dr[11].ToString();
                        fullReport.FirstGdsBookingAlias = dr[12].ToString();
                        string hour = dr[14].ToString().Substring(0, 2);
                        string minutes = dr[14].ToString().Substring(3, 2);
                        string day = dr[13].ToString().Substring(0, 2);
                        string month = dr[13].ToString().Substring(3, 2);
                        string year = dr[13].ToString().Substring(6);
                        fullReport.BookingDate = new DateTime(year: Int32.Parse(year), month: Int32.Parse(month), day: Int32.Parse(day), hour:Int32.Parse(hour), minute:Int32.Parse(minutes), second:0);
                        fullReport.FirstRoute = dr[15].ToString();
                        day = dr[16].ToString().Substring(0, 2);
                        month = dr[16].ToString().Substring(3, 2);
                        year = dr[16].ToString().Substring(6);
                        fullReport.DepartureDate = new DateTime(year: Int32.Parse(year), month: Int32.Parse(month), day: Int32.Parse(day));
                        if (!String.IsNullOrWhiteSpace(dr[17].ToString()))
                        {
                            day = dr[17].ToString().Substring(0, 2);
                            month = dr[17].ToString().Substring(3, 2);
                            year = dr[17].ToString().Substring(6);
                            fullReport.ReturnDate = new DateTime(year: Int32.Parse(year), month: Int32.Parse(month), day: Int32.Parse(day));
                        }
                        else
                        {
                            fullReport.ReturnDate=new DateTime(1900,1,1);
                        }
                        Decimal.TryParse(dr[28].ToString(), out decimal tempDec);
                        fullReport.Tariff =tempDec;
                        Decimal.TryParse(dr[29].ToString(), out tempDec);
                        fullReport.Tax = tempDec;
                        Decimal.TryParse(dr[30].ToString(), out tempDec);
                        fullReport.FullScFlex = tempDec;
                        Decimal.TryParse(dr[31].ToString(), out tempDec);
                        fullReport.BloPartScFlex = tempDec;
                        Decimal.TryParse(dr[32].ToString(), out tempDec);
                        fullReport.PartnerPartScFlex = tempDec;
                        Decimal.TryParse(dr[33].ToString(), out tempDec);
                        fullReport.BloFixSc = tempDec;
                        Decimal.TryParse(dr[34].ToString(), out tempDec);
                        fullReport.PartnerFixSc = tempDec;
                        Decimal.TryParse(dr[35].ToString(), out tempDec);
                        fullReport.TotalPrice = tempDec;
                        fullReport.SellingCurrency = dr[26].ToString();
                        Decimal.TryParse(dr[27].ToString(), out tempDec);
                        fullReport.ExchangeRateToEuro = tempDec;
                        fullReport.PaymentMethod = dr[36].ToString();
                        fullReport.SalesPoint = dr[37].ToString();
                        fullReport.Agent = dr[38].ToString().ToUpper();
                        fullReport.CardType = dr[39].ToString().ToUpper();
                        fullReport.CardHolder = dr[40].ToString().ToUpper();
                        fullReport.CustomerGender = dr[41].ToString().ToUpper();
                        fullReport.CustomerFirstName = dr[42].ToString().ToUpper();
                        fullReport.CustomerLastName = dr[43].ToString().ToUpper();
                        fullReport.CustomerCountry = dr[44].ToString().ToUpper();
                        fullReport.CustomerCity = dr[45].ToString().ToUpper();
                        fullReport.CustomerAddress = dr[46].ToString().ToUpper();
                        fullReport.CustomerEmail = dr[47].ToString().ToLower();
                        fullReport.CustomerPhone = dr[48].ToString();
                        Int32.TryParse(dr[50].ToString(), out tempInt);
                        fullReport.NumberOfSegments = tempInt;
                        fullReport.ClearingType = dr[66].ToString().ToUpper();
                        fullReport.BookingClass = dr[67].ToString().ToUpper();
                        fullReport.FareBasis = dr[68].ToString().ToUpper();
                        fullReport.Commission = dr[69].ToString();

                        fullReports.Add(fullReport);
                    }
                }
                con.Close();
            }
            return fullReports;
        }
    }
}
