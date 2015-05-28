using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Veuwer.Models
{
    public class ImageLink
    {
        public long Id { get; set; }
        public virtual Image Image { get; set; }
        public string VoatLink { get; set; }
    }
}