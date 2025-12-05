using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Pages.Customers
{
    [Authorize]
    public class ReferencesModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public CustomerReference? Reference { get; set; }

        public Customer? Customer { get; set; }
        public IList<CustomerReference> References { get; set; } = new List<CustomerReference>();
        public string? SuccessMessage { get; set; }

        public ReferencesModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Customer = await _unitOfWork.Customers.GetByIdAsync(id.Value);
            if (Customer == null)
                return NotFound();

            // Get references separately using the repository method
            References = (await _unitOfWork.CustomerReferences.GetCustomerReferencesAsync(id.Value)).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(int customerId)
        {
            Customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (Customer == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                References = (await _unitOfWork.CustomerReferences.GetCustomerReferencesAsync(customerId)).ToList();
                return Page();
            }

            if (Reference == null)
                return BadRequest();

            Reference.CustomerId = customerId;
            await _unitOfWork.CustomerReferences.AddAsync(Reference);
            await _unitOfWork.SaveChangesAsync();

            SuccessMessage = "Reference added successfully!";
            return RedirectToPage(new { id = customerId });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, int customerId)
        {
            var reference = await _unitOfWork.CustomerReferences.GetByIdAsync(id);
            if (reference == null)
                return NotFound();

            await _unitOfWork.CustomerReferences.DeleteAsync(reference);
            await _unitOfWork.SaveChangesAsync();

            SuccessMessage = "Reference deleted successfully!";
            return RedirectToPage(new { id = customerId });
        }
    }
}
