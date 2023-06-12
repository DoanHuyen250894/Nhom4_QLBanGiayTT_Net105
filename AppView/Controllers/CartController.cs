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
using System.Diagnostics;
using System.Drawing;
using System.Security.Claims;

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
        private string GenerateBillCode()
        {
            // Tạo mã đơn hàng dựa trên số lượng đơn hàng đã có trong cơ sở dữ liệu
            var count = _dBContext.Bills.Count();
            var billCode = $"HD{count + 1:000}";
            return billCode;
        }
        [HttpPost]
        public IActionResult CheckoutOk(List<CartItemViewModel> viewModel)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var CustomerID = !string.IsNullOrEmpty(userIdString) ? JsonConvert.DeserializeObject<Guid>(userIdString) : Guid.Empty;
            if (CustomerID == Guid.Empty)
            {
                return RedirectToAction("Login", "Customer");
            }

            // Thêm sản phẩm vào bảng BillDetail và cập nhật số lượng sản phẩm
            var cartItems = _dBContext.CartDetails
                .Where(c => c.CumstomerID == CustomerID)
                .Include(c => c.ShoesDetails)
                .ToList();
            // Tính tổng giá tiền cho cả hóa đơn
            decimal totalPrice = 0;
            foreach (var item in cartItems)
            {
                totalPrice += item.ShoesDetails.Price * item.Quantity;
            }
            // Tạo đơn hàng
            var bill = new Bill
            {
                BillID = Guid.NewGuid(),
                BillCode = GenerateBillCode(), // Tạo mã đơn hàng (ví dụ: HD001, HD002, ...)
                CustomerID = CustomerID,
                CreateDate = DateTime.Now,
                Status = 1,
                Note = "",
                SuccessDate = DateTime.Now,
                ShippingCosts = 0,
                DeliveryDate = DateTime.Now,
                CancelDate = DateTime.Now,
                TotalPrice = totalPrice,
                EmployeeID = Guid.Parse("899cd5fb-6a25-46ea-4507-08db5d8dd88e"),
                CouponID = Guid.Parse("db9a5ee0-9a90-47b8-95c2-925b65f44087"),
                VoucherID = Guid.Parse("c5c7648b-2bee-42c4-b0e3-a0a62e86988f")
            };
            _dBContext.Bills.Add(bill);

            foreach (var item in cartItems)
            {
                var billDetail = new BillDetails
                {
                    ID = Guid.NewGuid(),
                    BillID = bill.BillID,
                    ShoesDetailsId = item.ShoesDetailsId,
                    Quantity = item.Quantity,
                    Price = item.ShoesDetails.Price * item.Quantity
                };
                _dBContext.BillDetails.Add(billDetail);

                // Cập nhật số lượng sản phẩm
                item.ShoesDetails.AvailableQuantity -= item.Quantity;
                _dBContext.ShoesDetails.Update(item.ShoesDetails);
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            _dBContext.SaveChanges();

            // Xóa giỏ hàng của người dùng
            _dBContext.CartDetails.RemoveRange(cartItems);
            _dBContext.SaveChanges();

            // Chuyển hướng sang trang cảm ơn
            return Content("Thanh toán thành công");
        }
        [HttpPost]
        public IActionResult IncrementQuantity(Guid id)
        {
            var cartItem = _dBContext.CartDetails.FirstOrDefault(c => c.ShoesDetailsId == id);
            if (cartItem != null)
            {
                cartItem.Quantity++;
                _dBContext.SaveChanges();

                // Ghi log hoặc in thông tin ra màn hình
                Console.WriteLine("IncrementQuantity is called successfully."); // Sử dụng Console.WriteLine

                // Trả về kết quả thành công
                return Ok();
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult DecrementQuantity(Guid id)
        {
            var cartItem = _dBContext.CartDetails.FirstOrDefault(c => c.ShoesDetailsId == id);
            if (cartItem != null && cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
                _dBContext.SaveChanges();

                // Ghi log hoặc in thông tin ra màn hình
                Console.WriteLine("DecrementQuantity is called successfully."); // Sử dụng Console.WriteLine

                // Trả về kết quả thành công
                return Ok();
            }
            return NotFound();
        }
    }
}
//if (!HttpContext.Session.TryGetValue("EmployeeID", out _))
//{
//    // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập hoặc hiển thị thông báo lỗi.
//    return RedirectToAction("Login", "Employee");
//}