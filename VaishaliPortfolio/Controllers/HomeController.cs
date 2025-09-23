using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VaishaliPortfolio.Models;
using VaishaliPortfolio.Data;
using Microsoft.EntityFrameworkCore;

namespace VaishaliPortfolio.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new ContactInquiry();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactInquiry model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                model.CreatedDate = DateTime.Now;
                _context.ContactInquiries.Add(model);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Contact inquiry saved to database: {model.Name} - {model.PhoneNumber}");

                TempData["SuccessMessage"] = "Thank you for your inquiry! Vaishali will contact you soon.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing contact form");
                ModelState.AddModelError("", "There was an error sending your message. Please try again.");
                return View(model);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}