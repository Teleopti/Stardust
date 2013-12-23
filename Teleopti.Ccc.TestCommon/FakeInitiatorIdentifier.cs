using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeInitiatorIdentifier : IInitiatorIdentifier
	{
		public Guid InitiatorId { get; set; }
	}
}