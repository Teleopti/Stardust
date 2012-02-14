using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class AddressReader : ObjectReader<IAddressInformation>
    {
        private const string StoreProcId = "msg.sp_Address_Select";
        private const string StoreProc = "msg.sp_Address_Select_All";

        public AddressReader(string connectionString)
            : base(connectionString)
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

        protected override IMapperBase<IAddressInformation> GetMapper()
        {
            IMapperBase<IAddressInformation> mapper = new AddressMapper(ConnectionString);
            return mapper;
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, int id)
        {
            ICollection<IDataParameter> collection = new Collection<IDataParameter>();
            IDataParameter param = command.CreateParameter();
            param.ParameterName = "AddressId";
            param.Value = id;
            collection.Add(param);
            return collection;
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
