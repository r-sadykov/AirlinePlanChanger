using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AirlinePlanChanges_MailCenter.Model
{
    [Serializable]
    internal class FullReportStatistics:IDisposable
    {
        public DateTime LastMonth { get; set; }
        public List<TopResults> GdsTopResultses { get; set; }
        public List<TopResults> VendorTopResultses { get; set; }
        public List<TopResults> AgenciesTopResultses { get; set; }
        public List<TopResults> CommonTopResultses { get; set; }

        public FullReportStatistics()
        {
            GdsTopResultses=new List<TopResults>();
            VendorTopResultses= new List<TopResults>();
            AgenciesTopResultses=new List<TopResults>();
            CommonTopResultses=new List<TopResults>();
        }

        #region Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            string currentApplicationDirectory = Directory.GetCurrentDirectory();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "frrs.dat");
            if (!File.Exists(path)) Directory.CreateDirectory(Path.Combine(currentApplicationDirectory, "Data"));
            BinaryFormatter formatter = new BinaryFormatter();
            using (Stream stream = File.OpenWrite(path))
            {
                formatter.Serialize(stream, this);
            }
        }

        #endregion
    }

    [Serializable]
    internal class TopResults
    {
        public string Name { get; set; }
        public decimal FinanceResult { get; set; }
        public int SegmentsResult { get; set; }
    }
}
