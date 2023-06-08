using System.Net.Mail;
using System.Net;
using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AppView.Controllers
{ 
    public class CustomerController : Controller
    {
        private readonly IAllRepositories<Customer> _repos;
        private ShopDBContext _dbContext = new ShopDBContext();
        private DbSet<Customer> _customer;
        public CustomerController()
        {
            _customer = _dbContext.Customers;
            AllRepositories<Customer> all = new AllRepositories<Customer>(_dbContext, _customer);
            _repos = all;
        }
        public async Task<IActionResult> GetAllCustomer()
        {
            string apiUrl = "https://localhost:7036/api/Customer/get-customer";
            var httpClient = new HttpClient(); // tạo ra để callApi
            var response = await httpClient.GetAsync(apiUrl);// Lấy dữ liệu ra
                                                             // Lấy dữ liệu Json trả về từ Api được call dạng string
            string apiData = await response.Content.ReadAsStringAsync();
            // Lấy kqua trả về từ API
            // Đọc từ string Json vừa thu được sang List<T>
            var customer = JsonConvert.DeserializeObject<List<Customer>>(apiData);
            return View(customer);
        }
        [HttpGet]
        public async Task<IActionResult> CreateCustomer()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(Customer customer)
        {
            var httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/Customer/create-customer?UserName={customer.UserName}&Password={customer.Password}&Email={customer.Email}&Sex={customer.Sex}&PhoneNumber={customer.PhoneNumber}&Status={customer.Status}";
            var response = await httpClient.PostAsync(apiUrl, null);
            return RedirectToAction("GetAllCustomer");
        }
        [HttpGet]
        public async Task<IActionResult> EditCustomer(Guid id) // Khi ấn vào Create thì hiển thị View
        {
            // Lấy Product từ database dựa theo id truyền vào từ route
            Customer customer = _repos.GetAll().FirstOrDefault(c => c.CumstomerID == id);
            return View(customer);
        }
        public async Task<IActionResult> EditCustomer(Customer customer) // Thực hiện việc Tạo mới
        {
            var httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/Customer/update-customer?id={customer.CumstomerID}&UserName={customer.UserName}&Password={customer.Password}&Email={customer.Email}&Sex={customer.Sex}&PhoneNumber={customer.PhoneNumber}&Status={customer.Status}";
            var response = await httpClient.PutAsync(apiUrl, null);
            return RedirectToAction("GetAllCustomer");
            //if (_repos.EditItem(customer))
            //{
            //    return RedirectToAction("GetAllCustomer");
            //}
            //else return BadRequest();
        }
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var cus = _repos.GetAll().First(c => c.CumstomerID == id);
            var httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/Customer/delete-customer?id={id}";
            var response = await httpClient.DeleteAsync(apiUrl);
            return RedirectToAction("GetAllCustomer");
            //if (_repos.RemoveItem(cus))
            //{
            //    return RedirectToAction("GetAllCustomer");
            //}
            //else return Content("Error");
        }

        public IActionResult Login()
        {
            ViewBag.SignUpSuccess = TempData["SignUpSuccess"];
            return View();
        }
        [HttpPost]
        public IActionResult Login(Customer customer)
        {
            var loggedInUser = _repos.GetAll().FirstOrDefault(c => c.UserName == customer.UserName && c.Password == customer.Password);
            if (loggedInUser != null)
            {
                HttpContext.Session.SetString("UserId", JsonConvert.SerializeObject(loggedInUser.CumstomerID.ToString()));
                HttpContext.Session.SetString("UserName", JsonConvert.SerializeObject(loggedInUser.UserName));
                
                TempData["SignUpSuccess"] = "Đăng nhập thành công!";
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            else
            {
                return Json(new { success = false, message = "Vui lòng nhập đúng thông tin tài khoản" });
            }
        }

        public IActionResult SignUp()
        {
            
            return View();
        }
        [HttpPost]
        public IActionResult SignUp(Customer customer, string ConfirmPassword)
        {
            if (customer.Password != ConfirmPassword)
            {
                return View();
            }
            else
            if (_repos.GetAll().Any(c => c.UserName == customer.UserName))
            {
                return Json(new { success = false, message = "Tên đăng nhập đã tồn tại" });
            }
            else
                customer.PhoneNumber = "000000000";
            //user.DiaChi = "OK";
            if (_repos.AddItem(customer))
            {
                TempData["UserName"] = customer.UserName;
                TempData["Password"] = customer.Password;
                TempData["SignUpSuccess"] = "Đăng ký tài khoản thành công!";
                return Json(new { success = true, redirectUrl = Url.Action("Login", "Customer") });
            }
            else return BadRequest();
        }
        public IActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]
        public IActionResult DangNhap(Customer customer)
        {
            if (customer.UserName.Length <= 6 || customer.Password.Length <= 6)
            {
                return Content("User Name va Password phai dai hon 6 ky tu");
            }
            Regex regex = new Regex("^[a-zA-Z0-9]+$");
            if (!regex.IsMatch(customer.UserName) || !regex.IsMatch(customer.Password))
            {
                return Content("User Name và Password chỉ được chứa chữ cái và số.");
            }
            return RedirectToAction("Index", "Home");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
