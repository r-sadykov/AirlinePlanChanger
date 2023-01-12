using System;

namespace AirlinePlanChanges_MailCenter.Model
{
    [Serializable]
    public class DatabaseSettings
    {
        public string DatabaseLocalPathString { get; set; }
        public string DatabaseInitialCatalog { get; set; }
        public string DatabaseUserName { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseDataSource { get; set; }
        public bool DatabaseIntegratedSecurity { get; set; }
        public bool IsFullAvailable { get; set; }
    }
}
