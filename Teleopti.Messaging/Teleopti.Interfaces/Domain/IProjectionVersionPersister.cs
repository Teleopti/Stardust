using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IProjectionVersionPersister 
	{
		void Upsert(Guid personId, IEnumerable<DateOnly> dates);
	}
}