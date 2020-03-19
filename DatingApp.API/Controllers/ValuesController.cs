using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    [Authorize] // Chỗ này chúng ta thiết lập Authorize (tức là cần có quyền hạn tương ứng mới được phép truy cập vào)
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly DataContext _context;

        public ValuesController(DataContext context)
        {
            _context = context;
        }
        
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetValues()
        {
            var values = await _context.Values.ToListAsync();  //Lấy tất cả row của entity Values từ DataContext
            return Ok(values);
        }
        [AllowAnonymous]    // Chỗ này chúng ta thiết lập AllowAnonymous cho nên chúng ta có thể truy cập vào mà không cần có Quyền hạn tương ứng.
        [HttpGet("{id}")]
        public async Task<ActionResult> GetValue(int id)
        {
            var value = await _context.Values.FirstOrDefaultAsync(x => x.Id == id);    //Lấy từ entity Values của DataContext row có Id = id nhập vào từ URL
            return Ok(value);
        }
        [HttpPost]
        public void Post([FromBody] string value)
        {}
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {}
        [HttpDelete("{id}")]
        public void Delete(int id)
        {}


    }
}

