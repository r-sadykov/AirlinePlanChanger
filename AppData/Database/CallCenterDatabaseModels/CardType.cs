using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels
{
    internal class CardType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CardTypeId { get; set; }
        [DataType(DataType.Text)]
        [Display(Name="Card Type")]
        [MaxLength(25)]
        [Required]
        public string Name { get; set; }
    }
}
