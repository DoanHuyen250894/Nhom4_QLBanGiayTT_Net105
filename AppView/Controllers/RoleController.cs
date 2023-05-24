using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AppView.Controllers
{
    public class RoleController : Controller
    {
        private readonly IAllRepositories<Role> repos;
        private ShopDBContext context = new ShopDBContext();
        private DbSet<Role> role;

        public RoleController()
        {
            role = context.Roles;
            AllRepositories<Role> all = new AllRepositories<Role>(context, role);
            repos = all;
        }
        
        
       public async Task<IActionResult> GetAllRole()
        {
            string apiUrl = "https://localhost:7036/api/Role";
            var httpClient = new HttpClient(); // tạo ra để callApi
           var response = await httpClient.GetAsync(apiUrl);// Lấy dữ liệu ra
                                                             // Lấy dữ liệu Json trả về từ Api được call dạng string
            string apiData = await response.Content.ReadAsStringAsync();
            // Lấy kqua trả về từ API
            // Đọc từ string Json vừa thu được sang List<T>
            var role = JsonConvert.DeserializeObject<List<Role>>(apiData);
            return View(role);
        }
        [HttpGet]
        public async Task<IActionResult> CreateRole()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(Role role)
        {
           string apiUrl = $"https://localhost:7036/api/Role/create-role?UserName={role.RoleName}&Status={role.Status}";
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();
            // Cập nhật thông tin từ apiData vào đối tượng customer
            var newRole = JsonConvert.DeserializeObject<Role>(apiData);
            repos.AddItem(role);
            return RedirectToAction("GetAllRole");
        }
        [HttpGet]
        public IActionResult EditRole(Guid id) // Khi ấn vào Create thì hiển thị View
        {
            // Lấy Product từ database dựa theo id truyền vào từ route
            Role role = repos.GetAll().FirstOrDefault(c => c.RoleID == id);
            return View(role);
        }
        public IActionResult EditRole(Role role) // Thực hiện việc Tạo mới
        {
            if (repos.EditItem(role))
            {
                return RedirectToAction("GetAllRole");
            }
            else return BadRequest();
        }
        public IActionResult DeleteRole(Guid id)
        {
            var rol = repos.GetAll().First(c => c.RoleID == id);
            if (repos.RemoveItem(rol))
            {
                return RedirectToAction("GetAllRole");
            }
            else return BadRequest(); 
        }
        public IActionResult Details(Guid id)
        {
            ShopDBContext dBContext = new ShopDBContext();
            var rol = repos.GetAll().FirstOrDefault(c => c.RoleID == id);
            return View(rol);
        }
    }
}
