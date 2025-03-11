using System.Collections.Generic;

namespace PostgresqlDatabaseEqualizer.Models
{
  public interface IPostgresService
  {
    List<string> GetStoredProcedures();
  }
}
