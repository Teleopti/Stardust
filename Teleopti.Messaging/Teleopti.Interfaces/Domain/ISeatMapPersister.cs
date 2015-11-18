using System;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatMapPersister
	{
		void Save(ISaveSeatMapCommand command);
	}
}