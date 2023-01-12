using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels
{
    internal class FullReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int BookingNumber { get; set; }
        public string Status { get; set; }
        public string SystemAgency { get; set; }
        public string DatevAgencyAccount { get; set; }
        public string Gds { get; set; }
        public string PassengerNames { get; set; }
        public int PassengerCount { get; set; }
        public string FirstAirline { get; set; }
        public string Ticket { get; set; }
        public string FirstGdsBookingNumber { get; set; }
        public string FirstGdsBookingAlias { get; set; }
        public DateTime BookingDate { get; set; }
        public string FirstRoute { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal Tariff { get; set; }
        public decimal Tax { get; set; }
        public decimal FullScFlex { get; set; }
        public decimal BloPartScFlex { get; set; }
        public decimal PartnerPartScFlex { get; set; }
        public decimal BloFixSc { get; set; }
        public decimal PartnerFixSc { get; set; }
        public decimal TotalPrice { get; set; }
        public string SellingCurrency { get; set; }
        public decimal ExchangeRateToEuro { get; set; }
        public string PaymentMethod { get; set; }
        public string SalesPoint { get; set; }
        public string Agent { get; set; }
        public string CardType { get; set; }
        public string CardHolder { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerGender { get; set; }
        public string CustomerCountry { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        [NotMapped]
        public string CustomerFullName { get { return CustomerFirstName + " " + CustomerLastName; } }
        public int NumberOfSegments { get; set; }
        public string ClearingType { get; set; }
        public string BookingClass { get; set; }
        public string FareBasis { get; set; }
        public string Commission { get; set; }
    }
}
