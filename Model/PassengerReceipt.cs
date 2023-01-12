namespace AirlinePlanChanges_MailCenter.Model
{
    public class PassengerReceipt
    {
        public string Pnr { get; set; }
        public string Passenger { get; set; }
        public string BuchungDate { get; set; }
        public string ReceivedDate { get; set; }
        public string ChangesDate { get; set; }
        public string SepaCode { get; set; }
        public string SepaDate { get; set; }
        public string SepaAmount { get; set; }
    }
}
