using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeProjectionVersionPersister : IProjectionVersionPersister
	{
		private readonly IList<dataWithPerson> _projectionVersions = new List<dataWithPerson>();

		private class dataWithPerson : ProjectionVersion
		{
			public Guid PersonId { get; set; }
		}

		public IEnumerable<ProjectionVersion> LockAndGetVersions(Guid personId, DateOnly from, DateOnly to)
		{
			var dates = from.DateRange(to);
			dates.ForEach(x =>
			{
				var existing = _projectionVersions.SingleOrDefault(v => v.PersonId == personId && v.Date.Equals(x));
				if (existing == null)
					_projectionVersions.Add(new dataWithPerson {PersonId = personId, Date = x, Version = 1});
				else
					existing.Version++;
			});
			return _projectionVersions.Where(x => x.PersonId == personId).ToList();
		}
	}
}