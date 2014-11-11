﻿using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Messages.Rta
{
	public class FactQueueUpdatedMessage : IEvent
	{
		public int SourceId { get; set; }
		public string Datasource { get; set; }
		public string DataSentUpUntilInterval { get; set; }
	}
}
