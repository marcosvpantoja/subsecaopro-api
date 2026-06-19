namespace SubseOpro.Application.ViewModels.Dashboard;

public class DashboardStatsViewModel
{
    public int ActiveLawyers { get; set; }
    public int ActiveLawyersAddedThisMonth { get; set; }
    public decimal RoomRevenueThisMonth { get; set; }
    public double RoomRevenueChangePercent { get; set; }
    public int ReservationsToday { get; set; }
    public int ReservationsTodayConfirmed { get; set; }
    public int ReservationsTodayPending { get; set; }
    public int CouponsGenerated { get; set; }
    public int CouponsGeneratedLast7Days { get; set; }
    public List<DashboardRevenueDayViewModel> RevenueLast7Days { get; set; } = [];
    public decimal RevenueLast7DaysTotal { get; set; }
}

public class DashboardRevenueDayViewModel
{
    public string Day { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
