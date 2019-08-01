using System.Collections.Generic;

namespace studo.Models.Responses.Migrations
{
    public class MigrationsView
    {
        public IEnumerable<string> AppliedMigrations { get; set; }
        public IEnumerable<string> Migrations { get; set; }
        public IEnumerable<string> PendingMigrations { get; set; }
    }
}
