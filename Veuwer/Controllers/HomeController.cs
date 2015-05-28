using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using Veuwer.Models;

namespace Veuwer.Controllers
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
        public ActionResult Upload(object image)
        {
            string filename = "";
            Stream stream = null;

            if (image is HttpPostedFileBase[])
            {
                var file = ((HttpPostedFileBase[])image)[0];
                filename = file.FileName;
                stream = file.InputStream;
            }
            else if (image is string[])
            {
                var url = ((string[])image)[0];

                filename = Path.GetFileName(url);
                if (string.IsNullOrWhiteSpace(filename))
                    filename = url;

                using (WebClient client = new WebClient())
                    stream = client.OpenRead(url);
            }

            var newLink = CreateImageLink(stream);
            if (newLink == null)
                return Fail(filename + " is too large or uses an invalid format");

            db.ImageLinks.Add(newLink);

            db.SaveChanges();
            return Json(new { status = "success", message = Encode(newLink.Id) }, JsonRequestBehavior.AllowGet);
        }

        byte[] pngsig = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };  // .PNG....
        byte[] jpgsig = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };                          // ÿØÿà
        byte[] gifsig1 = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };             // GIF87a
        byte[] gifsig2 = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };             // GIF89a
        ImageLink CreateImageLink(Stream stream)
        {
            byte[] imgdata;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[2048];
                int bytesRead, totalRead = 0;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                    if (totalRead > 2097152)
                        return null;
                }
                imgdata = ms.ToArray();
            }

            var hash = BitConverter.ToString(sha.ComputeHash(imgdata)).Replace("-", String.Empty);
            var imgMatch = db.Images.FirstOrDefault(x => x.Hash == hash) ?? new Image() { Hash = hash };
            var imgLink = new ImageLink() { Image = imgMatch };

            string mimeType = "";
            if (imgdata.Take(pngsig.Length).SequenceEqual(pngsig))
                mimeType = "image/png";
            else if (imgdata.Take(jpgsig.Length).SequenceEqual(jpgsig))
                mimeType = "image/jpeg";
            else if (imgdata.Take(gifsig1.Length).SequenceEqual(gifsig1) || imgdata.Take(gifsig2.Length).SequenceEqual(gifsig2))
                mimeType = "image/gif";
            else
                return null;

            if (imgMatch.ImgBlob == null)
            {
                imgMatch.ImgBlob = imgdata;
                imgMatch.MimeType = mimeType;
            }

            return imgLink;
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