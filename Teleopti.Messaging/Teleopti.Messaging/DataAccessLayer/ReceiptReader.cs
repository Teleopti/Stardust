using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class ReceiptReader : ObjectReader<IEventReceipt>
    {
        private const string StoreProcId = "msg.sp_Receipt_Select";
        private const string StoreProc = "msg.sp_Receipt_Select_All";

        public ReceiptReader(string connectionString) : base(connectionString)
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
            param.ParameterName = "ReceiptId";
            param.Value = id;
            collection.Add(param);
            return collection;
        }

        protected override IMapperBase<IEventReceipt> GetMapper()
        {
            IMapperBase<IEventReceipt> mapper = new ReceiptMapper(ConnectionString);
            return mapper;
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, int id)
        {
            throw new NotImplementedException();
        }

    }
}
