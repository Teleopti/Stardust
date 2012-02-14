using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Logging.Core;

namespace Teleopti.Logging.Core
{
    public class LogEntryReader : ObjectReader<ILogEntry>
    {
        private const string StoreProcId = "msg.sp_Log_Select";
        private const string StoreProc = "msg.sp_Log_Select_All";

        public LogEntryReader(string connectionString) : base(connectionString)
        {
        }

        protected override ICollection<IDataParameter> GetParameters(IDbCommand command, Guid id)
        {
            Collection<IDataParameter> collection = new Collection<IDataParameter>();
            IDataParameter param = command.CreateParameter();
            param.ParameterName = "LogId";
            param.Value = id;
            collection.Add(param);
            return collection;  
        }

        protected override IMapperBase<ILogEntry> GetMapper()
        {
            MapperBase<ILogEntry> mapper = new EventLogEntryMapper(ConnectionString);
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