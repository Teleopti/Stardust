using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IInitiatorContext
	{
		Guid InitiatorId { get; set; }
	}
}