using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class ConfigurationInfoReader : ObjectReader<IConfigurationInfo>
    {
        private const string StoreProcId = "msg.sp_Configuration_Select";
        private const string StoreProc = "msg.sp_Configuration_Select_All";

        public ConfigurationInfoReader(string connectionString) : base(connectionString)
        {
        }

        protected override IMapperBase<IConfigurationInfo> GetMapper()
        {
            IMapperBase<IConfigurationInfo> mapper = new ConfigurationInfoMapper(ConnectionString);
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
            ICollection<IDataParameter> collection = new Collection<IDataParameter>();
            IDataParameter param = command.CreateParameter();
            param.ParameterName = "ConfigurationId";
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
