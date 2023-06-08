using AppData.IRepositories;
using AppData.IServices;
using AppData.Models;
using AppData.Repositories;
using AppData.Services;
using AppView.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ErrorViewModel = AppView.Models.ErrorViewModel;

namespace AppView.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShopDBContext _shopDBContext;
        private readonly IShoesDetailsService _shoesDT;
        private readonly IProductService _product;
        private readonly IImageService _image;
        public HomeController()
        {
            _shopDBContext = new ShopDBContext();
            _shoesDT = new ShoesDetailsService();
            _product = new ProductService();
            _image = new ImageService();
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
                var shoesList = _shoesDT.GetAllShoesDetails();
                foreach (var shoes in shoesList)
                {
                    //kiểm tra trong db có thg image nào đã chứa thằng ShoesDetails tương ứng chưa, nếu tồn tại rồi gán cho thuộc tính imageUrl giá trị của link ảnh đó
                    var firstImage = _image.GetAllImages().FirstOrDefault(c => c.ShoesDetailsID == shoes.ShoesDetailsId);
                    if (firstImage != null)
                    {
                        shoes.ImageUrl = firstImage.Image1;
                    }
                    var nameProduct = _product.GetAllProducts().FirstOrDefault(c => c.ProductID == shoes.ProductID);
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
            var ShoesDT = _shoesDT.GetAllShoesDetails().FirstOrDefault(c => c.ShoesDetailsId == id);
            var NameProduct = _product.GetAllProducts().FirstOrDefault(c => c.ProductID == ShoesDT.ProductID);
            if (NameProduct != null )
            {
                ViewBag.nameProduct = NameProduct.Name;
            }
            var ImageGoldens = _image.GetAllImages().FirstOrDefault(c => c.ShoesDetailsID == id);
            ViewBag.ImageGolden1 = ImageGoldens.Image1;
            ViewBag.ImageGolden2 = ImageGoldens.Image2;
            ViewBag.ImageGolden3 = ImageGoldens.Image3;
            ViewBag.ImageGolden4 = ImageGoldens.Image4;
            return View(ShoesDT);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
