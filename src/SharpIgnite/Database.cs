using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace SharpIgnite
{
    public interface IDatabaseDriver
    {
        string ConnectionString { get; set; }
        Database Database { get; set; }
        ISqlQueryBuilder QueryBuilder { get;set; }
        int Insert(string tableName, Dictionary<string, object> data);
        int Insert<T>(string tableName, List<T> data);
        int Insert<T>(string tableName, T data);
        List<T> Result<T>();
        T Row<T>();
        int Count(string tableName);
        int Update(string tableName);
        int Update<T>(string tableName, T data, string _where);
        int Delete(string tableName, string _where);
        int Truncate(string tableName);
        int QueryFromFile(string filePath);
    }
    
    public class ColumnAttribute : Attribute
    {
        public string Name { get; set; }
        public bool IsPrimaryKey { get; set; }
        
        public ColumnAttribute(bool isPrimaryKey)
        {
            this.IsPrimaryKey = isPrimaryKey;
        }
        
        public ColumnAttribute(string name)
        {
            this.Name  = name;
        }
        
        public ColumnAttribute(string name, bool isPrimaryKey)
        {
            this.Name = name;
            this.IsPrimaryKey = isPrimaryKey;
        }
        
        public ColumnAttribute()
        {
        }
    }
    
    public class DatabaseEventArgs : EventArgs
    {
        public string Query { get; set; }
        
        public DatabaseEventArgs(string query)
        {
            this.Query = query;
        }
        
        public DatabaseEventArgs()
        {
        }
    }
    
    public class Database
    {
        internal string tableName;
        internal string groupByClause;
        internal List<JoinClause> joinClauses = new List<JoinClause>();
        internal List<string> selectClauses = new List<string>();
        internal List<string> whereClauses = new List<string>();
        public Dictionary<string, object> Data { get; set; }
        internal string lastQuery;
        internal int _limit;
        internal int insertedId;
        
        public string LastQuery {
            get { return lastQuery; }
            internal set {
                lastQuery = value;
                ClearQuery();
                OnQueryChanged(new DatabaseEventArgs(lastQuery));
            }
        }
        
        public event EventHandler<DatabaseEventArgs> QueryChanged;
        
        protected virtual void OnQueryChanged(DatabaseEventArgs e)
        {
            if (QueryChanged != null) {
                QueryChanged(this, e);
            }
        }
        
        IDatabaseDriver databaseDriver;
        
        public Database()
        {
            Data = new Dictionary<string, object>();
        }
        
        public Database(IDatabaseDriver databaseDriver) : this()
        {
            this.databaseDriver = databaseDriver;
            this.databaseDriver.Database = this;
        }
        
        public void Load(string connectionName)
        {
            this.databaseDriver.ConnectionString = ConfigurationManager.AppSettings[connectionName];
        }
        
        public Database Select(string selectClause)
        {
            this.selectClauses.Add(selectClause);
            return this;
        }
        
        public Database From(string tableName)
        {
            this.tableName = tableName;
            return this;
        }
        
        public Database Join(string tableName, string _join)
        {
            return Join(tableName, _join, "INNER JOIN");
        }
        
        public Database Join(string tableName, string _join, string type)
        {
            this.joinClauses.Add(new JoinClause(tableName, _join, type));
            return this;
        }
        
        public Database Where(string _where)
        {
            this.whereClauses.Add(_where);
            return this;
        }
        
        public Database Where(string columnName, object _value)
        {
            return Where(columnName + " = " + _value);
        }
        
        public Database GroupBy(string groupBy)
        {
            this.groupByClause = groupBy;
            return this;
        }
        
        public Database Set(string column, object value)
        {
            this.Data.Add(column, value);
            return this;
        }
        
        public Database Limit(int limit)
        {
            this._limit = limit;
            return this;
        }
        
        public Database Get(string tableName)
        {
            this.tableName = tableName;
            return this;
        }
        
        public Database GetWhere(string tableName, string _where)
        {
            this.tableName = tableName;
            this.whereClauses.Add(_where);
            return this;
        }
        
        internal void ClearQuery()
        {
            selectClauses.Clear();
            joinClauses.Clear();
            whereClauses.Clear();
        }
        
        public int Insert(string tableName, Dictionary<string, object> data)
        {
            return databaseDriver.Insert(tableName, data);
        }
        
        public int Insert<T>(string tableName, T data)
        {
            insertedId = databaseDriver.Insert<T>(tableName, data);
            return insertedId;
        }
        
        public int Insert<T>(string tableName, List<T> data)
        {
            return databaseDriver.Insert<T>(tableName, data);
        }
        
        public int InsertedId()
        {
            return insertedId;
        }
        
        public List<T> Result<T>()
        {
            return databaseDriver.Result<T>();
        }
        
        public T Row<T>()
        {
            return databaseDriver.Row<T>();
        }
        
        public int Count()
        {
            return Count("");
        }
        
        public int Count(string tableName)
        {
            return this.databaseDriver.Count(tableName);
        }
        
        public int Update(string tableName)
        {
            this.tableName = tableName;
            return databaseDriver.Update(tableName);
        }
        
        public int Update<T>(string tableName, T data, string _where)
        {
            this.tableName = tableName;
            return databaseDriver.Update(tableName, data, _where);
        }
        
        public int Delete(string tableName, string _where)
        {
            return databaseDriver.Delete(tableName, _where);
        }
        
        public int Truncate(string tableName)
        {
            return databaseDriver.Truncate(tableName);
        }
        
        public int QueryFromFile(string filePath)
        {
            return databaseDriver.QueryFromFile(filePath);
        }
        
        public class JoinClause
        {
            public string Type { get; set; }
            public string Join { get; set; }
            public string TableName { get; set; }
            
            public JoinClause(string tableName, string _join, string type)
            {
                this.TableName = tableName;
                this.Join = _join;
                this.Type = type;
            }
        }
    }
    
    public interface ISqlQueryBuilder
    {
        string Insert(Database database, string tableName, Dictionary<string, object> data);
        string Insert<T>(Database database, string tableName, List<T> data);
        string Insert<T>(Database database, string tableName, T data);
        string Row(Database database);
        string Result(Database database);
        string Count(Database database, string tableName);
        string Update(Database database, string tableName);
        string Update<T>(Database database, string tableName, T data, string _where);
        string Delete(Database database, string tableName, string _where);
        string Truncate(Database database, string tableName);
    }
    
    public class SqlQueryBuilder : ISqlQueryBuilder
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
                "OUTPUT INSERTED.ID" + endl() +
                "VALUES(" + values + ")" + endl();
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
                : "OUTPUT INSERTED." + primaryKeyColumnName + endl();
            string query = "INSERT INTO " + tableName + "(" + columns + ")" + endl() +
                primaryKeyColumn +
                "VALUES(" + values  + ")" + endl();
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
    
    public class SqlDatabaseDriver : IDatabaseDriver
    {
        public string ConnectionString { get; set; }
        public Database Database { get; set; }
        ISqlQueryBuilder queryBuilder;
        
        public ISqlQueryBuilder QueryBuilder {
            get { return queryBuilder; }
            set { queryBuilder = value; }
        }
        
        public SqlDatabaseDriver() : this("")
        {
        }
        
        public SqlDatabaseDriver(string connectionString) : this(connectionString, new SqlQueryBuilder())
        {
        }
        
        public SqlDatabaseDriver(string connectionString, ISqlQueryBuilder queryBuilder)
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
                            property.SetValue(item, value);
                        }
                    }
                    /*if (property.GetCustomAttribute<ColumnAttribute>() != null) {
                        ColumnAttribute column = (ColumnAttribute)property.GetCustomAttribute(typeof(ColumnAttribute));
                        if (column.Name != null) {
                            propertyName = column.Name;
                        } else {
                            propertyName = property.Name;
                        }
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
                        property.SetValue(item, value);
                    }*/
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
        
        SqlDataReader ExecuteReader(string query)
        {
            var connection = new SqlConnection(ConnectionString);
            OpenConnection(connection);
            var cmd = new SqlCommand(query, connection);
            var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }
        
        T ExecuteScalar<T>(string query)
        {
            return ExecuteScalar<T>(query, default(T));
        }
        
        T ExecuteScalar<T>(string query, T defaultValue)
        {
            var connection = new SqlConnection(ConnectionString);
            OpenConnection(connection);
            var cmd = new SqlCommand(query, connection);
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
            var connection = new SqlConnection(ConnectionString);
            OpenConnection(connection);
            var cmd = new SqlCommand(query, connection);
            int rowsAffected = cmd.ExecuteNonQuery();
            CloseConnection(connection);
            return rowsAffected;
        }
        
        void OpenConnection(SqlConnection connection)
        {
            if (connection.State == ConnectionState.Closed) {
                connection.Open();
            }
        }
        
        void CloseConnection(SqlConnection connection)
        {
            if (connection.State == ConnectionState.Open) {
                connection.Close();
                connection.Dispose();
            }
        }
    }
    
    public abstract class DatabaseSeeder
    {
        IDatabaseDriver databaseDriver;
        
        public virtual void Run()
        {
        }

        public virtual void Rollback()
        {
        }
        
        protected Database db;
        
        public DatabaseSeeder()
        {
            var connectionString = ConfigurationManager.AppSettings["SqlConnection"];
            var databaseDriver = new SqlDatabaseDriver(connectionString); // TODO: Make this configurable!
            this.db = new Database(databaseDriver);
        }
        
        public DatabaseSeeder(IDatabaseDriver databaseDriver)
        {
            this.databaseDriver = databaseDriver;
            this.db = new Database(databaseDriver);
        }
    }
    
    
    
    
    
    public class Array // : IEnumerable<Array>
    {
        Dictionary<object, object> data = new Dictionary<object, object>();
        
        public IEnumerable<object> Keys {
            get { return data.Keys; }
        }
        
        public object this[object key] {
            get {
                if (data.ContainsKey(key)) {
                    return data[key];
                }
                return null;
            }
            set {
                data[key] = value;
            }
        }
        
        public Array Add(object key, object value)
        {
            data.Add(key, value);
            return this;
        }
    }
}
