using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels
{
    internal class Agency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AgencyId { get; set; }
        [Display(Name="Agency Name")]
        public string Name { get; set; }
        [Display(Name = "Sales Point")]
        [Required]
        public string SalesPoint { get; set; }
        [Display(Name = "Agency number in IDEA")]
        [Required]
        public string SystemNumber { get; set; }
        [Display(Name = "Agency number in DATEV")]
        [Required]
        public string DatevNumber { get; set; }
    }
}
