using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AppView.Controllers
{
    public class ColorController : Controller
    {

        //public IActionResult Index()
        //{
        //    return View();
        //}
        private readonly IAllRepositories<Color> _repos;
        private ShopDBContext _dbContext = new ShopDBContext();
        private DbSet<Color> _color;
        public ColorController()
        {
            _color = _dbContext.Colors;
            AllRepositories<Color> all = new AllRepositories<Color>(_dbContext, _color);
            _repos = all;
        }  
       
        public async Task<IActionResult> GetAllColor()
        {


            string apiUrl = "https://localhost:7036/api/Color/get-color";
            var httpClient = new HttpClient(); // tạo ra để callApi
            var response = await httpClient.GetAsync(apiUrl);// Lấy dữ liệu ra
                                                             // Lấy dữ liệu Json trả về từ Api được call dạng string
            string apiData = await response.Content.ReadAsStringAsync();
            // Lấy kqua trả về từ API
            // Đọc từ string Json vừa thu được sang List<T>
            var color = JsonConvert.DeserializeObject<List<Color>>(apiData);
            return View(color);
        }
        [HttpGet]
        public async Task<IActionResult> CreateColor()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateColor(Color color)
        {
            _repos.AddItem(color);
            return RedirectToAction("GetAllColor");
        }
        [HttpGet]
        public IActionResult EditColor(Guid id) // Khi ấn vào Create thì hiển thị View
        {
            // Lấy Product từ database dựa theo id truyền vào từ route
            Color color = _repos.GetAll().FirstOrDefault(c => c.ColorID == id);
            return View(color);
        }
        public IActionResult EditColor(Color color) // Thực hiện việc Tạo mới
        {
            if (_repos.EditItem(color))
            {
                return RedirectToAction("GetAllColor");
            }
            else return BadRequest();
        }
        public IActionResult DeleteColor(Guid id)
        {
            var colo = _repos.GetAll().First(c => c.ColorID == id);
            if (_repos.RemoveItem(colo))
            {
                return RedirectToAction("GetAllColor");
            }
            else return Content("Error");
        }
    }
}
