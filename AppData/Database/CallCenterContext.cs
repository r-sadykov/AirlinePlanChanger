using System.Data.Entity;
using AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels;

namespace AirlinePlanChanges_MailCenter.AppData.Database
{
    internal class CallCenterContext : DbContext
    {
        public CallCenterContext() : base("name = CallCenterContext") { }
        public CallCenterContext(string connectionString) : base(connectionString) { }

        public virtual DbSet<Test> Tests { get; set; }
        public virtual DbSet<ClearingType> ClearingTypes { get; set; }
        public virtual DbSet<CardType> CardTypes { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<Route> Routes { get; set; }
        public virtual DbSet<FlightLeg> FlightLegs { get; set; }
        public virtual DbSet<Airline> Airlines { get; set; }
        public virtual DbSet<Gds> Gdses { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Agency> Agencies { get; set; }
        public virtual DbSet<Passenger> Passengers { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<FullReport> FullReports { get; set; }
        public virtual DbSet<MailFilter> MailFilterses { get; set; }
    }

    public class Test
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
