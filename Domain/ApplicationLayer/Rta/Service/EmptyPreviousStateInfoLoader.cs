using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class EmptyPreviousStateInfoLoader : IPreviousStateInfoLoader
	{
		public PreviousStateInfo Load(Guid personId)
		{
			return new PreviousStateInfo(personId);
		}
	}
}