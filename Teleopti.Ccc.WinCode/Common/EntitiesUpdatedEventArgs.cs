using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
	public class EntitiesUpdatedEventArgs : EventArgs
	{
		public IEnumerable<Guid> UpdatedIds { get; set; }
		public Type EntityType { get; set; }
	    public DomainUpdateType EntityStatus { get; set; }
	}
}