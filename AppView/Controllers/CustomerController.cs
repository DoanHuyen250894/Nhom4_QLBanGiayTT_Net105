using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
            string apiUrl = $"https://localhost:7036/api/Customer/create-customer?UserName={customer.UserName}&Password={customer.Password}&Email={customer.Email}&Sex={customer.Sex}";
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();
            // Cập nhật thông tin từ apiData vào đối tượng customer
            var newCustomer = JsonConvert.DeserializeObject<Customer>(apiData);
            _repos.AddItem(customer);
            return RedirectToAction("GetAllCustomer");
        }
        [HttpGet]
        public IActionResult EditCustomer(Guid id) // Khi ấn vào Create thì hiển thị View
        {
            // Lấy Product từ database dựa theo id truyền vào từ route
            Customer customer = _repos.GetAll().FirstOrDefault(c => c.CumstomerID == id);
            return View(customer);
        }
        public IActionResult EditCustomer(Customer customer) // Thực hiện việc Tạo mới
        {
            if (_repos.EditItem(customer))
            {
                return RedirectToAction("GetAllCustomer");
            }
            else return BadRequest();
        }
        public IActionResult DeleteCustomer(Guid id)
        {
            var cus = _repos.GetAll().First(c => c.CumstomerID == id);
            if (_repos.RemoveItem(cus))
            {
                return RedirectToAction("GetAllCustomer");
            }
            else return Content("Error");
        }
    }
}
