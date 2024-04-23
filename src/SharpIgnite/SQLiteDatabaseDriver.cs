using System;
using System.Data;
using System.Data.SQLite;

namespace SharpIgnite
{
    
    public class SQLiteDatabaseDriver : BaseDatabaseDriver
    {
        public SQLiteDatabaseDriver() : this("")
        {
        }
        
        public SQLiteDatabaseDriver(string connectionString) : this(connectionString, new SqlQueryBuilder())
        {
        }
        
        public SQLiteDatabaseDriver(string connectionString, ISqlQueryBuilder queryBuilder)
        {
            this.ConnectionString = connectionString;
            this.QueryBuilder = queryBuilder;
        }
        
        public override IDbConnection CreateConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }
    }
}
