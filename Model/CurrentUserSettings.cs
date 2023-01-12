using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace AirlinePlanChanges_MailCenter.Model
{
    [Serializable]
    public class CurrentUserSettings : IDisposable
    {
        public string OperatorName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? LastDateMessageLooked { get; set; }
        public string WindowsUserName { get; set; }
        public string WindowsMachineName { get; set; }

        public EmailSettings EmailSetting { get; set; }

        public DatabaseSettings DatabaseSetting { get; set; }

        public string MozillaThunderbirdInboxFilePath { get; set; }
        public string MozillaFtcFilePath { get; set; }
        public string MozillaFtcCompletedFilePath { get; set; }

        public bool HaveAllSettingsUp { get; set; }

        public CurrentUserSettings()
        {
            EmailSetting = new EmailSettings();
            DatabaseSetting = new DatabaseSettings();
        }

        public void Dispose()
        {
            string currentApplicationDirectory = Directory.GetCurrentDirectory();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "bcc.info");
            if (!File.Exists(path)) Directory.CreateDirectory(Path.Combine(currentApplicationDirectory, "Data"));
            BinaryFormatter formatter = new BinaryFormatter();
            using (Stream stream = File.OpenWrite(path))
            {
                StartDate = LastDateMessageLooked ?? EndDate;
                HaveAllSettingsUp = !String.IsNullOrWhiteSpace(OperatorName) && !String.IsNullOrWhiteSpace(MozillaThunderbirdInboxFilePath) && DatabaseSetting.IsFullAvailable && EmailSetting.IsFullAvailable;
                if (HaveAllSettingsUp)
                {
                    formatter.Serialize(stream, this);
                    return;
                }
            }
            File.Delete(path);
            string message = "";
            if (!DatabaseSetting.IsFullAvailable) message += "Database settings is not fully defined!" + Environment.NewLine;
            if (!EmailSetting.IsFullAvailable) message += "EMail settings is not fully defined!" + Environment.NewLine;
            if (String.IsNullOrWhiteSpace(OperatorName)) message += "Operator Name is not defined!" + Environment.NewLine;
            if (String.IsNullOrWhiteSpace(MozillaThunderbirdInboxFilePath)) message += "INBOX file not found!" + Environment.NewLine;
            if (!HaveAllSettingsUp) message += "Settings do not saved.";
            MessageBox.Show(message, "Warning!!!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}