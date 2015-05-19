using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeDayOffTemplateRepository : IDayOffTemplateRepository
	{
		private IList<IDayOffTemplate> _dayOffTemplate;
		public void Add(IDayOffTemplate entity)
		{
			throw new NotImplementedException();
		}

		public void Remove(IDayOffTemplate entity)
		{
			throw new NotImplementedException();
		}

		public IDayOffTemplate Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IDayOffTemplate> LoadAll()
		{
			return new List<IDayOffTemplate> { DayOffFactory.CreateDayOff() };
		}

		public IDayOffTemplate Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IDayOffTemplate> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IList<IDayOffTemplate> FindAllDayOffsSortByDescription()
		{
			return new List<IDayOffTemplate>() { new DayOffTemplate() };
		}
	}
}