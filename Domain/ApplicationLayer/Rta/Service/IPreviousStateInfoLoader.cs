using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IPreviousStateInfoLoader
	{
		StoredStateInfo Load(Guid personId);
	}
}