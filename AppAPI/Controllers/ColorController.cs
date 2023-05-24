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
    public class ColorController : ControllerBase
    {
        private readonly IAllRepositories<Color> _repos;
        private ShopDBContext _dbContext = new ShopDBContext();
        private DbSet<Color> _color;
        // GET: api/<ColorController1>
       
        public ColorController()
        {
            _color = _dbContext.Colors;
            AllRepositories<Color> all = new AllRepositories<Color>(_dbContext, _color);
            _repos = all;
        }
        [HttpGet("get-color")]
       
        public IEnumerable<Color> GetAll()
        {
            return _repos.GetAll();
        }

        // GET api/<ColorController1>/5
        //[HttpGet("find-color")]
        //public IEnumerable<Color> GetAll(string name)
        //{
        //    return _repos.GetAll().Where(c => c.Name.ToLower().Contains(name.ToLower())).ToList();
        //}

        // POST api/<ColorController1>
        [HttpPost("create-color")]
        public string CreateColor(string Name, int Status)
        {
            Color color = new Color();
            color.Name = Name;
            color.Status = Status;
            color.ColorID = Guid.NewGuid();

            if (_repos.AddItem(color))
            {
                return "Thêm thành công";
            }
            else
            {
                return "Error";
            }
        }

        // PUT api/<ColorController1>/5
        [HttpPut("update-color")]
        public string UpdateColor(string Name, int Status, Guid ColorID)
        {
            var colo = _repos.GetAll().FirstOrDefault(c => c.ColorID == ColorID);

            if (_repos.EditItem(colo))
            {
                return "Sửa thành công";
            }
            else
            {
                return "Sửa thất bại";
            }
        }


        // DELETE api/<ColorController1>/5
        [HttpDelete("delete-color")]
        public bool Delete(Guid id)
        {
            var colo = _repos.GetAll().First(c => c.ColorID == id);
            return _repos.RemoveItem(colo);
        }
    }
}
