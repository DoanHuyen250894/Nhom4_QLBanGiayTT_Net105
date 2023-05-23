using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AppView.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IAllRepositories<Employee> _repos;
        private ShopDBContext _dbContext = new ShopDBContext();
        private DbSet<Employee> _customer;
        public EmployeeController()
        {
            _customer = _dbContext.Employees;
            AllRepositories<Employee> all = new AllRepositories<Employee>(_dbContext, _customer);
            _repos = all;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllEmployee() 
        {
            string apiUrl = "https://localhost:7036/api/Employee/get-employee";
            var httpClient = new HttpClient(); // tạo ra để callApi
            var response = await httpClient.GetAsync(apiUrl);// Lấy dữ liệu ra
                                                             // Lấy dữ liệu Json trả về từ Api được call dạng string
            string apiData = await response.Content.ReadAsStringAsync();
            // Lấy kqua trả về từ API
            // Đọc từ string Json vừa thu được sang List<T>
            var employees = JsonConvert.DeserializeObject<List<Employee>>(apiData);
            return View(employees);
        }
        [HttpGet]
        public async Task<IActionResult> CreateEmployee()
        {
            using (ShopDBContext shopDBContext = new ShopDBContext())
            {
                var role = shopDBContext.Roles.ToList();
                SelectList selectListRole = new SelectList(role, "RoleID", "RoleName");
                ViewBag.RoleList = selectListRole;
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateEmployee(Employee employee)
        {
            if (_repos.AddItem(employee))
            {
                return RedirectToAction("GetAllEmployee");
            }
            else return BadRequest();
        }
        [HttpGet]
        public IActionResult UpdateEmployee(Guid id)
        {
            Employee employee = _repos.GetAll().FirstOrDefault(c => c.EmployeeID == id);
            return View(employee);
        }
        public IActionResult UpdateEmployee(Employee employee)
        {
            if (_repos.EditItem(employee))
            {
                return RedirectToAction("GetAllEmployee");
            }
            else return Content("Loi");
        }
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var employee = _repos.GetAll().First(c => c.EmployeeID == id);
            if (_repos.RemoveItem(employee))
            {
                return RedirectToAction("GetAllEmployee");
            }
            else
            {
                return Content("Error");
            }
        }
    }
}
