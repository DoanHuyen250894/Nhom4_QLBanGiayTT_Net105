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
using System.Drawing;

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
            var userIdString = HttpContext.Session.GetString("UserId");
            var customerId = !string.IsNullOrEmpty(userIdString) ? JsonConvert.DeserializeObject<Guid>(userIdString) : Guid.Empty;
            if (customerId != Guid.Empty)
            {
                var loggedInUser = _dBContext.Customers.FirstOrDefault(c => c.CumstomerID == customerId);
                if (loggedInUser != null)
                {
                    var cartItemList = _dBContext.CartDetails
                        .Where(cd => cd.CumstomerID == loggedInUser.CumstomerID && cd.ShoesDetails != null)
                        .Select(cd => new CartItemViewModel
                        {
                            ShoesDetailsID = cd.ShoesDetailsId,
                            Quantity = cd.Quantity,
                            ProductName = _dBContext.Products.FirstOrDefault(p => p.ProductID == cd.ShoesDetails.ProductID).Name,
                            Price = cd.ShoesDetails.Price,
                            Description = cd.ShoesDetails.Description,
                            Size = _dBContext.Sizes.FirstOrDefault(i => i.SizeID == cd.ShoesDetails.SizeID).Name,
                            ProductImage = _dBContext.Images.FirstOrDefault(i => i.ShoesDetailsID == cd.ShoesDetails.ShoesDetailsId).Image1,
                            MaHD = ""
                        })
                        .ToList();
                    return View(cartItemList);
                }
            }
            // Nếu không có người dùng đăng nhập, lấy giỏ hàng từ session
            var cartItems = SessionServices.GetObjFromSession(HttpContext.Session, "Cart") as List<CartItemViewModel>;
            return View(cartItems);
        }
        public IActionResult AddToCart(Guid id, string size)
        {
            var ShoesDT = _shoesDT.GetAllShoesDetails().FirstOrDefault(c => c.ShoesDetailsId == id);
            if (ShoesDT == null)
            {
                return Content("Sản phẩm không tồn tại");
            }
            var userIdString = HttpContext.Session.GetString("UserId");
            var CustomerID = !string.IsNullOrEmpty(userIdString) ? JsonConvert.DeserializeObject<Guid>(userIdString) : Guid.Empty;
            if (CustomerID != Guid.Empty)
            {
                var loggedInUser = _dBContext.Customers.FirstOrDefault(c => c.CumstomerID == CustomerID);
                if (loggedInUser != null)
                {
                    var existingCart = _dBContext.Carts.FirstOrDefault(c => c.CumstomerID == loggedInUser.CumstomerID);
                    if (existingCart != null)
                    {
                        // Bản ghi đã tồn tại, bạn có thể cập nhật nó thay vì thêm mới
                        existingCart.Description = "Updated cart description";
                        _dBContext.Carts.Update(existingCart);
                    }
                    else
                    {
                        // Bản ghi chưa tồn tại, thêm mới bình thường
                        var cart = new Cart
                        {
                            CumstomerID = loggedInUser.CumstomerID,
                            Description = "Cart for logged in user"
                        };
                        _dBContext.Carts.Add(cart);
                    }
                    var SizeID = _dBContext.Sizes.FirstOrDefault(c => c.Name == size)?.SizeID;
                    var existingCartItem = _dBContext.CartDetails.FirstOrDefault(c => c.CumstomerID == loggedInUser.CumstomerID && c.ShoesDetailsId == ShoesDT.ShoesDetailsId && c.ShoesDetails.SizeID == SizeID);
                    if (existingCartItem != null)
                    {
                        // Sản phẩm và size đã tồn tại trong giỏ hàng, tăng số lượng lên 1
                        existingCartItem.Quantity++;
                        _dBContext.CartDetails.Update(existingCartItem);
                    }
                    else
                    {
                        // Sản phẩm và size chưa tồn tại trong giỏ hàng, thêm mới vào giỏ hàng
                        var cartDetails = new CartDetails
                        {
                            CartDetailsId = Guid.NewGuid(),
                            CumstomerID = loggedInUser.CumstomerID,
                            ShoesDetailsId = ShoesDT.ShoesDetailsId,
                            Quantity = 1
                        };
                        _dBContext.CartDetails.Add(cartDetails);
                    }
                    // Cập nhật kích thước của sản phẩm
                    ShoesDT.SizeID = SizeID;
                    _dBContext.Update(ShoesDT);
                    _dBContext.SaveChanges();
                }
            }
            else
            {
                var cartItems = SessionServices.GetObjFromSession(HttpContext.Session, "Cart") as List<CartItemViewModel>;
                if (cartItems == null)
                {
                    cartItems = new List<CartItemViewModel>();
                }
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
            var userIdString = HttpContext.Session.GetString("UserId");
            var CustomerID = !string.IsNullOrEmpty(userIdString) ? JsonConvert.DeserializeObject<Guid>(userIdString) : Guid.Empty;
            if (CustomerID != Guid.Empty)
            {
                var ShoesDT = _dBContext.CartDetails.FirstOrDefault(c => c.ShoesDetailsId == id);
                _dBContext.CartDetails.Remove(ShoesDT);
                _dBContext.SaveChanges();
            }
            else
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
            }
            return RedirectToAction("Cart");
        }
    }
}
//if (!HttpContext.Session.TryGetValue("EmployeeID", out _))
//{
//    // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập hoặc hiển thị thông báo lỗi.
//    return RedirectToAction("Login", "Employee");
//}