using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Veuwer
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.RouteExistingFiles = true;

            routes.MapRoute(
                name: "Image",
                url: "i/{id}",
                defaults: new { controller = "Home", action = "Images" }
            );

            routes.MapRoute(
                name: "ImagePng",
                url: "{id}.png",
                defaults: new { controller = "Home", action = "ImageDirect" }
            );

            routes.MapRoute(
                name: "ImageJpg",
                url: "{id}.jpg",
                defaults: new { controller = "Home", action = "ImageDirect" }
            );

            routes.MapRoute(
                name: "ImageJpeg",
                url: "{id}.jpeg",
                defaults: new { controller = "Home", action = "ImageDirect" }
            );

            routes.MapRoute(
                name: "ImageGif",
                url: "{id}.gif",
                defaults: new { controller = "Home", action = "ImageDirect" }
            );

            routes.MapRoute(
                name: "Image2",
                url: "{id}",
                defaults: new { controller = "Home", action = "Images" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
