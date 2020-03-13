using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly DataContext _context;

        public ValuesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetValues()
        {
            var values = await _context.Values.ToListAsync();  //Lấy tất cả row của entity Values từ DataContext
            return Ok(values);
        }

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

