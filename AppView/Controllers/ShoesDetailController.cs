using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace AppView.Controllers
{
    public class ShoesDetailController : Controller
    {
        private readonly IAllRepositories<ShoesDetails> repos;
        private ShopDBContext context = new ShopDBContext();
        private DbSet<ShoesDetails> shoesdt;

        public ShoesDetailController()
        {
            shoesdt = context.ShoesDetails;
            AllRepositories<ShoesDetails> all = new AllRepositories<ShoesDetails>(context, shoesdt);
            repos = all;
        }
        public async Task<IActionResult> GetAllShoesDetails()
        {
            string apiUrl = "https://localhost:7036/api/ShoesDetails";
            var httpClient = new HttpClient(); // tạo ra để callApi
            var response = await httpClient.GetAsync(apiUrl);// Lấy dữ liệu ra
                                                             // Lấy dữ liệu Json trả về từ Api được call dạng string
            string apiData = await response.Content.ReadAsStringAsync();
            // Lấy kqua trả về từ API
            // Đọc từ string Json vừa thu được sang List<T>
            var shoesdt = JsonConvert.DeserializeObject<List<ShoesDetails>>(apiData);
            return View(shoesdt);
        }
        [HttpGet]
        public async Task<IActionResult> CreateShoesDetail()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateShoesDetail(ShoesDetails shoesdt)
        {
            /*string apiUrl = $"https://localhost:7036/api/Role/create-role?UserName={role.RoleName}&Status={role.Status}";
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();
            // Cập nhật thông tin từ apiData vào đối tượng customer
            var newRole = JsonConvert.DeserializeObject<Role>(apiData);*/
            repos.AddItem(shoesdt);
            return RedirectToAction("GetAllShoesDetail");
        }
        [HttpGet]
        public IActionResult EditShoesDetail(Guid id) // Khi ấn vào Create thì hiển thị View
        {
            // Lấy Product từ database dựa theo id truyền vào từ route
            ShoesDetails shoesdt = repos.GetAll().FirstOrDefault(c => c.ShoesDetailsId == id);
            return View(shoesdt);
        }
        public IActionResult EditShoesDetail(ShoesDetails shoesdt) // Thực hiện việc Tạo mới
        {
            if (repos.EditItem(shoesdt))
            {
                return RedirectToAction("GetAllShoesDetail");
            }
            else return BadRequest();
        }
        public IActionResult DeleteShoesDetail(Guid id)
        {
            var shoesdt = repos.GetAll().First(c => c.ShoesDetailsId == id);
            if (repos.RemoveItem(shoesdt))
            {
                return RedirectToAction("GetAllShoesDetail");
            }
            else return BadRequest();
        }
        public IActionResult Details(Guid id)
        {
            ShopDBContext dBContext = new ShopDBContext();
            var shoesdt = repos.GetAll().FirstOrDefault(c => c.ShoesDetailsId == id);
            return View(shoesdt);
        }
    }
}
