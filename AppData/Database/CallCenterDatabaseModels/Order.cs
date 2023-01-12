using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels
{
    internal sealed class Order
    {
        public Order() {
            Passengers = new HashSet<Passenger>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }
        [Display(Name ="Referenznummer")]
        [Required]
        public int BookingNumber { get; set; }
        [Display(Name ="Status")]
        public string Status { get; set; }
        [Display(Name = "Agency")]
        public Agency Agency { get; set; }
        [Display(Name = "GDS")]
        public Gds Gds { get; set; }
        [Display(Name ="Amount of passengers")]
        public int PassengerAmount { get; set; }
        [Display(Name = "Airline")]
        public Airline Airline { get; set; }
        [Display(Name = "First GDS Booking ID")]
        public string FirstGdsBookingNumber { get; set; }
        [Display(Name = "GDS Alternative Booking ID")]
        public string FirstGdsBookingAlias { get; set; }
        [Display(Name = "GDS Optional Booking ID")]
        public string SecondGdsBookingAlias { get; set; }
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; }
        [Display(Name = "Route")]
        public FlightLeg FlightLeg { get; set; }
        [Display(Name = "Airline Ticket Tariff")]
        public decimal Tariff { get; set; }
        [Display(Name = "Airline Ticket Tax")]
        public decimal Tax { get; set; }
        [Display(Name = "Flex Service Charges")]
        public decimal FullScFlex { get; set; }
        [Display(Name = "where BERlogic part is")]
        public decimal BloPartScFlex { get; set; }
        [Display(Name = "BERlogic Fix Fee")]
        public decimal BloFixSc { get; set; }
        [Display(Name = "Partner Fix Fee")]
        public decimal PartnerFixSc { get; set; }
        [Display(Name = "Ticket Total Price")]
        public decimal TotalPrice { get; set; }
        [Display(Name = "Selling Currency")]
        public Currency SellingCurrency { get; set; }
        [Display(Name = "Exchange Ratio RUB/EUR")]
        public decimal ExchangeRateToEuro { get; set; }
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }
        [Display(Name = "Card Type")]
        public CardType CardType { get; set; }
        [Display(Name = "Customer")]
        public Customer Customer { get; set; }
        [Display(Name = "Number of Segments")]
        public int NumberOfSegments { get; set; }
        [Display(Name = "Clearing Type")]
        public ClearingType ClearingType { get; set; }
        [Display(Name = "Booking Class")]
        public char BookingClass { get; set; }
        [Display(Name = "Fare Basis")]
        public string FareBasis { get; set; }
        [Display(Name = "Commission")]
        public decimal CommissionSize { get; set; }
        [Display(Name = "Percentage/Absolute")]
        public char CommisionBase { get; set; }
        [Display(Name = "Currency")]
        public Currency CommissionCurrency { get; set; }

        public ICollection<Passenger> Passengers { get; set; }
    }
}
