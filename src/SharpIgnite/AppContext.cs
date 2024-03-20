using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpIgnite
{
    public class AppContext
    {
        Input input;
        Output output;
        Database database;

        private AppContext()
        {
            input = new Input();
            output = new Output();

            var databaseDriver = CreateDatabaseDriver();
            database = new Database(databaseDriver);
        }

        static AppContext instance = new AppContext();

        public IDatabaseDriver CreateDatabaseDriver()
        {
            return new SqlDatabaseDriver();
        }

        public static AppContext Instance {
            get { return instance; }
        }

        public Database Database {
            get { return database; }
        }

        public Input Input {
            get { return input; }
        }
    }
}
