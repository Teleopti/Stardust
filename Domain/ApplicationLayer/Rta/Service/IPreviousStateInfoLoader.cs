using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IPreviousStateInfoLoader
	{
		PreviousStateInfo Load(Guid personId);
	}
}