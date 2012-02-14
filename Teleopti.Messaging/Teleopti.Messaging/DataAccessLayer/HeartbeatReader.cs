using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class HeartbeatReader : ObjectReader<IEventHeartbeat>
    {
        private const string StoreProcId = "msg.sp_Heartbeat_Select";
        private const string StoreProc = "msg.sp_Heartbeat_Select_All";

        public HeartbeatReader(string connectionString) : base(connectionString)
        {
        }

        protected override string GetAllItems
        {
            get { return StoreProc; }
        }

        protected override string GetItemsById
        {
            get { return StoreProcId; }
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, Guid id)
        {
            ICollection<IDataParameter> collection = new Collection<IDataParameter>();
            IDataParameter param = command.CreateParameter();
            param.ParameterName = "SubscriberId";
            param.Value = id;
            collection.Add(param);
            return collection;
        }

        public IList<IEventHeartbeat> GetAllHeartbeatsForSubscriber(Guid id)
        {
            IDbConnection connection = ConnectionFactory.GetInstance(ConnectionString).GetOpenConnection();
            IDbCommand command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandType = CommandType;
            command.CommandText = GetItemsById;
            foreach (IDataParameter param in GetParameters(command, id))
                command.Parameters.Add(param);
            using (IDataReader reader = command.ExecuteReader())
            {
                try
                {
                    IMapperBase<IEventHeartbeat> mapper = GetMapper();
                    IList<IEventHeartbeat> collection = mapper.MapAll(reader);
                    return collection;
                }
                finally
                {
                    //reader.Close();
                    connection.Close();
                }
            }
        }


        public IList<IEventHeartbeat> DistinctHeartbeats()
        {
            IDbConnection connection = ConnectionFactory.GetInstance(ConnectionString).GetOpenConnection();
            IDbCommand command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandType = CommandType;
            command.CommandText = "msg.sp_Distinct_Heartbeats";
            using (IDataReader reader = command.ExecuteReader())
            {
                try
                {
                    IMapperBase<IEventHeartbeat> mapper = GetMapper();
                    IList<IEventHeartbeat> collection = mapper.MapAll(reader);
                    return collection;
                }
                finally
                {
                    //reader.Close();
                    connection.Close();
                }
            }
        }

        protected override IMapperBase<IEventHeartbeat> GetMapper()
        {
            IMapperBase<IEventHeartbeat> mapper = new HeartbeatMapper(ConnectionString);
            return mapper;
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, int id)
        {
            throw new NotImplementedException();
        }


    }
}
