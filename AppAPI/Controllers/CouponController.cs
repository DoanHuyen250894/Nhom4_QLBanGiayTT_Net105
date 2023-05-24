using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly IAllRepositories<Coupon> _repos;
        private ShopDBContext _dbContext = new ShopDBContext();
        private DbSet<Coupon> _coupon;
        public CouponController()
        {
            _coupon = _dbContext.Coupons;
            AllRepositories<Coupon> all = new AllRepositories<Coupon>(_dbContext, _coupon);
            _repos = all;
        }

        // GET: api/<CouponController1>
        [HttpGet("get-coupon")]
        public IEnumerable<Coupon> GetAll()
        {
            return _repos.GetAll();
        }

        // GET api/<CouponController1>/5
        //[HttpGet("find-customer")]
        //public IEnumerable<Customer> GetAll(string name)
        //{
        //    return _repos.GetAll().Where(c => c.UserName.ToLower().Contains(name.ToLower())).ToList();
        //}



        // POST api/<CouponController1>
        [HttpPost("create-coupon")]
        public string CreateCoupon(string CouponCode, decimal CouponValue, int MaxUsage, int RemainingUsage, DateTime ExpirationDate, int Status)
        {
            Coupon coupon = new Coupon();
            coupon.CouponCode = CouponCode;
            coupon.CouponValue = CouponValue;
            coupon.MaxUsage = MaxUsage;
            coupon.RemainingUsage = RemainingUsage;
            coupon.ExpirationDate = ExpirationDate;
            coupon.Status = Status;
            coupon.CouponID = Guid.NewGuid();

            if (_repos.AddItem(coupon))
            {
                return "Thêm thành công";
            }
            else
            {
                return "Error";
            }
        }


        // PUT api/<CouponController1>/5
        [HttpPut("update-coupon")]
        public string UpdateCoupon( Guid CouponID,string CouponCode, decimal CouponValue, int MaxUsage, int RemainingUsage, DateTime ExpirationDate, int Status)
        {
            var colo = _repos.GetAll().FirstOrDefault(c => c.CouponID == CouponID);

            if (_repos.EditItem(colo))
            {
                return "Sửa thành công";
            }
            else
            {
                return "Sửa thất bại";
            }
        }

        // DELETE api/<CouponController1>/5
        [HttpDelete("delete-coupon")]
        public bool Delete(Guid id)
        {
            var colo = _repos.GetAll().First(c => c.CouponID == id);
            return _repos.RemoveItem(colo);
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
