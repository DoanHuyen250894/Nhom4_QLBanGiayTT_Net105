using AppData.IRepositories;
using AppData.IServices;
using AppData.Models;
using AppData.Repositories;
using AppData.Services;
using AppView.Models;
using AppView.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AppView.Controllers
{
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private ShopDBContext _dBContext;
        private readonly IShoesDetailsService _shoesDT;
        private readonly IProductService _product;
        private readonly IImageService _image;
        public CartController(ILogger<CartController> logger)
        {
            _logger = logger;
            _dBContext = new ShopDBContext();
            _shoesDT = new ShoesDetailsService();
            _product = new ProductService();
            _image = new ImageService();
        }
        public IActionResult Cart()
        {
            var cartItems = SessionServices.GetObjFromSession(HttpContext.Session, "Cart");
            return View(cartItems);
        }
        public IActionResult AddToCart(Guid id, string size)
        {
            var ShoesDT = _shoesDT.GetAllShoesDetails().FirstOrDefault(c => c.ShoesDetailsId == id);
            if (ShoesDT == null)
            {
                return Content("Error");
            }
            var cartItems = SessionServices.GetObjFromSession(HttpContext.Session, "Cart") as List<CartItemViewModel>;
            //kiểm tra giỏ hàng tồn tại chưa
            if (cartItems == null)
            {
                // Nếu chưa có, tạo mới giỏ hàng với sản phẩm đó
                cartItems = new List<CartItemViewModel>
                {
                    new CartItemViewModel
                    {
                        ShoesDetailsID = ShoesDT.ShoesDetailsId,
                        Quantity = 1,
                        ProductName = _product.GetAllProducts().FirstOrDefault(c => c.ProductID == ShoesDT.ProductID)?.Name,
                        Price = ShoesDT.Price,
                        Description = ShoesDT.Description,
                        Size = size,
                        ProductImage = _image.GetAllImages().FirstOrDefault(c => c.ShoesDetailsID == ShoesDT.ShoesDetailsId)?.Image1,
                        MaHD = ""
                    }
                };
                SessionServices.SetObjToSession(HttpContext.Session, "Cart", cartItems);
            }
            else
            {
                // Nếu giỏ hàng đã tồn tại, kiểm tra sản phẩm đó đã có trong giỏ hàng chưa
                var cartItem = cartItems.FirstOrDefault(c => c.ShoesDetailsID == ShoesDT.ShoesDetailsId && c.Size == size);
                if (cartItem == null)
                {
                    // Nếu chưa có, thêm sản phẩm vào giỏ hàng với số lượng là 1
                    cartItems.Add(new CartItemViewModel
                    {
                        ShoesDetailsID = ShoesDT.ShoesDetailsId,
                        Quantity = 1,
                        ProductName = _product.GetAllProducts().FirstOrDefault(c => c.ProductID == ShoesDT.ProductID)?.Name,
                        Price = ShoesDT.Price,
                        Description = ShoesDT.Description,
                        Size = size,
                        ProductImage = _image.GetAllImages().FirstOrDefault(c => c.ShoesDetailsID == ShoesDT.ShoesDetailsId)?.Image1,
                        MaHD = ""
                    });
                }
                else
                {
                    // Nếu sản phẩm đã có trong giỏ hàng, tăng số lượng lên 1
                    cartItem.Quantity++;
                }
                SessionServices.SetObjToSession(HttpContext.Session, "Cart", cartItems);
            }
            return RedirectToAction("Cart");
        }
        public IActionResult RemoveCartItem(Guid id)
        {
            // Lấy thông tin giỏ hàng từ session
            List<CartItemViewModel> cartItems = SessionServices.GetObjFromSession(HttpContext.Session, "Cart") as List<CartItemViewModel>;
            // Tìm kiếm sản phẩm cần xóa
            CartItemViewModel itemToRemove = cartItems.FirstOrDefault(c => c.ShoesDetailsID == id);
            // Nếu sản phẩm tồn tại trong giỏ hàng, thực hiện xóa
            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                // Lưu lại thông tin giỏ hàng mới vào session
                SessionServices.SetObjToSession(HttpContext.Session, "Cart", cartItems);
            }
            // Chuyển hướng trở lại trang giỏ hàng
            return RedirectToAction("Cart");
        }
    }
}
