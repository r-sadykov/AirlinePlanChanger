using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels
{
    internal class Gds
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GdsId { get; set; }
        [Display(Name="GDS Channel")]
        [Required]
        public string Channel { get; set; }
        [Display(Name="GDS")]
        public string Name { get; set; }
    }
}
