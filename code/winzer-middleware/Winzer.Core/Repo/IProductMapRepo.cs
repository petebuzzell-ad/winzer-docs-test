using Winzer.Core.Types;

namespace Winzer.Core.Repo
{
    public interface IProductMapRepo
    {
        ProductMap CreateProductMap(ProductMap period);
        ProductMap UpdateProductMap(ProductMap period);
        ProductMap? GetProductMap(BrandEnum brandId, string oracleID);
        bool DeleteProductMap(BrandEnum brandId, string shopifyID);
        IEnumerable<ProductMap> GetAllProductMaps(BrandEnum brandId);
    }
}
