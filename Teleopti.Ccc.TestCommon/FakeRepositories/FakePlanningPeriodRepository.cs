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
		private readonly INow _now;
		private IPlanningPeriod _planinngPeriod;
		private IPlanningPeriodSuggestions _planningPeriodSuggestions;

		public FakePlanningPeriodRepository(INow now)
		{
			_now = now;
		}

		public bool AddExecuted { get; set; }

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
			if (_planinngPeriod == null)
				return new PlanningPeriod(new PlanningPeriodSuggestions(_now, new AggregatedSchedulePeriod[] {}));
			return _planinngPeriod;
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
			if (_planningPeriodSuggestions == null)
				return new PlanningPeriodSuggestions(now, new List<AggregatedSchedulePeriod>());
			return _planningPeriodSuggestions;
		}

		public void CustomData(PlanningPeriod planinnPeriod, PlanningPeriodSuggestions planningPeriodSuggestions)
		{
			_planinngPeriod = planinnPeriod;
			_planningPeriodSuggestions = planningPeriodSuggestions;
		}
	}
}