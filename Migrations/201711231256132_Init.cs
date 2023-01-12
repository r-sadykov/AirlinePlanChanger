using System.Data.Entity.Migrations;

namespace AirlinePlanChanges_MailCenter.Migrations
{
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Agencies",
                c => new
                    {
                        AgencyId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        SalesPoint = c.String(nullable: false),
                        SystemNumber = c.String(nullable: false),
                        DatevNumber = c.String(nullable: false)
                    })
                .PrimaryKey(t => t.AgencyId);
            
            CreateTable(
                "dbo.Airlines",
                c => new
                    {
                        AirlineId = c.Int(nullable: false, identity: true),
                        IcaoName = c.String(),
                        IataName = c.String(nullable: false),
                        CrtName = c.String(),
                        InternationalName = c.String(),
                        NationalName = c.String(),
                        CreationDate = c.DateTime(),
                        ClosingDate = c.DateTime(),
                        AirlineLogo = c.Binary()
                    })
                .PrimaryKey(t => t.AirlineId);
            
            CreateTable(
                "dbo.CardTypes",
                c => new
                    {
                        CardTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 25)
                    })
                .PrimaryKey(t => t.CardTypeId);
            
            CreateTable(
                "dbo.ClearingTypes",
                c => new
                    {
                        ClearingTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 25)
                    })
                .PrimaryKey(t => t.ClearingTypeId);
            
            CreateTable(
                "dbo.Currencies",
                c => new
                    {
                        CurrencyId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 3)
                    })
                .PrimaryKey(t => t.CurrencyId);
            
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        CustomerId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        Gender = c.String(),
                        Country = c.String(),
                        PostIndex = c.String(),
                        City = c.String(),
                        Address = c.String(),
                        Email = c.String(nullable: false),
                        Phone = c.String(nullable: false)
                    })
                .PrimaryKey(t => t.CustomerId);
            
            CreateTable(
                "dbo.FlightLegs",
                c => new
                    {
                        FlightLegId = c.Int(nullable: false, identity: true),
                        RouteId = c.Int(nullable: false),
                        DepartureDate = c.DateTime(nullable: false),
                        IsReturn = c.Boolean(),
                        ReturnRouteId = c.Int(),
                        ReturnDate = c.DateTime()
                    })
                .PrimaryKey(t => t.FlightLegId)
                .ForeignKey("dbo.Routes", t => t.RouteId, cascadeDelete: true)
                .ForeignKey("dbo.Routes", t => t.ReturnRouteId)
                .Index(t => t.RouteId)
                .Index(t => t.ReturnRouteId);
            
            CreateTable(
                "dbo.Routes",
                c => new
                    {
                        RouteId = c.Int(nullable: false, identity: true),
                        Departure = c.String(nullable: false, maxLength: 7),
                        Arrival = c.String(nullable: false, maxLength: 7)
                    })
                .PrimaryKey(t => t.RouteId);
            
            CreateTable(
                "dbo.FullReports",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BookingNumber = c.Int(nullable: false),
                        Status = c.String(),
                        SystemAgency = c.String(),
                        DatevAgencyAccount = c.String(),
                        Gds = c.String(),
                        PassengerNames = c.String(),
                        PassengerCount = c.Int(nullable: false),
                        FirstAirline = c.String(),
                        Ticket = c.String(),
                        FirstGdsBookingNumber = c.String(),
                        FirstGdsBookingAlias = c.String(),
                        BookingDate = c.DateTime(nullable: false),
                        FirstRoute = c.String(),
                        DepartureDate = c.DateTime(nullable: false),
                        ReturnDate = c.DateTime(nullable: false),
                        Tariff = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Tax = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FullScFlex = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BloPartScFlex = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PartnerPartScFlex = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BloFixSc = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PartnerFixSc = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SellingCurrency = c.String(),
                        ExchangeRateToEuro = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentMethod = c.String(),
                        SalesPoint = c.String(),
                        Agent = c.String(),
                        CardType = c.String(),
                        CardHolder = c.String(),
                        CustomerFirstName = c.String(),
                        CustomerLastName = c.String(),
                        CustomerGender = c.String(),
                        CustomerCountry = c.String(),
                        CustomerCity = c.String(),
                        CustomerAddress = c.String(),
                        CustomerEmail = c.String(),
                        CustomerPhone = c.String(),
                        NumberOfSegments = c.Int(nullable: false),
                        ClearingType = c.String(),
                        BookingClass = c.String(),
                        FareBasis = c.String(),
                        Commission = c.String()
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Gds",
                c => new
                    {
                        GdsId = c.Int(nullable: false, identity: true),
                        Channel = c.String(nullable: false),
                        Name = c.String()
                    })
                .PrimaryKey(t => t.GdsId);
            
            CreateTable(
                "dbo.MailFilters",
                c => new
                    {
                        MailFiltersId = c.Int(nullable: false, identity: true),
                        MailAddress = c.String(),
                        MailThemes = c.String()
                    })
                .PrimaryKey(t => t.MailFiltersId);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderId = c.Int(nullable: false, identity: true),
                        BookingNumber = c.Int(nullable: false),
                        Status = c.String(),
                        PassengerAmount = c.Int(nullable: false),
                        FirstGdsBookingNumber = c.String(),
                        FirstGdsBookingAlias = c.String(),
                        SecondGdsBookingAlias = c.String(),
                        BookingDate = c.DateTime(nullable: false),
                        Tariff = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Tax = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FullScFlex = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BloPartScFlex = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BloFixSc = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PartnerFixSc = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ExchangeRateToEuro = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NumberOfSegments = c.Int(nullable: false),
                        FareBasis = c.String(),
                        CommissionSize = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Agency_AgencyId = c.Int(),
                        Airline_AirlineId = c.Int(),
                        CardType_CardTypeId = c.Int(),
                        ClearingType_ClearingTypeId = c.Int(),
                        CommissionCurrency_CurrencyId = c.Int(),
                        Customer_CustomerId = c.Int(),
                        FlightLeg_FlightLegId = c.Int(),
                        Gds_GdsId = c.Int(),
                        PaymentMethod_PaymentMethodId = c.Int(),
                        SellingCurrency_CurrencyId = c.Int()
                    })
                .PrimaryKey(t => t.OrderId)
                .ForeignKey("dbo.Agencies", t => t.Agency_AgencyId)
                .ForeignKey("dbo.Airlines", t => t.Airline_AirlineId)
                .ForeignKey("dbo.CardTypes", t => t.CardType_CardTypeId)
                .ForeignKey("dbo.ClearingTypes", t => t.ClearingType_ClearingTypeId)
                .ForeignKey("dbo.Currencies", t => t.CommissionCurrency_CurrencyId)
                .ForeignKey("dbo.Customers", t => t.Customer_CustomerId)
                .ForeignKey("dbo.FlightLegs", t => t.FlightLeg_FlightLegId)
                .ForeignKey("dbo.Gds", t => t.Gds_GdsId)
                .ForeignKey("dbo.PaymentMethods", t => t.PaymentMethod_PaymentMethodId)
                .ForeignKey("dbo.Currencies", t => t.SellingCurrency_CurrencyId)
                .Index(t => t.Agency_AgencyId)
                .Index(t => t.Airline_AirlineId)
                .Index(t => t.CardType_CardTypeId)
                .Index(t => t.ClearingType_ClearingTypeId)
                .Index(t => t.CommissionCurrency_CurrencyId)
                .Index(t => t.Customer_CustomerId)
                .Index(t => t.FlightLeg_FlightLegId)
                .Index(t => t.Gds_GdsId)
                .Index(t => t.PaymentMethod_PaymentMethodId)
                .Index(t => t.SellingCurrency_CurrencyId);
            
            CreateTable(
                "dbo.Passengers",
                c => new
                    {
                        PassengerId = c.Int(nullable: false, identity: true),
                        CardHolder = c.String(),
                        FirstName = c.String(nullable: false),
                        MiddleName = c.String(),
                        LastName = c.String(nullable: false),
                        Birthdate = c.String(),
                        Gender = c.String(),
                        Compellation = c.String(),
                        TicketNumber = c.String()
                    })
                .PrimaryKey(t => t.PassengerId);
            
            CreateTable(
                "dbo.PaymentMethods",
                c => new
                    {
                        PaymentMethodId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 30)
                    })
                .PrimaryKey(t => t.PaymentMethodId);
            
            CreateTable(
                "dbo.Tests",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String()
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.PassengerOrders",
                c => new
                    {
                        Passenger_PassengerId = c.Int(nullable: false),
                        Order_OrderId = c.Int(nullable: false)
                    })
                .PrimaryKey(t => new { t.Passenger_PassengerId, t.Order_OrderId })
                .ForeignKey("dbo.Passengers", t => t.Passenger_PassengerId, cascadeDelete: true)
                .ForeignKey("dbo.Orders", t => t.Order_OrderId, cascadeDelete: true)
                .Index(t => t.Passenger_PassengerId)
                .Index(t => t.Order_OrderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Orders", "SellingCurrency_CurrencyId", "dbo.Currencies");
            DropForeignKey("dbo.Orders", "PaymentMethod_PaymentMethodId", "dbo.PaymentMethods");
            DropForeignKey("dbo.PassengerOrders", "Order_OrderId", "dbo.Orders");
            DropForeignKey("dbo.PassengerOrders", "Passenger_PassengerId", "dbo.Passengers");
            DropForeignKey("dbo.Orders", "Gds_GdsId", "dbo.Gds");
            DropForeignKey("dbo.Orders", "FlightLeg_FlightLegId", "dbo.FlightLegs");
            DropForeignKey("dbo.Orders", "Customer_CustomerId", "dbo.Customers");
            DropForeignKey("dbo.Orders", "CommissionCurrency_CurrencyId", "dbo.Currencies");
            DropForeignKey("dbo.Orders", "ClearingType_ClearingTypeId", "dbo.ClearingTypes");
            DropForeignKey("dbo.Orders", "CardType_CardTypeId", "dbo.CardTypes");
            DropForeignKey("dbo.Orders", "Airline_AirlineId", "dbo.Airlines");
            DropForeignKey("dbo.Orders", "Agency_AgencyId", "dbo.Agencies");
            DropForeignKey("dbo.FlightLegs", "ReturnRouteId", "dbo.Routes");
            DropForeignKey("dbo.FlightLegs", "RouteId", "dbo.Routes");
            DropIndex("dbo.PassengerOrders", new[] { "Order_OrderId" });
            DropIndex("dbo.PassengerOrders", new[] { "Passenger_PassengerId" });
            DropIndex("dbo.Orders", new[] { "SellingCurrency_CurrencyId" });
            DropIndex("dbo.Orders", new[] { "PaymentMethod_PaymentMethodId" });
            DropIndex("dbo.Orders", new[] { "Gds_GdsId" });
            DropIndex("dbo.Orders", new[] { "FlightLeg_FlightLegId" });
            DropIndex("dbo.Orders", new[] { "Customer_CustomerId" });
            DropIndex("dbo.Orders", new[] { "CommissionCurrency_CurrencyId" });
            DropIndex("dbo.Orders", new[] { "ClearingType_ClearingTypeId" });
            DropIndex("dbo.Orders", new[] { "CardType_CardTypeId" });
            DropIndex("dbo.Orders", new[] { "Airline_AirlineId" });
            DropIndex("dbo.Orders", new[] { "Agency_AgencyId" });
            DropIndex("dbo.FlightLegs", new[] { "ReturnRouteId" });
            DropIndex("dbo.FlightLegs", new[] { "RouteId" });
            DropTable("dbo.PassengerOrders");
            DropTable("dbo.Tests");
            DropTable("dbo.PaymentMethods");
            DropTable("dbo.Passengers");
            DropTable("dbo.Orders");
            DropTable("dbo.MailFilters");
            DropTable("dbo.Gds");
            DropTable("dbo.FullReports");
            DropTable("dbo.Routes");
            DropTable("dbo.FlightLegs");
            DropTable("dbo.Customers");
            DropTable("dbo.Currencies");
            DropTable("dbo.ClearingTypes");
            DropTable("dbo.CardTypes");
            DropTable("dbo.Airlines");
            DropTable("dbo.Agencies");
        }
    }
}
