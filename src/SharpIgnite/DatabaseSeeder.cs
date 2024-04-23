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
}
