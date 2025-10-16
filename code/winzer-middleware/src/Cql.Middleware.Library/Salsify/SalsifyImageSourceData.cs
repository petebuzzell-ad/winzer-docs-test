using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Salsify
{
    /// <summary>
    /// This class is used to help determine if the images associated with a product in Shopify have changed
    /// when comparing it to a Salsify product.  This is necessary because Shopify changes the image url
    /// and there is no way to know what the original image url was in Salsify unless you store that information
    /// somehow or another (which is what this class is for)
    /// </summary>
    public class SalsifyImageSourceData
    {
        public IList<string> OriginalImageSources { get; set; } = new List<string>();

        public IDictionary<string, string> SKUToOriginalSourceMapping { get; set; } = new Dictionary<string, string>();

        public override bool Equals(object? obj)
        {
            var other = obj as SalsifyImageSourceData;
            if (other == null) return false;

            if (!other.OriginalImageSources.SequenceEqual(OriginalImageSources)) return false;

            if (!other.SKUToOriginalSourceMapping.Keys.SequenceEqual(SKUToOriginalSourceMapping.Keys)) return false;

            foreach (var entry in SKUToOriginalSourceMapping)
            {
                if (other.SKUToOriginalSourceMapping[entry.Key] != entry.Value) return false;
            }

            return true;
        }
    }
}
