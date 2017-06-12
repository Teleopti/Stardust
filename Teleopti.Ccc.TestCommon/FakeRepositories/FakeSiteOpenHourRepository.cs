using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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

		public IUnitOfWork UnitOfWork { get; }
	}
}
