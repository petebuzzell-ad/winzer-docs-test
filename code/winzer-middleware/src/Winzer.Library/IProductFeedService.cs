using Winzer.Library.Salsify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winzer.Library
{
    public interface IProductFeedService
    {
        Task<bool> ImportEverything(bool skipHashCheck = false);

        Task DeleteAllProducts();

        // Task<bool> ImportUpdatedProducts();
    }

    public class ProductImportOptions
    {
        public String? CompanyName { get; set; }
    }
}
