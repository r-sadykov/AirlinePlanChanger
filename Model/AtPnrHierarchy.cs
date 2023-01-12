using System.Collections.Generic;

namespace AirlinePlanChanges_MailCenter.Model
{
    class AtPnrHierarchy
    {
        public string Pnr { get; set; }
        public List<string> InfoRows { get; set; }

        public AtPnrHierarchy()
        {
            InfoRows = new List<string>();
        }
    }
}
