using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.Caching;
using log4net;
using Teleopti.Ccc.Rta.Interfaces;

namespace Teleopti.Ccc.Rta.Server
{
    public class PersonResolver : IPersonResolver
    {
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
        private readonly string _connectionStringDataStore;
        private readonly ILog _loggingSvc;
        private readonly Cache _cache;
        private static readonly object lockObj = new object();

        public PersonResolver(IDatabaseConnectionFactory databaseConnectionFactory, string connectionStringDataStore) : this(databaseConnectionFactory,connectionStringDataStore,LogManager.GetLogger(typeof(PersonResolver)))
        {
        }

        protected PersonResolver(IDatabaseConnectionFactory databaseConnectionFactory, string connectionStringDataStore, ILog loggingSvc)
        {
            _databaseConnectionFactory = databaseConnectionFactory;
            _connectionStringDataStore = connectionStringDataStore;
            _loggingSvc = loggingSvc;

            _cache = HttpRuntime.Cache;
        }

        public bool TryResolveId(int dataSourceId, string logOn, out IEnumerable<Guid> personId)
        {
            var lookupKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", dataSourceId, logOn);

            IDictionary<string, IEnumerable<Guid>> dictionary;
            lock (lockObj)
            {
                dictionary = (IDictionary<string, IEnumerable<Guid>>)_cache.Get("PeopleCache");
                if (dictionary==null)
                {
                    dictionary = initialize();
                }
            }

            return dictionary.TryGetValue(lookupKey, out personId);
        }

        private IDictionary<string ,IEnumerable<Guid>> initialize()
        {
            _loggingSvc.Info("Loading new data into cache.");
            var dictionary = new Dictionary<string, IEnumerable<Guid>>();
            using (var connection = _databaseConnectionFactory.CreateConnection(_connectionStringDataStore))
            {
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "RTA.rta_load_external_logon";
                connection.Open();
                var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (reader.Read())
                {
                    int loadedDataSourceId = reader.GetInt16(reader.GetOrdinal("datasource_id"));
                    string originalLogOn = reader.GetString(reader.GetOrdinal("acd_login_original_id"));
                    Guid personId = reader.GetGuid(reader.GetOrdinal("person_code"));
                    
                    var lookupKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", loadedDataSourceId, originalLogOn);
                    IEnumerable<Guid> list;
                    if (dictionary.TryGetValue(lookupKey,out list))
                    {
                        ((ICollection<Guid>)list).Add(personId);
                    }
                    else
                    {
                        dictionary.Add(lookupKey,new Collection<Guid>{personId});
                    }
                }
                reader.Close();
            }

            addBatchSignature(dictionary);

            _cache.Add("PeopleCache", dictionary, null, DateTime.Now.AddMinutes(10), Cache.NoSlidingExpiration,
                       CacheItemPriority.Default, onRemoveCallback);
            
            _loggingSvc.Info("Done loading new data into cache.");
            
            return dictionary;
        }

        private static void addBatchSignature(Dictionary<string, IEnumerable<Guid>> dictionary)
        {
            IEnumerable<Guid> foundIds;
            if (!dictionary.TryGetValue(string.Empty,out foundIds))
            {
                dictionary.Add(string.Empty,new []{Guid.Empty});
            }
        }

        private void onRemoveCallback(string key, object value, CacheItemRemovedReason reason)
        {
            _loggingSvc.Info("The cache was cleared.");
            lock (lockObj)
            {
                initialize();
            }
        }
    }
}