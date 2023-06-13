﻿using AppData.IServices;
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

		public IActionResult Filters()
		{
			return View("ListProduct");
		}

		[HttpPost]
		public IActionResult Filters(string[] genders, string[] minPrice, string[] brands, string[] styles, string[] colors)
		{
			if (genders != null && genders.Length > 0
				|| minPrice != null && minPrice.Length > 0
				|| brands != null && brands.Length > 0
				|| styles != null && styles.Length > 0
				|| colors != null && colors.Length > 0)
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

				// Filter giới tính
				if (genders.Contains("male"))
				{
					var sizesList = _size.GetAllSizes().Where(x => x.Status == 1).ToList();
					List<ShoesDetails> maleShoesList = comparingShoesList.Where(shoes => sizesList.Any(size => size.SizeID == shoes.SizeID)).ToList();
					AddRangeIfNotExist(combinedShoesList, maleShoesList);
				}

				if (genders.Contains("female"))
				{
					var sizesList = _size.GetAllSizes().Where(x => x.Status == 2).ToList();
					List<ShoesDetails> femaleShoesList = comparingShoesList.Where(shoes => sizesList.Any(size => size.SizeID == shoes.SizeID)).ToList();
					AddRangeIfNotExist(combinedShoesList, femaleShoesList);
				}

				if (genders.Contains("unisex"))
				{
					var sizesList = _size.GetAllSizes().Where(x => x.Status == 0).ToList();
					List<ShoesDetails> unisexShoesList = comparingShoesList.Where(shoes => sizesList.Any(size => size.SizeID == shoes.SizeID)).ToList();
					AddRangeIfNotExist(combinedShoesList, unisexShoesList);
				}

				//Filter khoảng giá
				foreach (var price in minPrice)
				{
					if (int.TryParse(price, out int minprice))
					{
						if (minprice == 0)
						{
							var shoesList = _shoesDT.GetAllShoesDetails().Where(x => x.Price > minprice && x.Price <= 1000000).ToList();
							AddRangeIfNotExist(combinedShoesList, shoesList);
						}
						else if (minprice == 1001000)
						{
							var shoesList = _shoesDT.GetAllShoesDetails().Where(x => x.Price > minprice && x.Price <= 2700000).ToList();
							AddRangeIfNotExist(combinedShoesList, shoesList);
						}
						else if (minprice == 2701000)
						{
							var shoesList = _shoesDT.GetAllShoesDetails().Where(x => x.Price > minprice && x.Price <= 3999000).ToList();
							AddRangeIfNotExist(combinedShoesList, shoesList);
						}
						else if (minprice == 4000000)
						{
							var shoesList = _shoesDT.GetAllShoesDetails().Where(x => x.Price >= minprice).ToList();
							AddRangeIfNotExist(combinedShoesList, shoesList);
						}
					}
				}

				//Filter hãng
				var brandList = _supplier.GetAllSuppliers().Where(suppliers => brands.Contains(suppliers.Name.ToLower())).ToList();
				var filteredBrandsList = comparingShoesList.Where(shoes => brandList.Any(brand => brand.SupplierID == shoes.SupplierID)).ToList();
				AddRangeIfNotExist(combinedShoesList, (List<ShoesDetails>)filteredBrandsList);

				//Filter style
				var stylesList = _style.GetAllStyles().Where(s => styles.Contains(s.Name.ToLower())).ToList();
				var filteredStylesList = comparingShoesList.Where(shoes => stylesList.Any(style => style.StyleID == shoes.StyleID)).ToList();
				AddRangeIfNotExist(combinedShoesList, (List<ShoesDetails>)filteredStylesList);

				//Filter màu sắc
				var colorsList = _color.GetAllColors().Where(s => colors.Contains(s.Name.ToLower())).ToList();
				var filteredColorsList = comparingShoesList.Where(shoes => colorsList.Any(color => color.ColorID == shoes.ColorID)).ToList();
				AddRangeIfNotExist(combinedShoesList, (List<ShoesDetails>)filteredColorsList);

				//Tổng hợp lịa thành 1 list và gửi lên ViewBag
				ViewBag.shoesList = combinedShoesList;
				return View("ListProduct");
			}
			return RedirectToAction("ListProduct");
		}

		//Lọc xem sản phẩm đã có trong combinedShoesList chưa, nếu chưa thì thêm vào
		private void AddRangeIfNotExist(List<ShoesDetails> targetList, List<ShoesDetails> sourceList)
		{
			foreach (var item in sourceList)
			{
				if (!targetList.Contains(item))
				{
					targetList.Add(item);
				}
			}
		}
	}
}