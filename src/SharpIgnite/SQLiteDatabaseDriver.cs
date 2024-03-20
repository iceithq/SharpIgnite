using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using SharpIgnite;

namespace SharpIgnite
{
    public abstract class AbstractDatabaseDriver : IDatabaseDriver
    {
        public string ConnectionString { get; set; }
        public Database Database { get; set; }
        ISqlQueryBuilder queryBuilder;
        
        public ISqlQueryBuilder QueryBuilder {
            get { return queryBuilder; }
            set { queryBuilder = value; }
        }
        
        public AbstractDatabaseDriver(string connectionString, ISqlQueryBuilder queryBuilder)
        {
            this.ConnectionString = connectionString;
            this.QueryBuilder = queryBuilder;
        }
        
        public int Insert(string tableName, Dictionary<string, object> data)
        {
            string query = queryBuilder.Insert(this.Database, tableName, data);
            return ExecuteNonQuery(query);
        }
        
        public int Insert<T>(string tableName, List<T> data)
        {
            string query = queryBuilder.Insert<T>(this.Database, tableName, data);
            Database.LastQuery = query;
            return ExecuteNonQuery(query);
        }
        
        public int Insert<T>(string tableName, T data)
        {
            string query = queryBuilder.Insert<T>(this.Database, tableName, data);
            Database.LastQuery = query;
            Database.insertedId = ExecuteScalar<int>(query);
            return Database.insertedId;
        }
        
        public int InsertedId()
        {
            return Database.insertedId;
        }
        
        public List<T> Result<T>()
        {
            string query = queryBuilder.Result(this.Database);
            
            List<T> results = new List<T>();
            using (var rs = ExecuteReader(query)) {
                while (rs.Read()) {
                    T item = CreateItem<T>(rs);
                    results.Add(item);
                }
            }
            return results;
        }
        
        public T Row<T>()
        {
            string query = queryBuilder.Row(this.Database);
            
            T item = default(T);
            using (var rs = ExecuteReader(query)) {
                if (rs.Read()) {
                    item = CreateItem<T>(rs);
                }
            }
            return item;
        }
        
        T CreateItem<T>(IDataReader rs)
        {
            T item = Activator.CreateInstance<T>();
            Type type = typeof(T);
            foreach (PropertyInfo property in type.GetProperties()) {
                try {
                    string propertyName;
                    if (property.GetCustomAttribute<ColumnAttribute>() != null) {
                        ColumnAttribute column = (ColumnAttribute)property.GetCustomAttribute(typeof(ColumnAttribute));
                        if (column.Name != null) {
                            propertyName = column.Name;
                        } else {
                            propertyName = property.Name;
                        }
                        var index = rs.GetOrdinal(propertyName);
                        object value = rs[index];
                        if (value == DBNull.Value) {
                            if (property.PropertyType == typeof(string)) {
                                property.SetValue(item, null);
                            } else if (property.PropertyType.IsValueType) {
                                property.SetValue(item, Activator.CreateInstance(property.PropertyType));
                            }
                        } else {
                            if (property.PropertyType == typeof(int) && value.GetType() == typeof(Int64)) {
                                // Convert Int64 to int
                                // HACK: For default int in SQLite that's Int64
                                property.SetValue(item, Convert.ToInt32((Int64)value));
                            } else {
                                property.SetValue(item, value);
                            }
                        }
                    }
                } catch (IndexOutOfRangeException ex) {}
            }
            return item;
        }
        
        public int Count(string tableName)
        {
            string query = queryBuilder.Count(Database, tableName);
            return ExecuteScalar<int>(query);
        }
        
        public int Update(string tableName)
        {
            string query = queryBuilder.Update(Database, tableName);
            return ExecuteNonQuery(query);
        }
        
        public int Update<T>(string tableName, T data, string _where)
        {
            string query = queryBuilder.Update(Database, tableName, data, _where);
            return ExecuteNonQuery(query);
        }
        
        public int Delete(string tableName, string _where)
        {
            string query = queryBuilder.Delete(this.Database, tableName, _where);
            return ExecuteNonQuery(query);
        }
        
        public int Truncate(string tableName)
        {
            string query = queryBuilder.Truncate(this.Database, tableName);
            return ExecuteNonQuery(query);
        }
        
        public int QueryFromFile(string filePath)
        {
            string query = "";
            using (var sr = new StreamReader(filePath)) {
                query = sr.ReadToEnd();
                Database.LastQuery = query;
            }
            return ExecuteNonQuery(query);
        }
        
        IDataReader ExecuteReader(string query)
        {
            var connection = CreateConnection(ConnectionString);
            OpenConnection(connection);
            var cmd = connection.CreateCommand();
            cmd.CommandText = query;
            var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }
        
        T ExecuteScalar<T>(string query)
        {
            return ExecuteScalar<T>(query, default(T));
        }
        
        T ExecuteScalar<T>(string query, T defaultValue)
        {
            var connection = CreateConnection(ConnectionString);
            OpenConnection(connection);
            var cmd = connection.CreateCommand();
            cmd.CommandText = query;
            T returnValue = defaultValue;
            var o = cmd.ExecuteScalar();
            if (o != null && o != DBNull.Value) {
                returnValue = (T)Convert.ChangeType(o, typeof(T));
            }
            CloseConnection(connection);
            return returnValue;
        }
        
        int ExecuteNonQuery(string query)
        {
            var connection = CreateConnection(ConnectionString);
            OpenConnection(connection);
            //var cmd = new SqlCommand(query, connection);
            var cmd = connection.CreateCommand();
            cmd.CommandText = query;
            int rowsAffected = cmd.ExecuteNonQuery();
            CloseConnection(connection);
            return rowsAffected;
        }
        
        void OpenConnection(IDbConnection connection)
        {
            if (connection.State == ConnectionState.Closed) {
                connection.Open();
            }
        }
        
        void CloseConnection(IDbConnection connection)
        {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
                connection.Dispose();
            }
        }
        
        protected abstract IDbConnection CreateConnection(string connectionString);
    }
    
    public class SQLiteDatabaseDriver : AbstractDatabaseDriver
    {
        public SQLiteDatabaseDriver(string connectionString) : this(connectionString, new SQLiteQueryBuilder())
        {
        }
        
        public SQLiteDatabaseDriver(string connectionString, ISqlQueryBuilder queryBuilder) : base(connectionString, queryBuilder)
        {
        }
        
        protected override IDbConnection CreateConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }
    }

    public class SQLiteQueryBuilder : ISqlQueryBuilder
    {
        public string Insert(Database database, string tableName, Dictionary<string, object> data)
        {
            database.tableName = tableName;
            foreach (var d in data) {
                database.Data.Add(d.Key, d.Value);
            }
            string columns = "";
            string values = "";
            int i = 0;
            foreach (var k in database.Data.Keys) {
                if (i++ > 0) {
                    columns += ", ";
                    values +=  ", ";
                }
                columns += k;
                var value = database.Data[k];
                if (value is string) {
                    values += "'" + SqlSanitize(value.ToString()) + "'";
                } else {
                    values += value;
                }
            }
            string query = "INSERT INTO " + database.tableName + "(" + columns + ")" + endl() +
                "VALUES(" + values + ");" + endl() +
                "SELECT LAST_INSERT_ROWID();";
            database.LastQuery = query;
            return query;
        }
        
        public string Insert<T>(Database database, string tableName, T data)
        {
            database.tableName = tableName;
            Type type = data.GetType();
            string columns = "";
            string values = "";
            int i = 0;
            string primaryKeyColumnName = "";
            foreach (PropertyInfo property in type.GetProperties()) {
                if (property.GetCustomAttribute<ColumnAttribute>() != null) {
                    object value = property.GetValue(data);
                    if (value != null) {
                        if  (i > 0) {
                            columns += ", ";
                            values += ", ";
                        }
                        string columnName = "";
                        ColumnAttribute column = (ColumnAttribute)property.GetCustomAttribute(typeof(ColumnAttribute));
                        if (!column.IsPrimaryKey) {
                            i++;
                            columnName = string.IsNullOrEmpty(column.Name)
                                ? property.Name
                                : column.Name;
                            
                            columns += columnName;
                            if (value is string || value is DateTime || value is DateTime?) {
                                values += "'" + SqlSanitize(value.ToString()) + "'";
                            } else {
                                values += value;
                            }
                        } else {
                            primaryKeyColumnName = string.IsNullOrEmpty(column.Name)
                                ? property.Name
                                : column.Name;
                        }
                    }
                }
            }
            string primaryKeyColumn = string.IsNullOrEmpty(primaryKeyColumnName)
                ? ""
                : "SELECT LAST_INSERT_ROWID();" + endl();
            string query = "INSERT INTO " + tableName + "(" + columns + ")" + endl() +
                "VALUES(" + values  + ");" + endl() +
                primaryKeyColumn;
            return query;
        }
        
        public string Insert<T>(Database database, string tableName, List<T> data)
        {
            database.tableName = tableName;
            string query = "";
            foreach (var d in data) {
                string q = Insert<T>(database, tableName, d);
                query +=  q + endl();
            }
            database.LastQuery = query;
            return query;
        }
        
        public string Result(Database database)
        {
            string query = GetSelectClause(database) + endl() +
                "FROM " + database.tableName + endl() +
                GetJoinClausesString(database) + endl() +
                GetWhereClausesString(database);
            database.LastQuery = query;
            return query;
        }
        
        public string Row(Database database)
        {
            string query = GetSelectClause(database) + endl() +
                "FROM " + database.tableName + endl() +
                GetJoinClausesString(database) + endl() +
                GetWhereClausesString(database);
            database.LastQuery = query;
            return query;
        }
        
        public string Count(Database database, string tableName)
        {
            if (!string.IsNullOrEmpty(tableName)) {
                database.tableName = tableName;
            }
            string query = "SELECT COUNT(*)" + endl() +
                "FROM " + database.tableName + endl() +
                GetJoinClausesString(database) +
                GetWhereClausesString(database);
            database.LastQuery = query;
            return query;
        }
        
        string endl()
        {
            return Environment.NewLine;
        }
        
        public string Update(Database database, string tableName)
        {
            database.tableName = tableName;
            string query = "UPDATE " + database.tableName + " SET" + endl();
            int i  = 0;
            foreach (var d in database.Data) {
                if (i++ > 0) {
                    query += "," + endl();
                }
                var value = d.Value;
                query += d.Key + " = ";
                if (value is string)  {
                    query += "'" + value + "'";
                } else {
                    query += value;
                }
            }
            query += endl() + GetWhereClausesString(database);
            database.LastQuery = query;
            return query;
        }
        
        public string Update<T>(Database database, string tableName, T data, string _where)
        {
            database.tableName = tableName;
            database.whereClauses.Add(_where);
            Type type = data.GetType();
            string query = "UPDATE " + database.tableName + " SET" + endl();
            int i = 0;
            foreach (PropertyInfo property in type.GetProperties()) {
                if (property.GetCustomAttribute<ColumnAttribute>() != null) {
                    object value = property.GetValue(data);
                    if (value != null) {
                        if  (i > 0) {
                            query += ", " + endl();
                        }
                        string columnName = "";
                        ColumnAttribute column = (ColumnAttribute)property.GetCustomAttribute(typeof(ColumnAttribute));
                        if (!column.IsPrimaryKey) {
                            i++;
                            columnName = string.IsNullOrEmpty(column.Name)
                                ? property.Name
                                : column.Name;
                            
                            query += columnName + " = ";
                            if (value is string) {
                                query += "'" + SqlSanitize(value.ToString()) + "'";
                            } else {
                                query += value;
                            }
                        }
                    }
                }
            }
            query += endl() + GetWhereClausesString(database);
            database.LastQuery = query;
            return query;
        }
        
        public string Delete(Database database, string tableName, string _where)
        {
            database.tableName = tableName;
            database.whereClauses.Add(_where);
            string query = "DELETE FROM " + database.tableName + endl() +
                GetWhereClausesString(database);
            database.LastQuery = query;
            return query;
        }
        
        public string Truncate(Database database, string tableName)
        {
            string query = "TRUNCATE TABLE " + tableName + endl();
            database.LastQuery = query;
            return query;
        }
        
        string GetJoinClausesString(Database database)
        {
            string joinClause = "";
            foreach (var j in database.joinClauses) {
                joinClause += j.Type + " " + j.TableName + " ON " + j.Join + endl();
            }
            return joinClause;
        }
        
        string GetWhereClausesString(Database database)
        {
            string whereClause = "";
            if (database.whereClauses.Count > 0) {
                whereClause += "WHERE ";
            }
            int i = 0;
            foreach (var w in database.whereClauses) {
                if (i++ > 0) {
                    whereClause += "AND ";
                }
                whereClause += w + endl();
            }
            return whereClause;
        }
        
        string SqlSanitize(string value)
        {
            string sanitizedStr = value.Replace("'", "");
            return sanitizedStr;
        }
        
        string GetSelectClause(Database database)
        {
            var selectClause = "";
            if (database.selectClauses.Count > 0) {
                selectClause += "SELECT ";
            } else {
                selectClause += "SELECT * ";
            }
            int i = 0;
            foreach (var c in database.selectClauses) {
                if (i++ > 0) {
                    selectClause += ", ";
                }
                selectClause += c;
            }
            return selectClause;
        }
        
        string GetGroupByClauseString(string groupByClause)
        {
            return groupByClause != "" ? "GROUP BY " + groupByClause + endl() : "";
        }
    }
    
}
