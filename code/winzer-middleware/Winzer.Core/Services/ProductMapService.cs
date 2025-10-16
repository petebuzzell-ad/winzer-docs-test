using Winzer.Core.Repo;
using Winzer.Core.Types;

namespace Winzer.Core.Services
{
    public class ProductMapService : IProductMapService
    {
        private readonly IProductMapRepo _productMapRepo;

        public ProductMapService(IProductMapRepo productMapRepo)
        {
            _productMapRepo = productMapRepo;
        }

        public ProductMap CreateProductMap(ProductMap productMap)
        {
            productMap.UtcCreated = DateTime.UtcNow;
            productMap.UtcUpdated = DateTime.UtcNow;
            return _productMapRepo.CreateProductMap(productMap);
        }

        public IEnumerable<ProductMap> GetAllProductMaps(BrandEnum brandId)
        {
            return _productMapRepo.GetAllProductMaps(brandId);
        }

        public ProductMap GetProductMap(BrandEnum brandId, string oracleID)
        {
            return _productMapRepo.GetProductMap(brandId, oracleID);
        }

        public ProductMap UpdateProductMap(ProductMap productMap)
        {
            productMap.UtcUpdated = DateTime.UtcNow;
            return _productMapRepo.UpdateProductMap(productMap);
        }

        public bool DeleteProductMap(BrandEnum brandId, string shopifyID)
        {
            return _productMapRepo.DeleteProductMap(brandId, shopifyID);
        }
    }
}
