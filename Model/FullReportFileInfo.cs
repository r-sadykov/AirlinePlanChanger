using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AirlinePlanChanges_MailCenter.Model
{
    [Serializable]
    internal class FullReportFileInfo : IDisposable
    {
        public DateTime FileUpload { get; set; }
        public DateTime FileModification { get; set; }
        public DateTime BookingDate { get; set; }
        public string Pnr { get; set; }

        public void Dispose() {
            string currentApplicationDirectory = Directory.GetCurrentDirectory();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "frimp.dat");
            if (!File.Exists(path)) Directory.CreateDirectory(Path.Combine(currentApplicationDirectory, "Data"));
            BinaryFormatter formatter = new BinaryFormatter();
            using (Stream stream = File.OpenWrite(path))
            {
                formatter.Serialize(stream, this);
            }
        }
    }
}
