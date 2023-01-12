using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels
{
    public class MailFilter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MailFiltersId { get; set; }
        public string MailAddress { get; set; }
        public string MailThemes { get; set; }
    }
}
