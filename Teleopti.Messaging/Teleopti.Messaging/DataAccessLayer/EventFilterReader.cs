using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventFilterReader : ObjectReader<IEventFilter>
    {
        private const string StoreProcId = "msg.sp_Filter_Select";
        private const string StoreProc = "msg.sp_Filter_Select_All";

        public EventFilterReader(string connectionString) : base(connectionString)
        {
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, Guid id)
        {
            ICollection<IDataParameter> collection = new Collection<IDataParameter>();
            IDataParameter param = command.CreateParameter();
            param.ParameterName = "FilterId";
            param.Value = id;
            collection.Add(param);
            return collection;
        }

        protected override IMapperBase<IEventFilter> GetMapper()
        {
            IMapperBase<IEventFilter> mapper = new EventFilterMapper(ConnectionString);
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