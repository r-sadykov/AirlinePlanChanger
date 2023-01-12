using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels
{
    internal class Passenger
    {
        public Passenger()
        {
            Orders = new HashSet<Order>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PassengerId { get; set; }
        [Display(Name ="Card Holder")]
        public string CardHolder { get; set; }
        [Display(Name = "First Name")]
        [Required]
        public string FirstName { get; set; }
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }
        [Display(Name = "Last Name")]
        [Required]
        public string LastName { get; set; }
        [Display(Name = "Full Name")]
        [NotMapped]
        public string FullName { get {return FirstName + " " + MiddleName + " " + LastName; } }
        [Display(Name = "Bithdate")]
        public string Birthdate { get; set; }
        [Display(Name = "Gender")]
        public string Gender { get; set; }
        [Display(Name = "Compellation")]
        public string Compellation { get; set; }
        [Display(Name ="Ticket issued")]
        public string TicketNumber { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
