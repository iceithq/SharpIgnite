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
        int Insert<T>(string tableName, List<T> data);
        int Insert<T>(string tableName, T data);
        int Insert(string tableName, Array data);
        List<T> Result<T>();
        T Row<T>();
        int Count(string tableName);
        int Update(string tableName);
        int Update<T>(string tableName, T data, Array _where);
        int Update<T>(string tableName, T data, string _where);
        int Delete(string tableName, Array _where);
        int Delete(string tableName, string _where);
        int Truncate(string tableName);
        int QueryFromFile(string filePath);
    }
    
    public abstract class BaseDatabaseDriver : IDatabaseDriver
    {
        public string ConnectionString { get; set; }
        public Database Database { get; set; }
        
        ISqlQueryBuilder queryBuilder;
        
        public ISqlQueryBuilder QueryBuilder {
            get { return queryBuilder; }
            set { queryBuilder = value; }
        }
        
        public int Insert<T>(string tableName, List<T> data)
        {
            string query = queryBuilder.Insert<T>(this.Database, tableName, data);
            Database.LastQuery = query;
            return ExecuteNonQuery(query);
        }
        
        public int Insert(string tableName, Array data)
        {
            string query = queryBuilder.Insert(this.Database, tableName, data);
            Database.LastQuery = query;
            Database.insertedId = ExecuteScalar<int>(query);
            return Database.insertedId;
        }
        
        public int Insert<T>(string tableName, T data)
        {
            string query = queryBuilder.Insert<T>(this.Database, tableName, data);
            Database.LastQuery = query;
            Database.insertedId = ExecuteScalar<int>(query);
            return InsertedId();
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
        
        public int Update<T>(string tableName, T data, Array _where)
        {
            string query = queryBuilder.Update(Database, tableName, data, _where);
            return ExecuteNonQuery(query);
        }
        
        public int Update<T>(string tableName, T data, string _where)
        {
            string query = queryBuilder.Update(Database, tableName, data, _where);
            return ExecuteNonQuery(query);
        }
        
        public int Delete(string tableName, Array _where)
        {
            string query = queryBuilder.Delete(this.Database, tableName, _where);
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
        
        public abstract IDbConnection CreateConnection(string connectionString);
        
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
    }
    
    public class SqlDatabaseDriver : BaseDatabaseDriver
    {
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
        
        public override IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
