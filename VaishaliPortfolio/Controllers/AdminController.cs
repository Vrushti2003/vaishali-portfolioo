using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaishaliPortfolio.Data;
using VaishaliPortfolio.Models;

namespace VaishaliPortfolio.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Admin/Blog
        public async Task<IActionResult> Blog()
        {
            var blogPosts = await _context.BlogPosts
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(blogPosts);
        }

        // GET: /Admin/CreateBlog
        public IActionResult CreateBlog()
        {
            var model = new BlogPost();
            return View(model);
        }

        // POST: /Admin/CreateBlog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBlog(BlogPost blogPost)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found. Please log in again.");
                    return View(blogPost);
                }

                // Set the required fields BEFORE validation
                blogPost.AuthorId = user.Id;
                blogPost.AuthorName = user.Email ?? "Vaishali Shah";
                blogPost.CreatedDate = DateTime.Now;

                // Remove AuthorId from ModelState validation since we're setting it programmatically
                ModelState.Remove(nameof(BlogPost.AuthorId));
                ModelState.Remove(nameof(BlogPost.AuthorName));
                ModelState.Remove(nameof(BlogPost.CreatedDate));

                if (!ModelState.IsValid)
                {
                    return View(blogPost);
                }

                _context.BlogPosts.Add(blogPost);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Blog post created: {blogPost.Title} by {blogPost.AuthorName}");
                TempData["SuccessMessage"] = "Blog post created successfully!";

                return RedirectToAction(nameof(Blog));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog post");
                ModelState.AddModelError("", "An error occurred while creating the blog post.");
                return View(blogPost);
            }
        }

        // GET: /Admin/EditBlog/5
        public async Task<IActionResult> EditBlog(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // POST: /Admin/EditBlog/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBlog(int id, BlogPost blogPost)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            // Remove fields that shouldn't be validated on edit
            ModelState.Remove(nameof(BlogPost.AuthorId));
            ModelState.Remove(nameof(BlogPost.AuthorName));
            ModelState.Remove(nameof(BlogPost.CreatedDate));

            if (!ModelState.IsValid)
            {
                return View(blogPost);
            }

            try
            {
                var existingPost = await _context.BlogPosts.FindAsync(id);
                if (existingPost == null)
                {
                    return NotFound();
                }

                // Update properties
                existingPost.Title = blogPost.Title;
                existingPost.Content = blogPost.Content;
                existingPost.Summary = blogPost.Summary;
                existingPost.FeaturedImageUrl = blogPost.FeaturedImageUrl;
                existingPost.Category = blogPost.Category;
                existingPost.Tags = blogPost.Tags;
                existingPost.IsPublished = blogPost.IsPublished;
                existingPost.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Blog post updated: {existingPost.Title}");
                TempData["SuccessMessage"] = "Blog post updated successfully!";

                return RedirectToAction(nameof(Blog));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog post");
                ModelState.AddModelError("", "An error occurred while updating the blog post.");
                return View(blogPost);
            }
        }

        // GET: /Admin/DeleteBlog/5
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // POST: /Admin/DeleteBlog/5
        [HttpPost, ActionName("DeleteBlog")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBlogConfirmed(int id)
        {
            try
            {
                var blogPost = await _context.BlogPosts.FindAsync(id);
                if (blogPost != null)
                {
                    _context.BlogPosts.Remove(blogPost);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Blog post deleted: {blogPost.Title}");
                    TempData["SuccessMessage"] = "Blog post deleted successfully!";
                }

                return RedirectToAction(nameof(Blog));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog post");
                TempData["ErrorMessage"] = "An error occurred while deleting the blog post.";
                return RedirectToAction(nameof(Blog));
            }
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var totalPosts = await _context.BlogPosts.CountAsync();
            var publishedPosts = await _context.BlogPosts.CountAsync(b => b.IsPublished);
            var totalViews = await _context.BlogPosts.SumAsync(b => b.ViewCount);
            var totalContacts = await _context.ContactInquiries.CountAsync();

            ViewBag.TotalPosts = totalPosts;
            ViewBag.PublishedPosts = publishedPosts;
            ViewBag.TotalViews = totalViews;
            ViewBag.TotalContacts = totalContacts;

            // Recent blog posts
            var recentPosts = await _context.BlogPosts
                .OrderByDescending(b => b.CreatedDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentPosts = recentPosts;

            // Recent contact inquiries
            var recentContacts = await _context.ContactInquiries
                .OrderByDescending(c => c.CreatedDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentContacts = recentContacts;

            return View();
        }
    }
}