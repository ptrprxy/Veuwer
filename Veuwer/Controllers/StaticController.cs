using System.Web.Mvc;
using Veuwer.Models;

namespace Veuwer.Controllers
{
    public class StaticController : Controller
    {
        DefaultContext db = new DefaultContext();

        public ActionResult Transparency()
        {
            db.PageViews.Add(PageView.FromRequest(Request));
            db.SaveChanges();

            return View();
        }
    }
}