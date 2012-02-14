using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class ConcurrentUsersReader : ObjectReader<IConcurrentUsers>
    {
        private const string StoreProc = "msg.sp_ConcurrentUsers_Select";

        public ConcurrentUsersReader(string connectionString) : base(connectionString)
        {
        }

        protected override IMapperBase<IConcurrentUsers> GetMapper()
        {
            IMapperBase<IConcurrentUsers> mapper = new ConcurrentUsersMapper(ConnectionString);
            return mapper;  
        }

        protected override string GetAllItems
        {
            get { return StoreProc; }
        }

        protected override string GetItemsById
        {
            get { return StoreProc; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        protected ICollection<IDataParameter> GetParameters(IDbCommand command, string address)
        {
            ICollection<IDataParameter> collection = new Collection<IDataParameter>();
            IDataParameter param = command.CreateParameter();
            param.ParameterName = "Address";
            param.Value = address;
            collection.Add(param);
            return collection;
        }

        public IConcurrentUsers Execute(string address)
        {
            IDbConnection connection = ConnectionFactory.GetInstance(ConnectionString).GetOpenConnection();
            IDbCommand command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandType = CommandType;
            command.CommandText = GetItemsById;
            foreach (IDataParameter param in GetParameters(command, address))
                command.Parameters.Add(param);
            using (IDataReader reader = command.ExecuteReader())
            {
                try
                {
                    IMapperBase<IConcurrentUsers> mapper = GetMapper();
                    IList<IConcurrentUsers> collection = mapper.MapAll(reader);
                    if (collection != null && collection.Count > 0)
                        return collection[0];
                    return default(IConcurrentUsers);
                }
                finally
                {
                    //reader.Close();
                    connection.Close();
                }
            }
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, int id)
        {
            throw new NotImplementedException();
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
