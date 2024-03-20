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
            string connectionString = "Data Source=test.db;Version=3;";
            var databaseDriver = new SQLiteDatabaseDriver(connectionString);
            var db = new Database(databaseDriver);
            
            var item = new Item {
                Name = "Item Lalala"
            };
            db.Insert("items", item);
            
            item = db
                .GetWhere("items", "name = 'Item Lalala'")
                .Row<Item>();
            Console.WriteLine(item.Name);
            
            var items = db
                .Get("items")
                .Result<Item>();
            foreach (var i in items) {
                Console.WriteLine(i.Name);
            }
            
            item.Name = "Item Lololo";
            db.Update("items", item, "name = 'Item Lalala'");
            
            db.Delete("items", "name = 'Item Lololo'");
        }
        
        public class Item
        {
            [Column("id", true)] public int Id { get; set; }
            [Column("name")] public string Name { get; set; }
        }
    }
}
