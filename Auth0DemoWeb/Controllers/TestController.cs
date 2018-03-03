using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Auth0DemoWeb.Controllers
{
	[Authorize]
	public class TestController : Controller
    {
		private static string accessToken;
		private static string refreshToken;
		private static HttpClient Client = new HttpClient();

        public IActionResult Index()
        {
            return View();
        }
		
		public async Task<IActionResult> GetAllValues()
		{
			await SetupAuthorizationHeader();
			var response  = await Client.GetAsync("http://localhost:13826/api/values");
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadAsStringAsync();

			return View(nameof(Index));
		}

		public async Task<IActionResult> GetById()
		{
			await SetupAuthorizationHeader();
			var response = await Client.GetAsync("http://localhost:13826/api/values/2");
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadAsStringAsync();

			return View(nameof(Index));
		}

		public async Task<IActionResult> Create()
		{
			await SetupAuthorizationHeader();
			
			var response = await Client.PostAsync(
				"http://localhost:13826/api/values", 
				 new StringContent(JsonConvert.SerializeObject(GetSamplePayload()), 
				 Encoding.UTF8, 
				 "application/json")
			);

			response.EnsureSuccessStatusCode();

			return View(nameof(Index));
		}

		public async Task<IActionResult> Update()
		{
			await SetupAuthorizationHeader();
			var response = await Client.PutAsync(
				"http://localhost:13826/api/values",
				new StringContent(JsonConvert.SerializeObject(GetSamplePayload()), 
				Encoding.UTF8, 
				"application/json"));

			response.EnsureSuccessStatusCode();

			return View(nameof(Index));
		}

		public async Task<IActionResult> Delete()
		{
			await SetupAuthorizationHeader();
			var response = await Client.DeleteAsync("http://localhost:13826/api/values/1");
			response.EnsureSuccessStatusCode();

			return View(nameof(Index));
		}

		private async Task SetupAuthorizationHeader()
		{
			//used only to prove that we can acquire the rerfesh token
			if(string.IsNullOrEmpty(refreshToken))
			{
				refreshToken = await HttpContext.GetTokenAsync("refresh_token");
			}

			if(string.IsNullOrEmpty(accessToken))
			{
				accessToken = await HttpContext.GetTokenAsync("access_token");
			}

			if (Client.DefaultRequestHeaders.Authorization == null)
			{
				Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			}
		}

		private Payload GetSamplePayload()
		{
			return new Payload
			{
				id = DateTime.Now.Millisecond,
				Value = $"value-{DateTime.Now.Ticks}"
			};
		}
    }
}
