using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth0DemoApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
		private static ConcurrentDictionary<int, string> data = new ConcurrentDictionary<int, string>();

		public ValuesController()
		{
			var user = User;
			if (data.Count() > 0)
			{
				return;
			}

			data.TryAdd(1, "value1");
			data.TryAdd(2, "value2");
			data.TryAdd(3, "value3");
		}

        [HttpGet]
		[Authorize(Roles = "admin,developer,guest")]
		public IActionResult Get()
        {
			return Ok(data.Values);
        }

        // GET api/values/5
        [HttpGet("{id}")]
		[Authorize(Roles = "admin,developer,guest")]
		public string Get(int id)
        {
            data.TryGetValue(id, out var value);
			return value;
        }

        // POST api/values
        [HttpPost]
		[Authorize(Roles = "admin,developer")]
		public void Post([FromBody] Payload payload)
        {
			data.TryAdd(payload.id, payload.Value);
        }

        // PUT api/values/5
        [HttpPut]
		[Authorize(Roles = "admin,developer")]
		public void Put([FromBody] Payload payload)
        {
			data.AddOrUpdate(payload.id, payload.Value, (k, v) => payload.Value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
		[Authorize(Roles = "admin")]
		public void Delete(int id)
        {
			data.Remove(id, out var value);
        }
    }
}
