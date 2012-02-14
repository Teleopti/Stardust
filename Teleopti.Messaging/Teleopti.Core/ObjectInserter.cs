using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Core
{
    public abstract class ObjectInserter<T> : IObjectInserter<T> where T : class
    {
        private readonly string _connectionString;

        protected ObjectInserter(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected string ConnectionString
        {
            get { return _connectionString; }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected abstract IMapperBase<T> GetMapper();

        public bool Execute(T domainObject)
        {
            if(domainObject == null)
                throw new ArgumentNullException("domainObject", "Domain object to be insterted into database can not be null.");
            bool isSucceded;
            using (SqlConnection connection = ConnectionFactory.GetInstance(ConnectionString).GetOpenConnection())
            {
            try 
            {
                IMapperBase<T> mapper = GetMapper();
                isSucceded = mapper.Insert(connection, domainObject);
            }
            finally
            {
                //connection.Close();
            }
            }
            return isSucceded;
        }

        public bool Execute(ICollection<T> collection)
        {
            if(collection == null)
                throw new ArgumentNullException("collection", "Collection of domain objects can not be null.");                       
            IMapperBase<T> mapper = GetMapper();
            return mapper.InsertAll(collection);
        }

    }
}