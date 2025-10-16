using Winzer.Core.Repo;
using Winzer.Core.Types;
using Microsoft.Extensions.Options;
using Winzer.Repo;

namespace Winzer.Repo
{
    public class VariantMapRepo : RepoBase, IVariantMapRepo
    {
        public VariantMapRepo(IOptionsMonitor<RepoOptions> options) : base(options)
        {
        }

        public VariantMap CreateVariantMap(VariantMap map)
        {
            using var context = GetWinzerDBContext();

            context.VariantMap.Add(map);

            context.SaveChanges();

            return map;
        }

        public VariantMap UpdateVariantMap(VariantMap map)
        {
            using var context = GetWinzerDBContext();

            map.UtcUpdated = DateTime.UtcNow;
            context.VariantMap.Update(map);

            context.SaveChanges();

            return map;
        }

        public VariantMap? GetVariantMap(BrandEnum brandId, string oracleID)
        {
            using var context = GetWinzerDBContext();

            var map = context.VariantMap
                .SingleOrDefault(p => p.OracleID == oracleID && p.BrandId == brandId);

            return map;
        }

        public IEnumerable<VariantMap> GetAllVariantMaps(BrandEnum brandId)
        {
            using var context = GetWinzerDBContext();
            var map = context.VariantMap.Where(m => m.BrandId == brandId);
            return map.ToList();
        }
    }
}
