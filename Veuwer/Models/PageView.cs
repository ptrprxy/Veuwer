using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veuwer.Models
{
    public class PageView
    {
        public long Id { get; set; }
        public string IP { get; set; }
        public DateTime Timestamp { get; set; }
        public string Page { get; set; }

        public static PageView FromRequest(HttpRequestBase request)
        {
            return new PageView()
            {
                IP = (request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.ServerVariables["REMOTE_ADDR"]).Split(',')[0].Trim(),
                Timestamp = DateTime.Now,
                Page = request.Path
            };
        }
    }
}