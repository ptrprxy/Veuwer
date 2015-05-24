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
            List<ImageLink> newLinks = new List<ImageLink>();
            for (int i = 0; i < Request.Files.Count; i++)
			{
                var image = Request.Files[i];
                if (image.ContentLength < 0 || image.ContentLength > 2097152) //2 mb limit
                    return Fail(image.FileName + " too large");

                var hash = BitConverter.ToString(sha.ComputeHash(image.InputStream)).Replace("-", String.Empty);
                var imgMatch = db.Images.FirstOrDefault(x => x.Hash == hash) ?? new Image() { Hash = hash };
                var imgLink = new ImageLink() { Image = imgMatch };

                var img = System.Drawing.Image.FromStream(image.InputStream);
                image.InputStream.Seek(0, SeekOrigin.Begin);
                if (!ImageFormat.Jpeg.Equals(img.RawFormat) && !ImageFormat.Png.Equals(img.RawFormat) && !ImageFormat.Gif.Equals(img.RawFormat))
                    return Fail(image.FileName + " is not a supported file");

                db.ImageLinks.Add(imgLink);
                newLinks.Add(imgLink);

                if (imgMatch.ImgBlob == null)
                {
                    imgMatch.ImgBlob = new byte[image.ContentLength];
                    image.InputStream.Read(imgMatch.ImgBlob, 0, image.ContentLength);
                    imgMatch.MimeType = image.ContentType;
                }
            }

            db.SaveChanges();
            return Json(new { status = "success", message = "/i/" + string.Join(",", newLinks.Select(x => Encode(x.Id))) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Images(string id)
        {
            var ids = id.Split(',');
            return View(ids);
        }

        public ActionResult ImageDirect(string id)
        {
            var imgLinkId = Decode(id);
            var imgLink = db.ImageLinks.FirstOrDefault(x => x.Id == imgLinkId);
            if (imgLink == null)
                return HttpNotFound();

            return File(imgLink.Image.ImgBlob, imgLink.Image.MimeType);
        }

        JsonResult Fail(string reason)
        {
            return Json(new { status = "failure", message = reason }, JsonRequestBehavior.AllowGet);
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