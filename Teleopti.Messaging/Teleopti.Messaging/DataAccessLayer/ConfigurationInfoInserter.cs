﻿using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Core;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class ConfigurationInfoInserter : ObjectInserter<IConfigurationInfo>
    {
        public ConfigurationInfoInserter(string connectionString) : base(connectionString)
        {
        }

        protected override IMapperBase<IConfigurationInfo> GetMapper()
        {
            IMapperBase<IConfigurationInfo> mapper = new ConfigurationInfoMapper(ConnectionString);
            return mapper;
        }
    }
}
