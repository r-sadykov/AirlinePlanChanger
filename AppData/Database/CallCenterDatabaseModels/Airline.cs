using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels
{
    internal class Airline
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AirlineId { get; set; }
        [Display(Name ="ICAO")]
        public string IcaoName { get; set; }
        [Display(Name ="IATA")]
        [Required]
        public string IataName { get; set; }
        [Display(Name ="CRT")]
        public string CrtName { get; set; }
        [Display(Name ="International Company Name")]
        public string InternationalName { get; set; }
        [Display(Name ="National Company Name")]
        public string NationalName { get; set; }
        [Display(Name ="Date of company creation")]
        public DateTime? CreationDate { get; set; }
        [Display(Name ="Date of stopping activities")]
        public DateTime? ClosingDate { get; set; }
        [Display(Name ="Airline logo")]
        public byte[] AirlineLogo { get; set; }
    }
}
