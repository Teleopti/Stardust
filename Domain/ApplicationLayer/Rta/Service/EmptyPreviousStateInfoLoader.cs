using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class EmptyPreviousStateInfoLoader : IPreviousStateInfoLoader
	{
		public StoredStateInfo Load(Guid personId)
		{
			return null;
		}
	}
}