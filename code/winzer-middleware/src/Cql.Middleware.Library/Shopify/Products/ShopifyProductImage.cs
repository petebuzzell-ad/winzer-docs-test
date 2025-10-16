using Cql.Middleware.Library.Shopify.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Cql.Middleware.Library.Shopify.Products
{
    public class ShopifyProductImage
    {
        public string? MediaId { get; set; }

        public string? ImageId { get; set; }

        public ShopifyProductImage()
        {
        }

        public int? Position
        {
            get;
            set;
        }

        public DateTimeOffset? CreatedAt
        {
            get;
            set;
        }

        public DateTimeOffset? UpdatedAt
        {
            get;
            set;
        }

        public string? Src
        {
            get;
            set;
        }

        public string? Filename
        {
            get;
            set;
        }

        public string? Attachment
        {
            get;
            set;
        }

        public IEnumerable<long>? VariantIds
        {
            get;
            set;
        }

        public int? Height
        {
            get;
            set;
        }

        public int? Width
        {
            get;
            set;
        }

        public string? Alt
        {
            get;
            set;
        }

        public IList<ShopifyMetaField>? Metafields
        {
            get;
            set;
        }

        public string? JustTheFileName {
            get
            {
                if (string.IsNullOrWhiteSpace(Src)) return null;
                return Path.GetFileName(new Uri(Src.Replace('+', ' ')).LocalPath).ToLower().Replace(".jpg", "");
            }
        }

        public bool IsSameImageAs(ShopifyProductImage other)
        {
            if (!string.IsNullOrWhiteSpace(MediaId) && !string.IsNullOrWhiteSpace(other.MediaId) && MediaId == other.MediaId)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(ImageId) && !string.IsNullOrWhiteSpace(other.ImageId) && ImageId == other.ImageId)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(JustTheFileName) && !string.IsNullOrWhiteSpace(other.JustTheFileName) && JustTheFileName == other.JustTheFileName)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(Alt) && !string.IsNullOrWhiteSpace(other.Alt) && Alt == other.Alt)
            {
                return true;
            }


            return false;
        }        
    }
}
