using Microsoft.AspNetCore.Mvc;
using QL_NhaThuoc.Services;

namespace QL_NhaThuoc.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        public async Task<IActionResult> Search(string keyword)
        {
            var products = await _productService.SearchProductsAsync(keyword);
            return View("Index", products);
        }
    }
}
