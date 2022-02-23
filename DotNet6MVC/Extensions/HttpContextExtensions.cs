using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace DotNet6MVC.Extensions;

public static class HttpContextExtensions
{
	public static string GetCurrentCulture(this HttpContext Context)
	{
		return Context.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
		!.RequestCulture.Culture.TwoLetterISOLanguageName;
	}

	public static string GetFlippedLanguageLinkObject(this HttpContext Context, IUrlHelper Url)
	{
		var flippedCulture = Context.GetCurrentCulture() == "en" ? "fr" : "en";
		var flippedCultureName = flippedCulture == "fr" ? "Français" : "English";
		
		// Create new route data, set culture to flipped value
		var flippedCultureRouteData = new RouteData(Context.GetRouteData());
		flippedCultureRouteData.Values["culture"] = flippedCulture;
		
		// Construct URL using flipped localization
		var flippedCultureHref = Url.RouteUrl(flippedCultureRouteData.Values);
		// Persist query parameters
		for (var i = 0; i < Context.Request.Query.Count; i++)
		{
			var (key, value) = Context.Request.Query.ElementAt(i);
			if (i == 0) flippedCultureHref += "?";
			else flippedCultureHref += "&";
			flippedCultureHref += $"{key}={value}";
		}

		// Return WET JS object
		return $@"{{
			lang: ""{flippedCulture}"",
			href: ""{flippedCultureHref}"",
			text: ""{flippedCultureName}""
		}}";
	}
}