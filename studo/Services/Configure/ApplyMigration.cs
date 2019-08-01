using WebApp.Configure.Models.Configure.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using studo.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace studo.Services.Configure
{
    public class ApplyMigration : IConfigureWork
    {
        private readonly DatabaseContext dbContext;
        private readonly ILogger<ApplyMigration> logger;
        private int tryCount = 10;
        private TimeSpan tryPeriod = TimeSpan.FromSeconds(10);

        public ApplyMigration(DatabaseContext dbContext, ILogger<ApplyMigration> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task Configure()
        {
            try
            {
                var pending = await dbContext.Database.GetPendingMigrationsAsync();
                if (pending?.Count() != 0)
                    await dbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                if (tryCount == 0)
                    throw;

                logger.LogWarning(ex, "Error while apply migrations");
                tryCount--;
                await Task.Delay(tryPeriod);
                await Configure();
            }
        }
    }
}
