using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeInitiatorIdentifier : IInitiatorIdentifier
	{
		public Guid InitiatorId { get; set; }
	}
}