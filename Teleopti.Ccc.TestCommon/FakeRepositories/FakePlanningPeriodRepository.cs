using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

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

		public IPlanningPeriod Has(DateOnly start, int numberOfWeeks)
		{
			return Has(start, numberOfWeeks, null);
		}

		public IPlanningPeriod Has(DateOnly start, int numberOfWeeks, IAgentGroup agentGroup)
		{
			var planningPeriod =
				new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(start.Date.AddDays(-7*numberOfWeeks)), new[]
				{
					new AggregatedSchedulePeriod
					{
						DateFrom = start.Date,
						Number = numberOfWeeks,
						PeriodType = SchedulePeriodType.Week
					}
				}), agentGroup);
			planningPeriod.SetId(Guid.NewGuid());
			_planningPeriods.Add(planningPeriod);
			return planningPeriod;
		}

		public void Remove(IPlanningPeriod entity)
		{
			_planningPeriods.Remove(entity);
		}

		public IPlanningPeriod Get(Guid id)
		{
			return _planningPeriods.FirstOrDefault(planningPeriod => planningPeriod.Id == id);
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
			return _planningPeriods.FirstOrDefault(planningPeriod => planningPeriod.Id == id);
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public IPlanningPeriodSuggestions Suggestions(INow now)
		{
			if (_planningPeriodSuggestions == null)
				return new PlanningPeriodSuggestions(now, new List<AggregatedSchedulePeriod>());
			return _planningPeriodSuggestions;
		}

		public IEnumerable<IPlanningPeriod> LoadForAgentGroup(IAgentGroup agentGroup)
		{
			return _planningPeriods.Where(x => x.AgentGroup?.Id == agentGroup.Id).ToList();
		}

		public void CustomData(PlanningPeriod planningPeriod, PlanningPeriodSuggestions planningPeriodSuggestions)
		{
			if (planningPeriod != null)
				_planningPeriods.Add(planningPeriod);
			_planningPeriodSuggestions = planningPeriodSuggestions;
		}
	}
}