using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePlanningPeriodRepository : IPlanningPeriodRepository
	{
		private readonly IList<PlanningPeriod> _planningPeriods = new List<PlanningPeriod>();
		private IPlanningPeriodSuggestions _planningPeriodSuggestions;

		public void Add(PlanningPeriod entity)
		{
			entity.SetId(Guid.NewGuid());
			_planningPeriods.Add(entity);
		}

		public PlanningPeriod Has(DateOnly start, int numberOfWeeks)
		{
			return Has(start, numberOfWeeks, new PlanningGroup());
		}

		//TODO removed endDate
		public PlanningPeriod Has(DateOnly startDate, DateOnly endDate, SchedulePeriodType schedulePeriodType, int number)
		{
			var planningPeriod = new PlanningPeriod(startDate, schedulePeriodType,number, new PlanningGroup()).WithId();
			_planningPeriods.Add(planningPeriod);
			return planningPeriod;
		}

		public PlanningPeriod Has(DateOnly start, int number, SchedulePeriodType type, PlanningGroup planningGroup)
		{
			DateTime now;
			switch (type)
			{
				case SchedulePeriodType.Month:
				case SchedulePeriodType.ChineseMonth:
					now = start.Date.AddMonths(-number);
					break;
				case SchedulePeriodType.Week:
					now = start.Date.AddDays(-7 * number);
					break;
				case SchedulePeriodType.Day:
					now = start.Date.AddDays(-number);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
			var planningPeriod =
				new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(now), new[]
				{
					new AggregatedSchedulePeriod
					{
						DateFrom = start.Date,
						Number = number,
						PeriodType = type
					}
				}), planningGroup).WithId();
			_planningPeriods.Add(planningPeriod);
			return planningPeriod;

			
		}

		public PlanningPeriod Has(DateOnly start, int numberOfWeeks, PlanningGroup planningGroup)
		{
			return Has(start, numberOfWeeks, SchedulePeriodType.Week, planningGroup);
		}

		public void Remove(PlanningPeriod entity)
		{
			_planningPeriods.Remove(entity);
		}

		public PlanningPeriod Get(Guid id)
		{
			return _planningPeriods.FirstOrDefault(planningPeriod => planningPeriod.Id == id);
		}

		public IEnumerable<PlanningPeriod> LoadAll()
		{
			return _planningPeriods;
		}

		public PlanningPeriod Load(Guid id)
		{
			return _planningPeriods.FirstOrDefault(planningPeriod => planningPeriod.Id == id);
		}

		public IPlanningPeriodSuggestions Suggestions(INow now)
		{
			if (_planningPeriodSuggestions == null)
				return new PlanningPeriodSuggestions(now, new List<AggregatedSchedulePeriod>());
			return _planningPeriodSuggestions;
		}

		public IEnumerable<PlanningPeriod> LoadForPlanningGroup(PlanningGroup planningGroup)
		{
			return _planningPeriods.Where(x => x.PlanningGroup.Id == planningGroup.Id).ToList();
		}

		public void CustomData(PlanningPeriod planningPeriod, PlanningPeriodSuggestions planningPeriodSuggestions)
		{
			if (planningPeriod != null)
				_planningPeriods.Add(planningPeriod);
			_planningPeriodSuggestions = planningPeriodSuggestions;
		}

		public IPlanningPeriodSuggestions Suggestions(INow now, ICollection<Guid> personIds)
		{
			return Suggestions(now);
		}
	}
}