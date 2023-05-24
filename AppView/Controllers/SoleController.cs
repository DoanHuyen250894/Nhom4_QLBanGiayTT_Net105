using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AppView.Controllers
{
    public class SoleController : Controller
    {
        private readonly IAllRepositories<Sole> _repos;
        private ShopDBContext _dbContext = new ShopDBContext();
        private DbSet<Sole> _sole;
        public SoleController()
        {
            _sole = _dbContext.Soles;
            AllRepositories<Sole> all = new AllRepositories<Sole>(_dbContext, _sole);
            _repos = all;
        }
        public async Task<IActionResult> GetAllSole()
        {
            string apiUrl = "https://localhost:7036/api/Sole/Get-Sole";
            var httpClient = new HttpClient(); // tạo ra để callApi
            var response = await httpClient.GetAsync(apiUrl);// Lấy dữ liệu ra
                                                             // Lấy dữ liệu Json trả về từ Api được call dạng string
            string apiData = await response.Content.ReadAsStringAsync();
            // Lấy kqua trả về từ API
            // Đọc từ string Json vừa thu được sang List<T>
            var sole = JsonConvert.DeserializeObject<List<Sole>>(apiData);
            return View(sole);
        }
        [HttpGet]
        public async Task<IActionResult> CreateSole()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateSole(Sole sole)
        {
        //    string apiUrl = $"https://localhost:7036/api/Sole/Create-Sole? Name={sole.Name}&Fabric={sole.Fabric}&Status={sole.Status}&Height={sole.Height}";
        //    var httpClient = new HttpClient();
        //    var response = await httpClient.GetAsync(apiUrl);
        //    string apiData = await response.Content.ReadAsStringAsync();
        //    // Cập nhật thông tin từ apiData vào đối tượng customer
        //    var newSole = JsonConvert.DeserializeObject<Sole>(apiData);
            _repos.AddItem(sole);
            return RedirectToAction("GetAllSole");
        }
        [HttpGet]
        public IActionResult EditSole(Guid id) // Khi ấn vào Create thì hiển thị View
        {
            // Lấy Product từ database dựa theo id truyền vào từ route
            Sole sole = _repos.GetAll().FirstOrDefault(c => c.SoleID == id);
            return View(sole);
        }
        public IActionResult EditSole(Sole sole) // Thực hiện việc Tạo mới
        {
            if (_repos.EditItem(sole))
            {
                return RedirectToAction("GetAllSole");
            }
            else return BadRequest();
        }
        public IActionResult DeleteSole(Guid id)
        {
            var sl = _repos.GetAll().First(c => c.SoleID == id);
            if (_repos.RemoveItem(sl))
            {
                return RedirectToAction("GetAllSole");
            }
            else return Content("Error");
        }
    }
}
