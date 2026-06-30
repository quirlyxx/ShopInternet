using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopInternet.Data;
using ShopInternet.Interfaces;
using ShopInternet.Models;

namespace ShopInternet.Controllers
{
    public class ProductController : Controller
    {
        private readonly ShopDbContext _context;
        private readonly IUploader _uploader;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ShopDbContext context, IUploader uploader, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _uploader = uploader;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            var shopDbContext = _context.Product.Include(p => p.Category);
            return View(await shopDbContext.ToListAsync());
        }

        // Простий метод збереження PDF-файлу опису товару (без IUploader, своя реалізація)
        private async Task<string> SaveDescriptionPdfAsync(IFormFile pdfFile)
        {
            string folder = Path.Combine(_webHostEnvironment.WebRootPath, "files", "products");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string newFileName = Guid.NewGuid() + ".pdf";
            string fullPath = Path.Combine(folder, newFileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await pdfFile.CopyToAsync(stream);

            return newFileName;
        }

        private void DeleteDescriptionPdf(string fileName)
        {
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "files", "products", fileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name");
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,Image,DescriptionFile,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                var imageFile = Request.Form.Files["imageFile"];
                if (imageFile != null)
                {
                    product.Image = await _uploader.UploadFile(imageFile, WC.ImagePath);
                }

                var pdfFile = Request.Form.Files["pdfFile"];
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    product.DescriptionFile = await SaveDescriptionPdfAsync(pdfFile);
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Image,DescriptionFile,CategoryId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productDb = await _context.Product.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (productDb == null)
                    {
                        return NotFound();
                    }

                    var imageFile = Request.Form.Files["imageFile"];
                    if (imageFile != null)
                    {
                        if (!string.IsNullOrEmpty(productDb.Image))
                        {
                            _uploader.DeleteFile(WC.ImagePath, productDb.Image);
                        }
                        product.Image = await _uploader.UploadFile(imageFile, WC.ImagePath);
                    }
                    else
                    {
                        product.Image = productDb.Image;
                    }

                    var pdfFile = Request.Form.Files["pdfFile"];
                    if (pdfFile != null && pdfFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(productDb.DescriptionFile))
                        {
                            DeleteDescriptionPdf(productDb.DescriptionFile);
                        }
                        product.DescriptionFile = await SaveDescriptionPdfAsync(pdfFile);
                    }
                    else
                    {
                        product.DescriptionFile = productDb.DescriptionFile;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw new HttpRequestException("Bad request something wrong :(");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
                if (!string.IsNullOrEmpty(product.Image))
                {
                    _uploader.DeleteFile(WC.ImagePath, product.Image);
                }
                if (!string.IsNullOrEmpty(product.DescriptionFile))
                {
                    DeleteDescriptionPdf(product.DescriptionFile);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }
    }
}
