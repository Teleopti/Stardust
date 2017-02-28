using System;

namespace Teleopti.Interfaces.Messages
{
    [Serializable]
	public abstract class MessageWithLogOnContext : ILogOnContext
    {
        public abstract Guid Identity { get; }
		public Guid InitiatorId { get; set; }
        public string LogOnDatasource { get; set; }
		public DateTime Timestamp { get; set; }
		public Guid LogOnBusinessUnitId { get; set; }
    }
}