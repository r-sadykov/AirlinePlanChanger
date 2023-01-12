using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels
{
    internal class ClearingType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClearingTypeId { get; set; }
        [DataType(DataType.Text, ErrorMessage ="Incorrect input type")]
        [Display(Name="Clearing Type")]
        [MaxLength(25)]
        [Required]
        public string Name { get; set; }
    }
}
