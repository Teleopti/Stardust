using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeProjectionVersionPersister : IProjectionVersionPersister
	{
		public void Upsert(Guid personId, IEnumerable<DateOnly> dates)
		{
		}
	}
}