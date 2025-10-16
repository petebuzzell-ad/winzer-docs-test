using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winzer.Library.Salsify;

namespace Winzer.Library.Salsify
{
    public interface ISalsifyAPIService
    {
        public Task<Stream> CreateProductExport(SalsifyExportRequest req);

        public Task CreateProductImport(string importID);

        public Task UpdateProductProperty(string productId, string propertyName, string propertyValue);

        public Task<SalsifyProduct?> GetProductById(string productId);

        public Task UpdateProductsBatch(IEnumerable<SalsifyProduct> products);
    }

    public class SalsifyExportRequest
    {
        public string Filter { get; set; } = string.Empty;

        public string Format { get; set; } = "json";

        public bool IncludeAllColumns { get; set; } = true;

        public IList<string> PropertiesToExport { get; set; } = new List<string>();
    }
}
