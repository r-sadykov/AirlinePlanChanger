using System;
using System.Diagnostics;
using System.Linq;
using AirlinePlanChanges_MailCenter.AppData.Database;
using AirlinePlanChanges_MailCenter.Model;

namespace AirlinePlanChanges_MailCenter.Utils
{
    internal class FullReportStatisticsGatherer
    {
        private readonly string _connection;
        public FullReportStatisticsGatherer(string connectionString)
        {
            _connection = connectionString;
        }

        public FullReportStatistics GetStatistics(DateTime currentDateTime)
        {
            FullReportStatistics statistics = new FullReportStatistics {LastMonth = currentDateTime.AddMonths(-1)};
            CallCenterContext context=new CallCenterContext(_connection);
            var reports = from item in context.FullReports
                where (item.BookingDate.Year == statistics.LastMonth.Year)
                select (new
                {
                    item.Gds,
                    Vendor=item.FirstAirline,
                    Agency=item.SalesPoint,
                    Finance=item.TotalPrice,
                    Segments=item.NumberOfSegments,
                    item.BookingDate
                });
            if (!reports.Any())
            {
                return null;
            }

            var gds = reports.Where(c => c.BookingDate.Month == statistics.LastMonth.Month)
                    .GroupBy(g => g.Gds)
                    .Select(s => new
                    {
                        GdsGroup = s.Key,
                        Finance = s.Sum(c => c.Finance),
                        Segments = s.Sum(c => c.Segments)
                    }).OrderByDescending(c => c.Finance).Take(3).ToList();

                foreach (var gd in gds)
                {
                    TopResults tr = new TopResults
                    {
                        Name = gd.GdsGroup,
                        FinanceResult = gd.Finance,
                        SegmentsResult = gd.Segments
                    };
                    statistics.GdsTopResultses.Add(tr);
                }

            var vendor = reports.Where(c => c.BookingDate.Month == statistics.LastMonth.Month)
                .GroupBy(g => g.Vendor)
                .Select(s => new
                {
                    VendorGroup = s.Key,
                    Finance = s.Sum(c => c.Finance),
                    Segments = s.Sum(c => c.Segments)
                }).OrderByDescending(c => c.Finance).Take(3).ToList();
            foreach (var vr in vendor)
            {
                TopResults tr = new TopResults
                {
                    Name = vr.VendorGroup,
                    FinanceResult = vr.Finance,
                    SegmentsResult = vr.Segments
                };
                statistics.VendorTopResultses.Add(tr);
            }
            var agency = reports.Where(c => c.BookingDate.Month == statistics.LastMonth.Month)
                .GroupBy(g => g.Agency)
                .Select(s => new
                {
                    AgencyGroup = s.Key,
                    Finance = s.Sum(c => c.Finance),
                    Segments = s.Sum(c => c.Segments)
                }).OrderByDescending(c => c.Finance).Take(3).ToList();
            foreach (var ag in agency)
            {
                TopResults tr = new TopResults
                {
                    Name = ag.AgencyGroup,
                    FinanceResult = ag.Finance,
                    SegmentsResult = ag.Segments
                };
                statistics.AgenciesTopResultses.Add(tr);
            }
            var monthSales = reports.Where(c => c.BookingDate.Month == statistics.LastMonth.Month).Select(s => new
            {
                s.BookingDate.Month,
                s.Finance,
                s.Segments
            }).GroupBy(g => g.Month).Select(ss => new
            {
                Finance = ss.Sum(c => c.Finance),
                Segment = ss.Sum(c => c.Segments)
            }).FirstOrDefault();
            Debug.Assert(monthSales != null, nameof(monthSales) + " != null");
            TopResults mSales = new TopResults
            {
                FinanceResult = monthSales.Finance,
                SegmentsResult = monthSales.Segment
            };
            statistics.CommonTopResultses.Add(mSales);

            var yearSales =reports.Select(s=>new
            {
                s.BookingDate.Year,
                s.Finance,
                Segment=s.Segments
            }).GroupBy(g=>g.Year).Select(ss=>new
            {
                Year=ss.Key,
                Finance=ss.Sum(c=>c.Finance),
                Segment=ss.Sum(c=>c.Segment)
            }).FirstOrDefault();
            Debug.Assert(yearSales != null, nameof(yearSales) + " != null");
            TopResults ySales = new TopResults
            {
                FinanceResult = yearSales.Finance,
                SegmentsResult = yearSales.Segment
            };
            statistics.CommonTopResultses.Add(ySales);

                var bestMonthSales = reports.Select(s => new
                {
                    s.BookingDate.Month,
                    s.Finance,
                    s.Segments
                }).GroupBy(g => g.Month).Select(ss => new
                {
                    Month = ss.Key,
                    Finance = ss.Sum(c => c.Finance),
                    Segment = ss.Sum(c => c.Segments)
                }).OrderByDescending(c => c.Finance).Take(1).FirstOrDefault();
            Debug.Assert(bestMonthSales != null, nameof(bestMonthSales) + " != null");
            TopResults bSales = new TopResults
                {
                    Name = new DateTime(statistics.LastMonth.Year,bestMonthSales.Month,1).ToString("MMMM"),
                    FinanceResult = bestMonthSales.Finance,
                    SegmentsResult = bestMonthSales.Segment
                };
                statistics.CommonTopResultses.Add(bSales);

            return statistics;
        }
    }
}
