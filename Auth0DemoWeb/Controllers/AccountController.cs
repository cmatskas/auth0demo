using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth0DemoWeb.Controllers
{
    public class AccountController : Controller
    {
		public async Task Login(string returnUrl = "/")
		{
			await HttpContext.ChallengeAsync("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl });
		}

		//[Authorize(Roles ="admin")]
		[Authorize]
		public IActionResult Claims()
		{
			var test = HttpContext.User.IsInRole("developer");
			return View();
		}

		[Authorize]
		public async Task Logout()
		{
			await HttpContext.SignOutAsync("Auth0", new AuthenticationProperties
			{
				RedirectUri = Url.Action("Index", "Home")
			});
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		}

	}
}
