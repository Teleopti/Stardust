using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public interface ICommandIdentifier
	{
		Guid CommandId { get; set; }
	}
}