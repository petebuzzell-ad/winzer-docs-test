using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PetaPoco;
using PetaPoco.Providers;

namespace Winzer.Repo
{
    public class RepoBase
    {
        private readonly IOptionsMonitor<RepoOptions> _options;

        public RepoBase(IOptionsMonitor<RepoOptions> options)
        {
            _options = options;

            ContextOptions = new DbContextOptionsBuilder<WinzerDBContext>()
                .UseNpgsql(ConnectionString,
                              opt => opt.EnableRetryOnFailure())
                .Options;
        }

        private string ConnectionString => _options.CurrentValue.WinzerDBConnectionString;

        private DbContextOptions<WinzerDBContext> ContextOptions { get; set; }

        protected virtual WinzerDBContext GetWinzerDBContext()
        {
            return new WinzerDBContext(ContextOptions);
        }

        protected IDatabase GetDatabase()
        {
            var db = GetWinzerDBContext();
            return PetaPoco.DatabaseConfiguration
                .Build()
                .UsingProvider<PetaPoco.Providers.PostgreSQLDatabaseProvider>()
                .UsingConnectionString(db.Database.GetConnectionString())
                .Create();
        }
    }
}
