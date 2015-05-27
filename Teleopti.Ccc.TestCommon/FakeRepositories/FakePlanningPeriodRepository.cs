using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePlanningPeriodRepository : IPlanningPeriodRepository
	{
		public bool AddExecuted { get; set; }
		public IPlanningPeriodSuggestions CustomSuggestions { get; set; }
		public IPlanningPeriod CustomPlanningPeriod { get; set; }
		public void Add(IPlanningPeriod entity)
		{
			entity.SetId(Guid.NewGuid());
			AddExecuted = true;
		}

		public void Remove(IPlanningPeriod entity)
		{
			throw new NotImplementedException();
		}

		public IPlanningPeriod Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPlanningPeriod> LoadAll()
		{
			return new IPlanningPeriod[] {};
		}

		public IPlanningPeriod Load(Guid id)
		{
			if (CustomPlanningPeriod== null)
			return new PlanningPeriod(new PlanningPeriodSuggestions(new TestableNow(DateTime.Now),new AggregatedSchedulePeriod[]{} ));
			return CustomPlanningPeriod;
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IPlanningPeriod> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IPlanningPeriodSuggestions Suggestions(INow now)
		{
			if (CustomSuggestions == null)
				return new PlanningPeriodSuggestions(now, new List<AggregatedSchedulePeriod>());
			return CustomSuggestions;
		}
	}
}