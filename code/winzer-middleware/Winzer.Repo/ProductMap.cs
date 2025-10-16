using Winzer.Core.Repo;
using Winzer.Core.Types;
using Microsoft.Extensions.Options;
using Winzer.Repo;

namespace Winzer.Repo
{
    public class ProductMapRepo : RepoBase, IProductMapRepo
    {
        public ProductMapRepo(IOptionsMonitor<RepoOptions> options) : base(options)
        {
        }

        public ProductMap CreateProductMap(ProductMap map)
        {
            using var context = GetWinzerDBContext();

            context.ProductMap.Add(map);

            context.SaveChanges();

            return map;
        }

        public ProductMap UpdateProductMap(ProductMap map)
        {
            using var context = GetWinzerDBContext();

            map.UtcUpdated = DateTime.UtcNow;
            context.ProductMap.Update(map);

            context.SaveChanges();

            return map;
        }

        public ProductMap? GetProductMap(BrandEnum brandId, string oracleID)
        {
            using var context = GetWinzerDBContext();

            var map = context.ProductMap    
                .SingleOrDefault(p => p.OracleID == oracleID && p.BrandId == brandId);

            return map;
        }

        public IEnumerable<ProductMap> GetAllProductMaps(BrandEnum brandId)
        {
            using var context = GetWinzerDBContext();

            return context.ProductMap.Where(p => p.BrandId == brandId).ToList();
        }

        public bool DeleteProductMap(BrandEnum brandId, string shopifyID)
        {
            using var context = GetWinzerDBContext();

            var map = context.ProductMap.SingleOrDefault(p => p.ShopifyID == shopifyID && p.BrandId == brandId);

            if (map != null)
            {
                context.ProductMap.Remove(map);
                context.SaveChanges();

                return true;
            }

            return false;
        }
    }
}
