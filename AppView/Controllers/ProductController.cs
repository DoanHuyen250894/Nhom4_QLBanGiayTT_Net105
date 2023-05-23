using AppData.IRepositories;
using AppData.IServices;
using AppData.Models;
using AppData.Repositories;
using AppData.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AppView.Controllers
{
    public class ProductController : Controller
    {
        private readonly IAllRepositories<Product> _repos;
        ShopDBContext _dbContext = new ShopDBContext();
        DbSet<Product> _product;
      
        public ProductController()
        {
            
            _product = _dbContext.Products;
            AllRepositories<Product> all = new AllRepositories<Product>(_dbContext, _product);
            _repos = all;
        }
      //  public ProductController() { }
      
    
        public async Task<IActionResult> GetAllProduct()
        {
            string apiUrl = "https://localhost:7036/api/Product";
            var httpClient = new HttpClient(); // tạo ra để callApi
            var response = await httpClient.GetAsync(apiUrl);// Lấy dữ liệu ra
                                                             // Lấy dữ liệu Json trả về từ Api được call dạng string
            string apiData = await response.Content.ReadAsStringAsync();
            // Lấy kqua trả về từ API
            // Đọc từ string Json vừa thu được sang List<T>
            var images = JsonConvert.DeserializeObject<List<Product>>(apiData);
            return View(images);
        }
       
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
           /* string apiUrl = $"https://localhost:7036/api/Product/Create-Product??Name={product.Name}&Status={product.Status}";
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();
            
            var pro = JsonConvert.DeserializeObject<Product>(apiData);*/
            _repos.AddItem(product);
            return RedirectToAction("GetAllProduct");
        }
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            Product product = _repos.GetAll().FirstOrDefault( c => c.ProductID == id);
            return View(product);
        }
        public IActionResult Edit(Product product)
        {
            if (_repos.EditItem(product))
            {
                return RedirectToAction("GetAllProduct");
            }
            else return BadRequest();
           
        }
        public IActionResult Delete(Guid id)
        {
            var pro = _repos.GetAll().FirstOrDefault( c => c.ProductID == id);
            if (_repos.RemoveItem(pro))
            {
                return RedirectToAction("GetAllProduct");
            }
            else return BadRequest();
        }

        public IActionResult Details(Guid id)
        {
            ShopDBContext dBContext = new ShopDBContext();
            var pro = _repos.GetAll().FirstOrDefault( c => c.ProductID == id);
            return View(pro);
        }
    }
}
