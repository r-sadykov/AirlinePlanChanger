using System;
using System.Collections.Generic;

namespace AirlinePlanChanges_MailCenter.Model
{
    internal class GdsMailInfo
    {
        public string Pnr { get; set; }
        public List<RouteInfoChanges> RouteChanges { get; set; }

        public GdsMailInfo()
        {
            RouteChanges=new List<RouteInfoChanges>();
        }

        public static string GetStatus(string str)
        {
            if (str.IndexOf("HK", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "OK";
            }

            if(str.IndexOf("UN", StringComparison.OrdinalIgnoreCase) >= 0 || str.IndexOf("UC", StringComparison.OrdinalIgnoreCase) >= 0 || str.IndexOf("HX", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "CANCELLED";
            }

            if(str.IndexOf("TK", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "CHANGED";
            }

            return "NO DATA";
        }
    }

    internal class RouteInfoChanges
    {
        public string AirlineCode { get; set; }
        public string AirlineNumber { get; set; }
        public string AirlineClass { get; set; }
        public string DepartureDate { get; set; }
        public int PassengersAmount { get; set; }
        public string DepartureRoute { get; set; }
        public string Status { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalTime { get; set; }
    }
}
