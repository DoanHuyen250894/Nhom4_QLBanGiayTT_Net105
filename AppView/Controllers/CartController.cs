using Microsoft.AspNetCore.Mvc;

namespace AppView.Controllers
{
	public class CartController : Controller
	{
		public IActionResult Cart()
		{
			return View();
		}
	}
}
