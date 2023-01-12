using System;

namespace AirlinePlanChanges_MailCenter.Model
{
    [Serializable]
    public class EmailSettings
    {
        public string EmailUserName { get; set; }
        public string EmailAddress { get; set; }
        public string EmailPassword { get; set; }
        public string ServerNameOfIncomeMessages { get; set; }
        public int ServerPortOfIncomeMessages { get; set; }
        public string ServerNameOfOutcomeMessages { get; set; }
        public int ServerPortOfOutcomeMessages { get; set; }
        public ServerMailConnectionParameters ServerSecureConnectionParameters { get; set; }
        public bool? UseNameAndPassword { get; set; }
        public string UsedNameForEmailConnection { get; set; }
        public bool? UseSecureAuthentication { get; set; }
        public bool IsFullAvailable { get; set; }
    }
}
