using Winzer.Core.Repo;
using Winzer.Core.Types;

namespace Winzer.Core.Services
{
    public class VariantMapService : IVariantMapService
    {
        private readonly IVariantMapRepo _variantMapRepo;

        public VariantMapService(IVariantMapRepo variantMapRepo)
        {
            _variantMapRepo = variantMapRepo;
        }

        public VariantMap CreateVariantMap(VariantMap variantMap)
        {
            variantMap.UtcCreated = DateTime.UtcNow;
            variantMap.UtcUpdated = DateTime.UtcNow;
            return _variantMapRepo.CreateVariantMap(variantMap);
        }

        public VariantMap GetVariantMap(BrandEnum brandId, string oracleID)
        {
            return _variantMapRepo.GetVariantMap(brandId, oracleID);
        }

        public VariantMap UpdateVariantMap(VariantMap variantMap)
        {
            variantMap.UtcUpdated = DateTime.UtcNow;
            return _variantMapRepo.UpdateVariantMap(variantMap);
        }

        public IEnumerable<VariantMap> GetAllVariantMaps(BrandEnum brandId)
        {
            return _variantMapRepo.GetAllVariantMaps(brandId);
        }
    }
}
