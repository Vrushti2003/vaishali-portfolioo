using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaishaliPortfolio.Data;
using VaishaliPortfolio.Models;

namespace VaishaliPortfolio.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlogController> _logger;

        public BlogController(ApplicationDbContext context, ILogger<BlogController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Blog
        public async Task<IActionResult> Index(int page = 1, string? search = null, string? category = null)
        {
            const int pageSize = 5;

            var query = _context.BlogPosts
                .Where(b => b.IsPublished)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Title.Contains(search) || b.Content.Contains(search) || b.Summary!.Contains(search));
                ViewBag.SearchTerm = search;
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b => b.Category == category);
                ViewBag.Category = category;
            }

            var totalPosts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

            var blogPosts = await query
                .OrderByDescending(b => b.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            // Get categories for filter dropdown
            ViewBag.Categories = await _context.BlogPosts
                .Where(b => b.IsPublished && !string.IsNullOrEmpty(b.Category))
                .Select(b => b.Category)
                .Distinct()
                .ToListAsync();

            return View(blogPosts);
        }

        // GET: /Blog/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var blogPost = await _context.BlogPosts
                .FirstOrDefaultAsync(b => b.Id == id && b.IsPublished);

            if (blogPost == null)
            {
                return NotFound();
            }

            // Increment view count
            blogPost.ViewCount++;
            await _context.SaveChangesAsync();

            // Get related posts
            var relatedPosts = await _context.BlogPosts
                .Where(b => b.Id != id && b.IsPublished &&
                       (b.Category == blogPost.Category || (b.Tags != null && blogPost.Tags != null && b.Tags.Contains(blogPost.Tags))))
                .OrderByDescending(b => b.CreatedDate)
                .Take(3)
                .ToListAsync();

            ViewBag.RelatedPosts = relatedPosts;

            return View(blogPost);
        }
    }
}