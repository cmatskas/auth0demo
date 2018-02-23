using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Auth0DemoWeb
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			})
			.AddCookie()
			.AddOpenIdConnect("Auth0", options =>
			{
				// Set the authority to your Auth0 domain
				options.Authority = $"https://{Configuration["Auth0:Domain"]}";

				// Configure the Auth0 Client ID and Client Secret
				options.ClientId = Configuration["Auth0:ClientId"];
				options.ClientSecret = Configuration["Auth0:ClientSecret"];

				options.ResponseType = "code";

				options.Scope.Clear();
				options.Scope.Add("openid");
				options.Scope.Add("profile");

				options.SaveTokens = true;
				options.CallbackPath = new PathString("/signin-auth0");
				options.ClaimsIssuer = "Auth0";

				options.TokenValidationParameters = new TokenValidationParameters
				{
					NameClaimType = "name",
					RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/roles"
				};

				options.Events = new OpenIdConnectEvents
				{
					OnRedirectToIdentityProvider = context =>
					{
						context.ProtocolMessage.SetParameter("audience", @"http://auth0demoApi");
						return Task.FromResult(0);
					},
					// handle the logout redirection 
					OnRedirectToIdentityProviderForSignOut = (context) =>
				    {
					   var logoutUri = $"https://{Configuration["Auth0:Domain"]}/v2/logout?client_id={Configuration["Auth0:ClientId"]}";

					   var postLogoutUri = context.Properties.RedirectUri;
					   if (!string.IsNullOrEmpty(postLogoutUri))
					   {
						   if (postLogoutUri.StartsWith("/"))
						   {
							   var request = context.Request;
							   postLogoutUri = $"{request.Scheme}://{request.Host}{request.PathBase}{postLogoutUri}";
						   }
						   logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
					   }

					   context.Response.Redirect(logoutUri);
					   context.HandleResponse();

					   return Task.CompletedTask;
				    }
				};
			});

			services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseBrowserLink();
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
