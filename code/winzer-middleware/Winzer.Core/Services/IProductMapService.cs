using Winzer.Core.Types;

namespace Winzer.Core.Services
{
    public interface IProductMapService
    {
        ProductMap CreateProductMap(ProductMap productMap);
        ProductMap UpdateProductMap(ProductMap productMap);
        ProductMap GetProductMap(BrandEnum brandId, string oracleID);

        IEnumerable<ProductMap> GetAllProductMaps(BrandEnum brandId);

        bool DeleteProductMap(BrandEnum brandId, string shopifyID);
    }
}
