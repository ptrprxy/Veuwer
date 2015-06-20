using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Veuwer.Models;
using Veuwer.Utility;

namespace Veuwer.Controllers
{
    public class HomeController : Controller
    {
        const int sizeLimit = 10485760;

        IAmazonS3 s3;
        DefaultContext db = new DefaultContext();
        MD5 md5 = MD5.Create();

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            string[] keys = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/awskeys.txt")).Split(',');
            s3 = AWSClientFactory.CreateAmazonS3Client(keys[0], keys[1], RegionEndpoint.USWest2);

            return base.BeginExecuteCore(callback, state);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(object image)
        {
            string filename = "";
            MemoryStream stream = null;

            if (image is HttpPostedFileBase[])
            {
                var file = ((HttpPostedFileBase[])image)[0];
                if (file.ContentLength > sizeLimit)
                    return Fail(filename + " is too large");

                filename = file.FileName;
                stream = ImageManager.ProcessImage(file.InputStream);
            }
            else if (image is string[])
            {
                var url = ((string[])image)[0];

                filename = Path.GetFileName(url);
                if (string.IsNullOrWhiteSpace(filename))
                    filename = url;

                using (WebClient client = new WebClient())
                using (var webstream = client.OpenRead(url))
                {
                    int filesize = int.Parse(client.ResponseHeaders["Content-Length"]);
                    if (filesize > sizeLimit)
                        return Fail(filename + " is too large");

                    stream = ImageManager.ProcessImage(webstream);
                }
            }

            var newLink = CreateImageLink(stream);
            if (newLink == null)
                return Fail(filename + " does not use a valid format");

            db.ImageLinks.Add(newLink);

            bool sendToS3 = newLink.Image.Id == 0;
            db.SaveChanges();
            if (sendToS3)
            {
                try
                {
                    string key = Request.Url.Host + "/images/" + Encode(newLink.Image.Id) + ".png";
                    s3.PutObject(new PutObjectRequest()
                    {
                        Key = key,
                        InputStream = stream,
                        BucketName = "veuwer",
                        ContentType = newLink.Image.MimeType
                    });
                }
                catch (Exception)
                {
                    return Fail("Transferring " + filename + " to storage failed");
                }
            }

            stream.Dispose();
            return Json(new { status = "success", message = Encode(newLink.Id) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Images(string id)
        {
            var ids = id.Split(',');
            return View(ids);
        }

        [OutputCache(
            Duration = 3600,
            VaryByParam = "id",
            Location = OutputCacheLocation.ServerAndClient)]
        public ActionResult ImageDirect(string id)
        {
            var imgLinkId = Decode(id);
            var imgLink = db.ImageLinks.FirstOrDefault(x => x.Id == imgLinkId);
            if (imgLink == null)
                return HttpNotFound();

            using (var res = s3.GetObject("veuwer", Request.Url.Host + "/images/" + Encode(imgLink.Image.Id) + ".png"))
            using (Stream stream = res.ResponseStream)
                return File(StreamToByteArray(stream), imgLink.Image.MimeType);
        }

        static readonly byte[] pngsig = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };  // .PNG....
        static readonly byte[] jpgsig = new byte[] { 0xFF, 0xD8 };                                      // 0xFFD8
        static readonly byte[] gifsig1 = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };             // GIF87a
        static readonly byte[] gifsig2 = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };             // GIF89a
        ImageLink CreateImageLink(Stream stream)
        {
            byte[] imghead = new byte[8];
            stream.Read(imghead, 0, imghead.Length);

            stream.Position = 0;
            var hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
            var imgMatch = db.Images.FirstOrDefault(x => x.Hash == hash);
            if (imgMatch == null)
            {
                string mimeType = "";
                if (imghead.Take(pngsig.Length).SequenceEqual(pngsig))
                    mimeType = "image/png";
                else if (imghead.Take(jpgsig.Length).SequenceEqual(jpgsig))
                    mimeType = "image/jpeg";
                else if (imghead.Take(gifsig1.Length).SequenceEqual(gifsig1) || imghead.Take(gifsig2.Length).SequenceEqual(gifsig2))
                    mimeType = "image/gif";
                else
                    return null;

                imgMatch = new Image()
                {
                    Hash = hash,
                    MimeType = mimeType
                };
            }

            return new ImageLink() { Image = imgMatch };
        }

        byte[] StreamToByteArray(Stream stream, int maxsize = int.MaxValue)
        {
            byte[] data;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[2048];
                int bytesRead, totalRead = 0;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                    if (totalRead > maxsize)
                        return null;
                }
                data = ms.ToArray();
            }

            return data;
        }

        JsonResult Fail(string reason)
        {
            return Json(new { status = "failure", message = reason }, JsonRequestBehavior.AllowGet);
        }

        const string CharList = "0123456789abcdefghijklmnopqrstuvwxyz";
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