using MongoDB.Driver;

namespace Devoted.GenericLibrary.GenericNoSql.Context
{
    public class NoSqlDbContext
    {
        internal IMongoDatabase database { get; private set; }


        public NoSqlDbContext(string connectionStr, string db)
        {
            var client = new MongoClient(
                    connectionStr
                );

            database = client.GetDatabase(
                db
            );
        }
    }
}
