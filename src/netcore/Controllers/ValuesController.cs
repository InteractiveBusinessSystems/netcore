using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NetCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NetCore.Controllers
{
    [Authorize(Policy = "UserManagement"), Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private MyContext _context;
        ILogger<ValuesController> _logger;
        public ValuesController(MyContext context, ILogger<ValuesController> logger)
        {
            _logger = logger;
            
            _context = context;
        }
        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            _logger.LogError("this is an error");
            var classes = await _context.Classes
                .Include(y => y.Students)
                .ToListAsync();
            var cl = User.Claims.ToList();
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
