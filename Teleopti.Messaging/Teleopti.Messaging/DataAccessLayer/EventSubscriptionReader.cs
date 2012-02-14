using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventSubscriberReader : ObjectReader<IEventSubscriber>
    {
        private const string StoreProcId = "msg.sp_Subscriber_Select";
        private const string StoreProc = "msg.sp_Subscriber_Select_All";

        public EventSubscriberReader(string connectionString) : base(connectionString)
        {
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, Guid id)
        {
            Collection<IDataParameter> collection = new Collection<IDataParameter>();
            IDataParameter param = command.CreateParameter();
            param.ParameterName = "SubscriberId";
            param.Value = id;
            collection.Add(param);
            return collection;
        }

        protected override IMapperBase<IEventSubscriber> GetMapper()
        {
            MapperBase<IEventSubscriber> mapper = new EventSubscriberMapper(ConnectionString);
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