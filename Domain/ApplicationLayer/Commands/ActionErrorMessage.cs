using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ActionErrorMessage
	{
		public Guid PersonId { get; set; }
		public Name PersonName { get; set; }
		public IEnumerable<string> ErrorMessages { get; set; }
	}
}