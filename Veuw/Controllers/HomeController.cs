using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using Veuw.Models;

namespace Veuw.Controllers
{
    public class HomeController : Controller
    {
        DefaultContext db = new DefaultContext();
        SHA256Managed sha = new SHA256Managed();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload()
        {
            try
            {
                var image = Request.Files[0];
                if (image.ContentLength > 0 && image.ContentLength < 2097152) //2 mb limit
                {
                    var hash = BitConverter.ToString(sha.ComputeHash(image.InputStream)).Replace("-", String.Empty);
                    var imgMatch = db.Images.FirstOrDefault(x => x.Hash == hash) ?? new Image() { Hash = hash };
                    var imgLink = new ImageLink() { Image = imgMatch };

                    var img = System.Drawing.Image.FromStream(image.InputStream);
                    image.InputStream.Seek(0, SeekOrigin.Begin);
                    if (ImageFormat.Jpeg.Equals(img.RawFormat) || ImageFormat.Png.Equals(img.RawFormat) || ImageFormat.Gif.Equals(img.RawFormat))
                    {
                        db.ImageLinks.Add(imgLink);
                        db.SaveChanges();

                        if (imgMatch.Uri == null)
                        {
                            var path = Path.Combine(Server.MapPath("~/Uploads/Images"), Encode(imgMatch.Id) + ".png");
                            img.Save(path);
                            imgMatch.Uri = path;
                            imgMatch.MimeType = image.ContentType;
                            db.SaveChanges();
                        }

                        Response.Redirect("/" + Encode(imgLink.Id));
                        return Json(new { status = "success" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch { }

            return Json(new { status = "failure" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Images(string id)
        {
            ViewBag.ImageUri = id;
            return View();
        }

        public ActionResult ImageDirect(string id)
        {
            var imgLinkId = Decode(id);
            var imgLink = db.ImageLinks.FirstOrDefault(x => x.Id == imgLinkId);
            if (imgLink == null)
                return HttpNotFound();

            var path = Path.Combine(Server.MapPath("~/Uploads/Images"), Encode(imgLink.Image.Id) + ".png");
            if (!System.IO.File.Exists(path))
                return HttpNotFound();

            return File(System.IO.File.OpenRead(path), imgLink.Image.MimeType);
        }

        private const string CharList = "0123456789abcdefghijklmnopqrstuvwxyz";
        string Encode(long input)
        {
            char[] clistarr = CharList.ToCharArray();
            var result = new Stack<char>();
            while (input != 0)
            {
                result.Push(clistarr[input % 36]);
                input /= 36;
            }
            return new string(result.ToArray());
        }

        long Decode(string input)
        {
            var reversed = input.ToLower().Reverse();
            long result = 0;
            int pos = 0;
            foreach (char c in reversed)
            {
                result += CharList.IndexOf(c) * (long)Math.Pow(36, pos);
                pos++;
            }
            return result;
        }
    }
}