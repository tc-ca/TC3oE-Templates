using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Globalization;
using Microsoft.Extensions.Options;
using DotNet6MVC.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// builder.Services.AddLocalization();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options => 
{
    options.AddSupportedCultures("en","fr");
    options.AddSupportedUICultures("en","fr");
    options.SetDefaultCulture("en");
    options.RequestCultureProviders = new[]{
        new UrlRequestCultureProvider { Options = options}
        // new Microsoft.AspNetCore.Localization.Routing.RouteDataRequestCultureProvider { Options = options}
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
// .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix);

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

var app = builder.Build();

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
// app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
// app.UseRequestLocalization(options =>
// {
//     options.SupportedCultures = new[]{
//         new CultureInfo("en"),
//         new CultureInfo("fr")
//     };
//     options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(options.SupportedCultures[0]);
//     options.RequestCultureProviders.Insert(0, new Microsoft.AspNetCore.Localization.Routing.RouteDataRequestCultureProvider { Options = options});
// });


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{culture}/{controller}/{action}/{id?}"
);


app.MapRazorPages();

app.Run();
