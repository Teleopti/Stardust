using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class EntitiesUpdatedEventArgs : EventArgs
	{
		public IEnumerable<Guid> UpdatedIds { get; set; }
		public Type EntityType { get; set; }
	    public DomainUpdateType EntityStatus { get; set; }
	}
}