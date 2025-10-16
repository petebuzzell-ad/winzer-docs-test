using Winzer.Core.Types;

namespace Winzer.Core.Repo
{
    public interface IVariantMapRepo
    {
        VariantMap CreateVariantMap(VariantMap period);
        VariantMap UpdateVariantMap(VariantMap period);
        VariantMap? GetVariantMap(BrandEnum brandId, string oracleID);

        IEnumerable<VariantMap> GetAllVariantMaps(BrandEnum brandId);
    }
}
