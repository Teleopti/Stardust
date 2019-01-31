using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[Serializable]
	public class EventWithLogOnContext : Event, ILogOnContext
	{
		public string LogOnDatasource { get; set; }
		public Guid LogOnBusinessUnitId { get; set; }
	}
}