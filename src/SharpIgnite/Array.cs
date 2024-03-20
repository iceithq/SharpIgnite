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
