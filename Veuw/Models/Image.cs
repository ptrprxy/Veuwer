using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Veuw.Models
{
    public class Image
    {
        public long Id { get; set; }
        public string Uri { get; set; }
        public string Hash { get; set; }
        public string MimeType { get; set; }
    }
}