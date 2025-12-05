using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;
using MyTitlePawnCompany.Services;

namespace MyTitlePawnCompany.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ITitlePawnService _titlePawnService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardMetrics Metrics { get; set; } = new();

    public IndexModel(ITitlePawnService titlePawnService, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _titlePawnService = titlePawnService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return;

        var companyId = user.CompanyId;

        // Get all loans for the company
        var allLoans = await _unitOfWork.TitlePawns.GetCompanyTitlePawnsAsync(companyId);
        var activeLoans = allLoans.Where(tp => tp.Status == "Active").ToList();
        var pendingLoans = allLoans.Where(tp => tp.Status == "Pending").ToList();
        var defaultedLoans = allLoans.Where(tp => tp.Status == "Defaulted").ToList();

        // Calculate metrics
        Metrics.ActiveLoans = activeLoans.Count;
        Metrics.PendingApprovals = pendingLoans.Count;
        Metrics.DefaultedLoans = defaultedLoans.Count;

        // Calculate financial metrics
        Metrics.TotalPortfolio = activeLoans.Sum(tp => tp.RemainingBalance);
        Metrics.TotalLoanValue = allLoans.Sum(tp => tp.LoanAmountApproved + tp.TotalInterestCharged);
        Metrics.TotalInterest = allLoans.Sum(tp => tp.TotalInterestCharged);

        // Get all payments for the company
        var allPayments = await _unitOfWork.Payments.GetCompanyPaymentsAsync(companyId);
        Metrics.TotalPayments = allPayments.Sum(p => p.Amount);

        // Calculate overdue amount (loans past maturity date)
        Metrics.OverdueAmount = activeLoans
            .Where(tp => tp.LoanMaturityDate < DateTime.UtcNow)
            .Sum(tp => tp.MonthlyPayment);
    }
}
