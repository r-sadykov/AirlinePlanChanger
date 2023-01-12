using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels
{
    internal class Route
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RouteId { get; set; }
        [DataType(DataType.Text)]
        [Display(Name = "Departure route")]
        [MaxLength(7)]
        [Required]
        public string Departure { get; set; }
        [DataType(DataType.Text)]
        [Display(Name = "Arrival route")]
        [MaxLength(7)]
        [Required]
        public string Arrival { get; set; }
    }
}
