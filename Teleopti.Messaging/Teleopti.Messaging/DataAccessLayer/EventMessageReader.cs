using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventMessageReader : ObjectReader<IEventMessage>
    {
        private const string StoreProcId = "msg.sp_Event_Select"; 
        private const string StoreProc = "msg.sp_Event_Select_All";

        public EventMessageReader(string connectionString) : base(connectionString)
        {
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, Guid id)
        {
            Collection<IDataParameter> collection = new Collection<IDataParameter>();
            IDataParameter param = command.CreateParameter();
            param.ParameterName = "EventId";
            param.Value = id;
            collection.Add(param);
            return collection;
        }

        protected override IMapperBase<IEventMessage> GetMapper()
        {
            IMapperBase<IEventMessage> mapper = new EventMessageMapper(ConnectionString);
            return mapper;
        }


        protected override string GetAllItems
        {
            get { return StoreProc; }
        }

        protected override string GetItemsById
        {
            get { return StoreProcId; }
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, int id)
        {
            throw new NotImplementedException();
        }
    }
}