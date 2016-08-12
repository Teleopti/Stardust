using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSiteOpenHourRepository:ISiteOpenHourRepository
	{
		private List<ISiteOpenHour> _openHours =new List<ISiteOpenHour>();
		public void Add(ISiteOpenHour siteOpenHour)
		{
			_openHours.Add(siteOpenHour);
		}

		public void Remove(ISiteOpenHour siteOpenHour)
		{
			_openHours.Remove(siteOpenHour);
		}

		public ISiteOpenHour Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<ISiteOpenHour> LoadAll()
		{
			throw new NotImplementedException();
		}

		public ISiteOpenHour Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<ISiteOpenHour> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; }
	}
}
