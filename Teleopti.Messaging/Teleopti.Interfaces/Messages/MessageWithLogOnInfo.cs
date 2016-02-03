using System;

namespace Teleopti.Interfaces.Messages
{
    [Serializable]
	public abstract class MessageWithLogOnInfo : ILogOnInfo
    {
        public abstract Guid Identity { get; }
		public Guid InitiatorId { get; set; }
        public string LogOnDatasource { get; set; }
		public DateTime Timestamp { get; set; }
		public Guid LogOnBusinessUnitId { get; set; }
    }
}