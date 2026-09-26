using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;

namespace TechStackLearningHub.Web
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            // No AutoRedirectMode here on purpose. RedirectMode.Permanent made every
            // ".aspx" request answer 301 with the extensionless URL, which (a) cost a
            // round trip per page, (b) silently dropped form POST bodies aimed at a
            // ".aspx" URL, so the login button only worked after the redirect, and
            // (c) rewrote FormsAuthentication returnUrl values. Friendly URLs stay
            // available for extensionless requests, they just stop redirecting.
            var settings = new FriendlyUrlSettings();
            routes.EnableFriendlyUrls(settings);
        }
    }
}
