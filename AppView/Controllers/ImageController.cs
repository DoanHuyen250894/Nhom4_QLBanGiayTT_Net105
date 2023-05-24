using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AppView.Controllers
{
    public class ImageController : Controller
    {
        private readonly IAllRepositories<Image> _repos;
        ShopDBContext _dbContext = new ShopDBContext();
        DbSet<Image> _images;

        public ImageController()
        {

            _images = _dbContext.Images;
            AllRepositories<Image> all = new AllRepositories<Image>(_dbContext, _images);
            _repos = all;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllImge()
        {
            string apiUrl = "https://localhost:7036/api/Image";
            var httpClient = new HttpClient(); // tạo ra để callApi
            var response = await httpClient.GetAsync(apiUrl);// Lấy dữ liệu ra
                                                             // Lấy dữ liệu Json trả về từ Api được call dạng string
            string apiData = await response.Content.ReadAsStringAsync();
            // Lấy kqua trả về từ API
            // Đọc từ string Json vừa thu được sang List<T>
            var images = JsonConvert.DeserializeObject<List<Image>>(apiData);
            return View(images);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            using (ShopDBContext shopDBContext = new ShopDBContext())
            {
                var shoes = shopDBContext.ShoesDetails.ToList();
                SelectList selectListShoesDT = new SelectList(shoes, "ShoesDetailsId", "ShoesDetailsId");
                ViewBag.ShoesDTList = selectListShoesDT;
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Image im, [Bind] IFormFile imageFile)
        {
            var x = imageFile.FileName; // debug
            if (imageFile != null && imageFile.Length > 0)//Khoong null và không trống
            {
                // Trỏ tới thư mục wroot để lát nữa thực hiện việc copy sang
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "image", imageFile.FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    // Thực hiện copy ảnh chọn sang thư mục wwwrooot
                    imageFile.CopyTo(stream);
                }
                // Gán lại giá trị  cho image của đối tượng bằng tên file ảnh đã đc sao chép
                im.Image1 = imageFile.FileName;
                im.Image2 = imageFile.FileName;
                im.Image3 = imageFile.FileName;
                im.Image4 = imageFile.FileName;
            }

            if (_repos.AddItem(im))
            {
                return RedirectToAction("GetAllImge");
            }
            else return BadRequest();
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            Image image = _repos.GetAll().FirstOrDefault(c => c.ImageID == id);
            return View(image);
        }
        public IActionResult Edit(Image image)
        {
            if (_repos.EditItem(image))
            {
                return RedirectToAction("GetAllImge");
            }
            else return BadRequest();
        }
    }
}
