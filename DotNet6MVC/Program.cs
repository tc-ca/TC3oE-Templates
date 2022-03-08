using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using DotNet6MVC.Helpers;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options => 
{
    options.AddSupportedCultures("en","fr");
    options.AddSupportedUICultures("en","fr");
    options.SetDefaultCulture("en");
    options.RequestCultureProviders = new[]{
        new UrlRequestCultureProvider { Options = options}
    };
});

builder.Services.AddControllersWithViews(options =>
{
    options.EnableEndpointRouting = true;
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddViewLocalization();

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.AllowedHosts.Add("myApplication.tc.gc.ca");
    options.KnownProxies.Add(IPAddress.Parse("55.55.55.555"));
});


// https://stackoverflow.com/a/70684379/11141271
// ensure the app redirects to the proper url after login
// this is needed when running behind the application gateway, the app service doesn't know the url we want to use
// this lets us use app services WITHOUT configuring custom domains.

if (!builder.Environment.IsDevelopment())
{
    builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        // options.SaveTokens = true; // this saves the token for the downstream api
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = async ctxt =>
            {
                ctxt.ProtocolMessage.RedirectUri = "https://myApplication.tc.gc.ca/myApp-monApp/signin-oidc";
                await Task.Yield();
            }
        };
    });
}

var app = builder.Build();

// Configure the app to work properly when running behind the application gateway
// https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-3.1#deal-with-path-base-and-proxies-that-change-the-request-path
app.UseForwardedHeaders();
// The app is reverse proxied at myApplication.tc.gc.ca/myApp-monApp
app.UsePathBase("/myApp-monApp");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(o =>
    {
        o.Run(ctx =>
        {
            var language = ctx.Request.Path.Value?.Length >= 3 ? ctx.Request.Path.Value.Substring(1, 2) : "en";
            if (language.Equals("en")) ctx.Response.Redirect($"{ctx.Request.PathBase.Value}/{language}/error");
            if (language.Equals("fr")) ctx.Response.Redirect($"{ctx.Request.PathBase.Value}/{language}/erreur");
            return Task.CompletedTask;
        });
    });
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{culture}/{controller}/{action}/{id?}"
);


app.MapRazorPages();

app.Run();
