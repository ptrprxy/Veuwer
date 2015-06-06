namespace Veuwer.Models
{
    public class ImageLink
    {
        public long Id { get; set; }
        public virtual Image Image { get; set; }
    }
}