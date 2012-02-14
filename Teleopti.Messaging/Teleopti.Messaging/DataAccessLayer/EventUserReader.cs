using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventUserReader : ObjectReader<IEventUser>
    {
        private const string StoreProcId = "msg.sp_Users_Select";
        private const string StoreProc = "msg.sp_Users_Select_All";

        public EventUserReader(string connectionString) : base(connectionString)
        {
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, int id)
        {
            Collection<IDataParameter> collection = new Collection<IDataParameter>();
            IDataParameter param = command.CreateParameter();
            param.ParameterName = "UserId"; 
            param.Value = id;
            collection.Add(param);
            return collection;   
        }

        protected override IMapperBase<IEventUser> GetMapper()
        {
            MapperBase<IEventUser> mapper = new EventUserMapper(ConnectionString);
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

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, Guid id)
        {
            throw new NotImplementedException();
        }
    }
}