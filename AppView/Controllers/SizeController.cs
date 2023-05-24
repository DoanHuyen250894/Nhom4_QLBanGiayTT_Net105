using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AppView.Controllers
{
    public class SizeController : Controller
    {
        private readonly IAllRepositories<Size> _repos;
        private ShopDBContext _dbcontext = new ShopDBContext();
        private DbSet<Size> _size;

        public SizeController()
        {
            _size = _dbcontext.Sizes;
            AllRepositories<Size> all = new AllRepositories<Size>(_dbcontext, _size);
            _repos = all;
        }
        public async Task<IActionResult> GetAllSize()
        {
            string apiUrl = "https://localhost:7036/api/Size/Get-Size";
            var httpClient = new HttpClient(); // tạo ra để callApi
            var response = await httpClient.GetAsync(apiUrl);// Lấy dữ liệu ra
                                                             // Lấy dữ liệu Json trả về từ Api được call dạng string
            string apiData = await response.Content.ReadAsStringAsync();
            // Lấy kqua trả về từ API
            // Đọc từ string Json vừa thu được sang List<T>
            var size = JsonConvert.DeserializeObject<List<Size>>(apiData);

            return View(size);
        }
        public async Task<IActionResult> CreateSize()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateSize(Size size)
        {
            //string apiUrl = $"https://localhost:7036/api/Size/Create-size? Name={size.Name}&Status={size.Status}";
            //var httpClient = new HttpClient();
            //var response = await httpClient.GetAsync(apiUrl);
            //string apiData = await response.Content.ReadAsStringAsync();
            //var newSize = JsonConvert.DeserializeObject<Size>(apiData);
            _repos.AddItem(size);
            return RedirectToAction("GetAllSize");


        }
        [HttpGet]
        public IActionResult EditSize(Guid id) // Khi ấn vào Create thì hiển thị View
        {
            // Lấy Product từ database dựa theo id truyền vào từ route
            Size size = _repos.GetAll().FirstOrDefault(c => c.SizeID == id);
            return View(size);
        }
        public IActionResult EditSize(Size size) // Thực hiện việc Tạo mới
        {
            if (_repos.EditItem(size))
            {
                return RedirectToAction("GetAllSize");
            }
            else return BadRequest();
        }
        public IActionResult DeleteSize(Guid id)
        {
            var sz = _repos.GetAll().First(c => c.SizeID == id);
            if (_repos.RemoveItem(sz))
            {
                return RedirectToAction("GetAllSize");
            }
            else return Content("Error");
        }
       


    }
}
