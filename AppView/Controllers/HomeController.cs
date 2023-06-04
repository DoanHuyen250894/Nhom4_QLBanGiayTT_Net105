using AppData.Models;
using AppView.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ErrorViewModel = AppView.Models.ErrorViewModel;

namespace AppView.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult ListProduct()
        {
            using (var dbContext = new ShopDBContext())
            {
                var shoesList = dbContext.ShoesDetails.ToList();
                foreach (var shoes in shoesList)
                {
                    //kiểm tra trong db có thg image nào đã chứa thằng ShoesDetails tương ứng chưa, nếu tồn tại rồi gán cho thuộc tính imageUrl giá trị của link ảnh đó
                    var firstImage = dbContext.Images.FirstOrDefault(i => i.ShoesDetailsID == shoes.ShoesDetailsId);
                    if (firstImage != null)
                    {
                        shoes.ImageUrl = firstImage.Image1;
                    }
                    var nameProduct = dbContext.Products.FirstOrDefault(c => c.ProductID == shoes.ProductID);
                    if (nameProduct != null)
                    {
                        ViewBag.NameSP = nameProduct.Name;
                    }
                }
                return View(shoesList);
            }
        }
        public IActionResult DetailsProduct(Guid id)
        {
            ShopDBContext shopDBContext = new ShopDBContext();
            var productDT = shopDBContext.ShoesDetails.Find(id);
            var NameProduct = shopDBContext.Products.FirstOrDefault(c => c.ProductID == productDT.ProductID);
            if (NameProduct != null )
            {
                ViewBag.nameProduct = NameProduct.Name;
            }
            var ImageGoldens = shopDBContext.Images.First(c => c.ShoesDetailsID == id);
            ViewBag.ImageGolden1 = ImageGoldens.Image1;
            ViewBag.ImageGolden2 = ImageGoldens.Image2;
            ViewBag.ImageGolden3 = ImageGoldens.Image3;
            ViewBag.ImageGolden4 = ImageGoldens.Image4;
            return View(productDT);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
