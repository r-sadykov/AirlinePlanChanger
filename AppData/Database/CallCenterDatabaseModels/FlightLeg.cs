using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels
{
    internal class FlightLeg
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FlightLegId { get; set; }
        [Display(Name ="Departure Route")]
        [Required]
        public int RouteId { get; set; }
        [ForeignKey("RouteId")]
        public virtual Route OneRoute { get; set; }
        [Display(Name = "Departure Date")]
        [Required]
        public DateTime? DepartureDate { get; set; }
        [Display(Name ="Is Return")]
        public bool? IsReturn { get; set; }
        [Display(Name = "Returns Route")]
        public int? ReturnRouteId { get; set; }
        [ForeignKey("ReturnRouteId")]
        public virtual Route ReturnRoute { get; set; }
        [Display(Name ="Return Date")]
        public DateTime? ReturnDate { get; set; }
    }
}
