using System;
using NUnit.Framework;

namespace SharpIgnite.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        [Test]
        public void TestMethod()
        {
            var connectionString = "server=localhost;database=test;uid=root";
            var databaseDriver = new MySqlDatabaseDriver(connectionString);
            var db = new Database(databaseDriver);
            
            var items = db
                .Get("items")
                .Result<Item>();
            foreach (var i in items) {
                Console.WriteLine(i.Name);
            }
        }
        
        public class Item
        {
            [Column("id", true)] public int Id { get; set; }
            [Column("name")] public string Name { get; set; }
        }
    }
}
