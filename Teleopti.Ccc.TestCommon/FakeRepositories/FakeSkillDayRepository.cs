using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{

	public class FakeSkillDayRepository_DoNotUse : FakeSkillDayRepository
	{
		public override ICollection<ISkillDay> FindReadOnlyRange(DateOnlyPeriod period, IEnumerable<ISkill> skills,
			IScenario scenario)
		{
			return _skillDays
				.Where(skillDayInDb =>
					skillDayInDb.Scenario.Equals(scenario) &&
					period.Contains(skillDayInDb.CurrentDate) &&
					skills.Contains(skillDayInDb.Skill))
				.ToList();
		}
	}

	public class FakeSkillDayRepository : ISkillDayRepository
	{
		protected readonly List<ISkillDay> _skillDays = new List<ISkillDay>();
		public bool HasSkillDays;

		public void Add(ISkillDay root)
		{
			_skillDays.Add(root);
		}

		public IList<ISkillDay> Has(IList<ISkillDay> skillDays)
		{
			_skillDays.AddRange(skillDays);
			return skillDays;
		}

		public void Has(params ISkillDay[] skillDays)
		{
			_skillDays.AddRange(skillDays);
		}

		public void Remove(ISkillDay root)
		{
			throw new NotImplementedException();
		}

		public ISkillDay Get(Guid id)
		{
			return _skillDays.FirstOrDefault(x => x.Id.GetValueOrDefault() == id);
		}

		public IEnumerable<ISkillDay> LoadAll()
		{
			throw new NotImplementedException();
		}

		public ISkillDay Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public ICollection<ISkillDay> FindRange(DateOnlyPeriod period, ISkill skill, IScenario scenario)
		{
			return _skillDays
				.Where(skillDayInDb =>
						skillDayInDb.Scenario.Equals(scenario) &&
						period.Contains(skillDayInDb.CurrentDate) &&
						skill == skillDayInDb.Skill)
				.ToList();
		}

		public ICollection<ISkillDay> GetAllSkillDays(DateOnlyPeriod period, ICollection<ISkillDay> skillDays, ISkill skill, IScenario scenario,
			Action<IEnumerable<ISkillDay>> optionalAction)
		{
			throw new NotImplementedException();
		}

		public void Delete(DateOnlyPeriod dateTimePeriod, ISkill skill, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public DateOnly FindLastSkillDayDate(IWorkload workload, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public ISkillDay FindLatestUpdated(ISkill skill, IScenario scenario, bool withLongterm)
		{
			throw new NotImplementedException();
		}

		public virtual ICollection<ISkillDay> FindReadOnlyRange(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario)
		{
			var days = _skillDays.Where(skillDayInDb => skillDayInDb.Scenario.Equals(scenario) &&
														period.Contains(skillDayInDb.CurrentDate) && skills.Contains(skillDayInDb.Skill))
				.Select(skillDay =>
					{
						var ret = new SkillDay(skillDay.CurrentDate, skillDay.Skill, skillDay.Scenario, skillDay.WorkloadDayCollection,
							skillDay.SkillDataPeriodCollection);
						ret.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = skillDay.SkillDataPeriodCollection.First().Shrinkage);
						return ret;
					}
				).Cast<ISkillDay>().ToList();

			var grouped = days.GroupBy(d => d.Skill);
			foreach (var group in grouped)
			{
				var calculator = new SkillDayCalculator(group.Key, group, period);
				foreach (var skillDay in group)
				{
					skillDay.SetupSkillDay();
					skillDay.SkillDayCalculator = calculator;
				}
			}
			return days.ToList();
		}

		public bool HasSkillDaysWithinPeriod(DateOnly startDate, DateOnly endDate, IBusinessUnit businessUnit, IScenario scenario)
		{
			return HasSkillDays;
		}
	}
}