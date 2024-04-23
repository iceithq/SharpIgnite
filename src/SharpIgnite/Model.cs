using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharpIgnite
{
    public class Model
    {
        public Model()
        {
        }
        
        Database Database {
            get {
                return AppContext.Instance.Database;
            }
        }
        
        string TableName {
            get {
                Type type = this.GetType();
                
                TableAttribute tableAttribute = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute));
                string tableName = type.Name;
                if (tableAttribute != null && !tableAttribute.Name.IsNullOrEmpty()) {
                    tableName = tableAttribute.Name;
                }
                return tableName;
            }
        }
        
        public Model Save()
        {
            Database.Insert(TableName, this);
            return this;
        }
        
        public int Update()
        {
            var w = ToArray(GetPrimaryKeys());
            return Database
                .Update(TableName, this, w);
        }
        
        public void Delete()
        {
            var w = ToArray(GetPrimaryKeys());
            Database.Delete(TableName, w);
        }
        
        Array ToArray(List<PropertyInfo> properties)
        {
            var array = new Array();
            foreach (var property in properties) {
                string propertyName;
                if (property.GetCustomAttribute<ColumnAttribute>() != null) {
                    ColumnAttribute column = (ColumnAttribute)property.GetCustomAttribute(typeof(ColumnAttribute));
                    if (column.Name != null) {
                        propertyName = column.Name;
                    } else {
                        propertyName = property.Name;
                    }
                    object value = property.GetValue(this);
                    array.Add(propertyName, value);
                }
            }
            return array;
            
        }
        
        List<PropertyInfo> GetPrimaryKeys()
        {
            var primaryProperties = new List<PropertyInfo>();
            foreach (var p in GetColumnProperties()) {
                ColumnAttribute column = (ColumnAttribute)p.GetCustomAttribute(typeof(ColumnAttribute));
                if (column != null && column.IsPrimaryKey) {
                    primaryProperties.Add(p);
                }
            }
            return primaryProperties;
        }
        
        List<PropertyInfo> GetColumnProperties()
        {
            Type type = this.GetType();
            var columnProperties = new List<PropertyInfo>();
            foreach (PropertyInfo property in type.GetProperties()) {
                if (property.GetCustomAttribute<ColumnAttribute>() != null) {
                    columnProperties.Add(property);
                }
            }
            return columnProperties;
        }
        
        public static T Read<T>(Array array)
        {
            var t = Activator.CreateInstance<T>();
            var tableName = (t as Model).TableName;
            var db = AppContext.Instance.Database;
            return db
                .From(tableName)
                .Where(array)
                .Row<T>();
        }
        
        public static List<T> Find<T>(Array where)
        {
            var t = Activator.CreateInstance<T>();
            var tableName = (t as Model).TableName;
            var db = AppContext.Instance.Database;
            return db
                .From(tableName)
                .Where(where)
                .Result<T>();
        }
        
        public static List<T> All<T>()
        {
            var t = Activator.CreateInstance<T>();
            var tableName = (t as Model).TableName;
            var db = AppContext.Instance.Database;
            return db
                .From(tableName)
                .Result<T>();
        }
        
        public override string ToString()
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            string result = GetType().Name +  ": { ";
            foreach (PropertyInfo prop in properties) {
                object value = prop.GetValue(this);

                if (value != null && value.GetType() == typeof(string)) {
                    result += prop.Name + ": '" + value + "', ";
                } else {
                    result += prop.Name + ": " + value + ", ";
                }
            }
            result = result.TrimEnd(',', ' ') + " }";
            return result;
        }
        
    }
}
