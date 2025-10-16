using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Winzer.Core.Types;
using Npgsql;
using Npgsql.NameTranslation;


namespace Winzer.Repo
{
    [SuppressMessage("ReSharper", "PartialMethodWithSinglePart")]
    public class WinzerDBContext : DbContext
    {
        static WinzerDBContext() {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<BrandEnum>("brand_enum",new NpgsqlNullNameTranslator());
        }

        public WinzerDBContext()
        {
        }

        public WinzerDBContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<ProductMap> ProductMap { get; set; }
        public virtual DbSet<BulkPricing> BulkPricing { get; set; }
        public virtual DbSet<ContractPricing> ContractPricing { get; set; }
        public virtual DbSet<TemplatePricing> TemplatePricing { get; set; }
        public virtual DbSet<LastPurchasePricing> LastPurchasePricing { get; set; }
        public virtual DbSet<VariantMap> VariantMap { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<BrandEnum>("public", "brand_enum");
            base.OnModelCreating(modelBuilder);
        }
    }
}
