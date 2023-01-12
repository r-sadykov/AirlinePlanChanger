using System.ComponentModel;

namespace AirlinePlanChanges_MailCenter.Model
{
    public enum ServerMailConnectionParameters
    {
        [Description("Nothing")]
        Nothing=0,
        [Description("SSL/TLS")]
        Ssltls=1,
        [Description("STARTTLS")]
        Starttls=2
    }
}
