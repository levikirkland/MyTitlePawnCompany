namespace MyTitlePawnCompany.Pages
{
    public class DashboardMetrics
    {
        public int ActiveLoans { get; set; }
        public int PendingApprovals { get; set; }
        public int DefaultedLoans { get; set; }
        public decimal TotalPortfolio { get; set; }
        public decimal TotalLoanValue { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal OverdueAmount { get; set; }
    }
}
