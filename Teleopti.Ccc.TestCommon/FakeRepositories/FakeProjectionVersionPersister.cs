using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeProjectionVersionPersister : IProjectionVersionPersister
	{
		private readonly IList<dataWithPerson> _projectionVersions = new List<dataWithPerson>();

		private class dataWithPerson : ProjectionVersion
		{
			public Guid PersonId{get; set; }
		}

		public IEnumerable<ProjectionVersion> Upsert(Guid personId, IEnumerable<DateOnly> dates)
		{
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