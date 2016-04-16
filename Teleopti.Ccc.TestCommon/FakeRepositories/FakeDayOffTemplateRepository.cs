using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeDayOffTemplateRepository : IDayOffTemplateRepository
	{
		private readonly IList<IDayOffTemplate> _dayOffTemplates = new List<IDayOffTemplate>(); 

		public void Add(IDayOffTemplate entity)
		{
			_dayOffTemplates.Add(entity);
		}

		public void Remove(IDayOffTemplate entity)
		{
			throw new NotImplementedException();
		}

		public IDayOffTemplate Get(Guid id)
		{
			return _dayOffTemplates.FirstOrDefault(d => id == d.Id);
		}

		public IList<IDayOffTemplate> LoadAll()
		{
			if(!_dayOffTemplates.Any())
				return new List<IDayOffTemplate> { DayOffFactory.CreateDayOff() };

			return _dayOffTemplates;
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