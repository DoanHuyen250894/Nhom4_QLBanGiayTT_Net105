using AppData.IServices;
using AppData.Models;
using AppData.Services;
using AppView.IServices;
using AppView.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ErrorViewModel = AppView.Models.ErrorViewModel;

namespace AppView.Controllers
{
	public class HomeController : Controller
	{
		private readonly ShopDBContext _shopDBContext;
		private readonly IShoesDetailsService _shoesDT;
		private readonly IProductService _product;
		private readonly IImageService _image;
		private readonly ISizeService _size;
		private readonly ISupplierService _supplier;
		private readonly IStyleService _style;
		private readonly IColorService _color;

		public HomeController()
		{
			_shopDBContext = new ShopDBContext();
			_shoesDT = new ShoesDetailsService();
			_product = new ProductService();
			_image = new ImageService();
			_size = new SizeService();
			_supplier = new SupplierService();
			_style = new StyleService();
			_color = new ColorService();
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		public IActionResult ListProduct()
		{
			var shoesList = _shoesDT.GetAllShoesDetails();
			foreach (var shoes in shoesList)
			{
				//kiểm tra trong db có thg image nào đã chứa thằng ShoesDetails tương ứng chưa, nếu tồn tại rồi gán cho thuộc tính imageUrl giá trị của link ảnh đó
				var firstImage = _image.GetAllImages().FirstOrDefault(c => c.ShoesDetailsID == shoes.ShoesDetailsId);
				if (firstImage != null)
				{
					shoes.ImageUrl = firstImage.Image1;
				}
				var nameProduct = _product.GetAllProducts().FirstOrDefault(c => c.ProductID == shoes.ProductID);
				if (nameProduct != null)
				{
					ViewBag.NameSP = nameProduct.Name;
				}
			}
			ViewBag.shoesList = shoesList;
			return View();
		}

		public IActionResult DetailsProduct(Guid id)
		{
			var ShoesDT = _shoesDT.GetAllShoesDetails().FirstOrDefault(c => c.ShoesDetailsId == id);
			var NameProduct = _product.GetAllProducts().FirstOrDefault(c => c.ProductID == ShoesDT.ProductID);
			if (NameProduct != null)
			{
				ViewBag.nameProduct = NameProduct.Name;
			}
			var ImageGoldens = _image.GetAllImages().FirstOrDefault(c => c.ShoesDetailsID == id);
			ViewBag.ImageGolden1 = ImageGoldens.Image1;
			ViewBag.ImageGolden2 = ImageGoldens.Image2;
			ViewBag.ImageGolden3 = ImageGoldens.Image3;
			ViewBag.ImageGolden4 = ImageGoldens.Image4;
			return View(ShoesDT);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public IActionResult SexFilter()
		{
			return View("ListProduct");
		}

		[HttpPost]
		public IActionResult SexFilter(string[] genders)
		{
			if (genders != null && genders.Length > 0)
			{
				var combinedShoesList = new List<ShoesDetails>();
				var comparingShoesList = _shoesDT.GetAllShoesDetails();
				foreach (var shoes in comparingShoesList)
				{
					//kiểm tra trong db có thg image nào đã chứa thằng ShoesDetails tương ứng chưa, nếu tồn tại rồi gán cho thuộc tính imageUrl giá trị của link ảnh đó
					var firstImage = _image.GetAllImages().FirstOrDefault(c => c.ShoesDetailsID == shoes.ShoesDetailsId);
					if (firstImage != null)
					{
						shoes.ImageUrl = firstImage.Image1;
					}
					var nameProduct = _product.GetAllProducts().FirstOrDefault(c => c.ProductID == shoes.ProductID);
					if (nameProduct != null)
					{
						ViewBag.NameSP = nameProduct.Name;
					}
				}
				if (genders.Contains("male"))
				{
					var sizesList = _size.GetAllSizes().Where(x => x.Status == 1);
					List<ShoesDetails> maleShoesList = comparingShoesList.Where(shoes => sizesList.Any(size => size.SizeID == shoes.SizeID)).ToList();
					combinedShoesList.AddRange(maleShoesList);
				}

				if (genders.Contains("female"))
				{
					var sizesList = _size.GetAllSizes().Where(x => x.Status == 2);
					List<ShoesDetails> maleShoesList = comparingShoesList.Where(shoes => sizesList.Any(size => size.SizeID == shoes.SizeID)).ToList();
					combinedShoesList.AddRange(maleShoesList);
				}

				if (genders.Contains("unisex"))
				{
					var sizesList = _size.GetAllSizes().Where(x => x.Status == 0);
					List<ShoesDetails> maleShoesList = comparingShoesList.Where(shoes => sizesList.Any(size => size.SizeID == shoes.SizeID)).ToList();
					combinedShoesList.AddRange(maleShoesList);
				}
				ViewBag.shoesList = combinedShoesList;
				return View("ListProduct");
			}
			else
			{
				return RedirectToAction("ListProduct");
			}
		}

		public IActionResult SizeFilter()
		{
			return View("ListProduct");
		}

		[HttpPost]
		public IActionResult SizeFilter(string[] sizes)
		{
			if (sizes != null && sizes.Length > 0)
			{
				var combinedShoesList = new List<ShoesDetails>();
				var comparingShoesList = _shoesDT.GetAllShoesDetails();
				foreach (var shoes in comparingShoesList)
				{
					//kiểm tra trong db có thg image nào đã chứa thằng ShoesDetails tương ứng chưa, nếu tồn tại rồi gán cho thuộc tính imageUrl giá trị của link ảnh đó
					var firstImage = _image.GetAllImages().FirstOrDefault(c => c.ShoesDetailsID == shoes.ShoesDetailsId);
					if (firstImage != null)
					{
						shoes.ImageUrl = firstImage.Image1;
					}
					var nameProduct = _product.GetAllProducts().FirstOrDefault(c => c.ProductID == shoes.ProductID);
					if (nameProduct != null)
					{
						ViewBag.NameSP = nameProduct.Name;
					}
				}
				for (int size = 35; size <= 49; size++)
				{
					if (sizes.Contains(size.ToString()))
					{
						var sizesList = _size.GetAllSizes().Where(x => x.Name == size.ToString());
						List<ShoesDetails> maleShoesList = comparingShoesList.Where(shoes => sizesList.Any(size => size.SizeID == shoes.SizeID)).ToList();
						combinedShoesList.AddRange(maleShoesList);
					}
				}

				ViewBag.shoesList = combinedShoesList;
				return View("ListProduct");
			}
			return RedirectToAction("ListProduct");
		}

		public IActionResult PriceFilter()
		{
			return View("ListProduct");
		}

		[HttpPost]
		public IActionResult PriceFilter(string[] minPrice)
		{
			var combinedShoesList = new List<ShoesDetails>();
			var comparingShoesList = _shoesDT.GetAllShoesDetails();

			foreach (var shoes in comparingShoesList)
			{
				// Check if the ShoesDetails has an associated image in the database
				var firstImage = _image.GetAllImages().FirstOrDefault(c => c.ShoesDetailsID == shoes.ShoesDetailsId);
				if (firstImage != null)
				{
					shoes.ImageUrl = firstImage.Image1;
				}

				// Check if the ShoesDetails has an associated product in the database
				var nameProduct = _product.GetAllProducts().FirstOrDefault(c => c.ProductID == shoes.ProductID);
				if (nameProduct != null)
				{
					ViewBag.NameSP = nameProduct.Name;
				}
			}

			foreach (var price in minPrice)
			{
				if (int.TryParse(price, out int minprice))
				{
					if (minprice == 0)
					{
						var shoesList = _shoesDT.GetAllShoesDetails().Where(x => x.Price > minprice && x.Price <= 1000000);
						combinedShoesList.AddRange(shoesList);
					}
					else if (minprice == 1001000)
					{
						var shoesList = _shoesDT.GetAllShoesDetails().Where(x => x.Price > minprice && x.Price <= 2700000);
						combinedShoesList.AddRange(shoesList);
					}
					else if (minprice == 2701000)
					{
						var shoesList = _shoesDT.GetAllShoesDetails().Where(x => x.Price > minprice && x.Price <= 3999000);
						combinedShoesList.AddRange(shoesList);
					}
					else if (minprice == 4000000)
					{
						var shoesList = _shoesDT.GetAllShoesDetails().Where(x => x.Price >= minprice);
						combinedShoesList.AddRange(shoesList);
					}
				}
			}

			ViewBag.shoesList = combinedShoesList;
			return View("ListProduct");
		}

		public IActionResult SupplierFilter()
		{
			return View("ListProduct");
		}

		[HttpPost]
		public IActionResult SupplierFilter(string[] brands)
		{
			if (brands != null && brands.Length > 0)
			{
				var combinedShoesList = new List<ShoesDetails>();
				var comparingShoesList = _shoesDT.GetAllShoesDetails();

				foreach (var shoes in comparingShoesList)
				{
					// Check if the ShoesDetails has an associated image in the database
					var firstImage = _image.GetAllImages().FirstOrDefault(c => c.ShoesDetailsID == shoes.ShoesDetailsId);
					if (firstImage != null)
					{
						shoes.ImageUrl = firstImage.Image1;
					}

					// Check if the ShoesDetails has an associated product in the database
					var nameProduct = _product.GetAllProducts().FirstOrDefault(c => c.ProductID == shoes.ProductID);
					if (nameProduct != null)
					{
						ViewBag.NameSP = nameProduct.Name;
					}
				}

				var brandList = _supplier.GetAllSuppliers().Where(suppliers => brands.Contains(suppliers.Name.ToLower()));
				var filteredShoesList = comparingShoesList.Where(shoes => brandList.Any(brand => brand.SupplierID == shoes.SupplierID));
				combinedShoesList.AddRange(filteredShoesList);

				ViewBag.shoesList = combinedShoesList;
				return View("ListProduct");
			}
			return RedirectToAction("ListProduct");
		}

		public IActionResult StyleFilter()
		{
			return View("ListProduct");
		}


		[HttpPost]
		public IActionResult StyleFilter(string[] styles)
		{
			if (styles != null && styles.Length > 0)
			{
				var combinedShoesList = new List<ShoesDetails>();
				var comparingShoesList = _shoesDT.GetAllShoesDetails();

				foreach (var shoes in comparingShoesList)
				{
					// Check if the ShoesDetails has an associated image in the database
					var firstImage = _image.GetAllImages().FirstOrDefault(c => c.ShoesDetailsID == shoes.ShoesDetailsId);
					if (firstImage != null)
					{
						shoes.ImageUrl = firstImage.Image1;
					}

					// Check if the ShoesDetails has an associated product in the database
					var nameProduct = _product.GetAllProducts().FirstOrDefault(c => c.ProductID == shoes.ProductID);
					if (nameProduct != null)
					{
						ViewBag.NameSP = nameProduct.Name;
					}
				}

				var stylesList = _style.GetAllStyles().Where(s => styles.Contains(s.Name.ToLower()));
				var filteredShoesList = comparingShoesList.Where(shoes => stylesList.Any(style => style.StyleID == shoes.StyleID));
				combinedShoesList.AddRange(filteredShoesList);
				ViewBag.shoesList = combinedShoesList;
				return View("ListProduct");
			}
			return RedirectToAction("ListProduct");
		}

		public IActionResult ColorFilter()
		{
			return View("ListProduct");
		}

		[HttpPost]
		public IActionResult ColorFilter(string[] colors)
		{
			if (colors != null || colors.Length > 0)
			{
				var combinedShoesList = new List<ShoesDetails>();
				var comparingShoesList = _shoesDT.GetAllShoesDetails();

				foreach (var shoes in comparingShoesList)
				{
					// Check if the ShoesDetails has an associated image in the database
					var firstImage = _image.GetAllImages().FirstOrDefault(c => c.ShoesDetailsID == shoes.ShoesDetailsId);
					if (firstImage != null)
					{
						shoes.ImageUrl = firstImage.Image1;
					}

					// Check if the ShoesDetails has an associated product in the database
					var nameProduct = _product.GetAllProducts().FirstOrDefault(c => c.ProductID == shoes.ProductID);
					if (nameProduct != null)
					{
						ViewBag.NameSP = nameProduct.Name;
					}
				}

				var colorsList = _color.GetAllColors().Where(s => colors.Contains(s.Name.ToLower()));
				var filteredShoesList = comparingShoesList.Where(shoes => colorsList.Any(color => color.ColorID == shoes.ColorID));
				combinedShoesList.AddRange(filteredShoesList);
				ViewBag.shoesList = combinedShoesList;
				return View("ListProduct");
			}
			return RedirectToAction("ListProduct");
		}

	}
}