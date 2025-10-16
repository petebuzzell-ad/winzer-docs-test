using Winzer.Core.Types;

namespace Winzer.Core.Services
{
    public interface IVariantMapService
    {
        VariantMap CreateVariantMap(VariantMap variantMap);
        VariantMap UpdateVariantMap(VariantMap variantMap);
        VariantMap GetVariantMap(BrandEnum brandId, string oracleID);
        IEnumerable<VariantMap> GetAllVariantMaps(BrandEnum brandId);
    }
}
