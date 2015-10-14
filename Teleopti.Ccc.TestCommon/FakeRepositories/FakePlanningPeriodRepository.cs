using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly IList<IPlanningPeriod> _planningPeriods;
		private IPlanningPeriodSuggestions _planningPeriodSuggestions;

		public FakePlanningPeriodRepository(INow now)
		{
			_now = now;
			_planningPeriods = new List<IPlanningPeriod>();
		}

		public bool AddExecuted { get; set; }

		public void Add(IPlanningPeriod entity)
		{
			entity.SetId(Guid.NewGuid());
			_planningPeriods.Add(entity);
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
			return _planningPeriods;
		}

		public IPlanningPeriod Load(Guid id)
		{
			if (!_planningPeriods.Any())
				return new PlanningPeriod(new PlanningPeriodSuggestions(_now, new AggregatedSchedulePeriod[] {}));
			//TODO: fix this!
			return _planningPeriods.FirstOrDefault();
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
			if(planinnPeriod != null)
				_planningPeriods.Add(planinnPeriod);
			_planningPeriodSuggestions = planningPeriodSuggestions;
		}
	}
}