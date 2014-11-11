using System;

namespace Teleopti.Interfaces.Messages
{
    [Serializable]
	public abstract class MessageWithLogOnInfo : ILogOnInfo
    {
        public abstract Guid Identity { get; }
		public Guid InitiatorId { get; set; }
        public string Datasource { get; set; }
		public DateTime Timestamp { get; set; }
		public Guid BusinessUnitId { get; set; }
    }
}