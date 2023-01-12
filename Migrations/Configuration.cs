using System.Data.Entity.Migrations;
using AirlinePlanChanges_MailCenter.AppData.Database;

namespace AirlinePlanChanges_MailCenter.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<CallCenterContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(CallCenterContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
