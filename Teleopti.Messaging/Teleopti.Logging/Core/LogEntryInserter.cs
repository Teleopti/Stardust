using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Logging.Core;

namespace Teleopti.Logging.Core
{
    public class LogEntryInserter : ObjectInserter<ILogEntry>
    {

        public LogEntryInserter(string connectionString) : base(connectionString)
        {
        }

        protected override IMapperBase<ILogEntry> GetMapper()
        {
            MapperBase<ILogEntry> mapper = new EventLogEntryMapper(ConnectionString);
            return mapper;
        }


    }
}