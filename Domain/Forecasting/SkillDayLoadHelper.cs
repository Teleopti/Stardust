using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface ISkillDayLoadHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IDictionary<ISkill, IEnumerable<ISkillDay>> LoadSchedulerSkillDays(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario);
		IDictionary<ISkill, IEnumerable<ISkillDay>> LoadSkillDaysWithFlexablePeriod(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario);
		IEnumerable<ISkillDay> LoadSchedulerSkillDaysFlat(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IDictionary<ISkill, IEnumerable<ISkillDay>> LoadBudgetSkillDays(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario);
	}

	/// <summary>
	/// Helper for loading of skill days
	/// </summary>
	/// <remarks>
	/// Created by: robink
	/// Created date: 2008-05-08
	/// </remarks>
	public class SkillDayLoadHelper : ISkillDayLoadHelper
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IMultisiteDayRepository _multisiteDayRepository;
		private readonly IStaffingCalculatorServiceFacade _staffingCalculatorServiceFacade;

		public SkillDayLoadHelper(ISkillDayRepository skillDayRepository, IMultisiteDayRepository multisiteDayRepository, IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
		{
			InParameter.NotNull(nameof(multisiteDayRepository), multisiteDayRepository);
			InParameter.NotNull(nameof(skillDayRepository), skillDayRepository);

			_skillDayRepository = skillDayRepository;
			_multisiteDayRepository = multisiteDayRepository;
			_staffingCalculatorServiceFacade = staffingCalculatorServiceFacade;
		}

		/// <summary>
		/// Loads the scheduler skill days.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <param name="skills">The skills.</param>
		/// <param name="scenario">The scenario.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-05-08
		/// </remarks>
		public IDictionary<ISkill, IEnumerable<ISkillDay>> LoadSchedulerSkillDays(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario)
		{
			if (skills == null || scenario==null) return new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			
			var skillsToLoad = skills.Where(skill => !skill.IsChildSkill).ToArray();
			var periodToLoad = new DateOnlyPeriod(period.StartDate.AddDays(-8), period.EndDate.AddDays(2));
			return calculateSkillSkillDayDictionary(period, scenario, skillsToLoad,periodToLoad);
		}

		public IDictionary<ISkill, IEnumerable<ISkillDay>> LoadSkillDaysWithFlexablePeriod(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario)
		{
			if (skills == null || scenario == null) return new Dictionary<ISkill, IEnumerable<ISkillDay>>();

			IDictionary<ISkill, IEnumerable<ISkillDay>> result = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			var skillsToLoad = skills.Where(skill => !skill.IsChildSkill).ToArray();

			var emailAndBackofficeSkills = skillsToLoad.Where(x =>
				x.SkillType.ForecastSource == ForecastSource.Backoffice || x.SkillType.ForecastSource == ForecastSource.Email);
			var periodToLoad = new DateOnlyPeriod(period.StartDate.AddDays(-8), period.EndDate.AddDays(2));
			result =  calculateSkillSkillDayDictionary(period, scenario, emailAndBackofficeSkills, periodToLoad);

			var otherSkills = skillsToLoad.Except(emailAndBackofficeSkills);
			periodToLoad = new DateOnlyPeriod(period.StartDate.AddDays(-1), period.EndDate.AddDays(1));
			var otherSkillsDays = calculateSkillSkillDayDictionaryWithoutPeriodInflation(period, scenario, otherSkills, periodToLoad);

			otherSkillsDays.ForEach(x => {result.Add(x.Key,x.Value);});
			return result;
		}

		
		public IEnumerable<ISkillDay> LoadSchedulerSkillDaysFlat(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario)
		{
			return this.LoadSchedulerSkillDays(period, skills, scenario).SelectMany(x => x.Value);
		}

		public IDictionary<ISkill, IEnumerable<ISkillDay>> LoadBudgetSkillDays(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario)
		{
			if (skills == null || scenario == null) return new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			var skillsToLoad = new List<ISkill>();
			foreach (var skill in skills)
			{
				if (skill is IMultisiteSkill) throw new ArgumentException("Should have no multisite skill.");
				if (skill is IChildSkill childSkill)
				{
					var multisiteSkill = childSkill.ParentSkill;
					if (!skillsToLoad.Contains(multisiteSkill))
						skillsToLoad.Add(multisiteSkill);
					continue;
				}
				skillsToLoad.Add(skill);
			}
			var periodToLoad = new DateOnlyPeriod(period.StartDate.AddDays(-8), period.EndDate.AddDays(2));
			return calculateSkillSkillDayDictionary(period, scenario, skillsToLoad,periodToLoad);
		}

		private IDictionary<ISkill, IEnumerable<ISkillDay>> calculateSkillSkillDayDictionary(DateOnlyPeriod period, IScenario scenario, IEnumerable<ISkill> skillsToLoad, DateOnlyPeriod periodToLoad)
		{
			IList<SkillDayCalculator> calculators = new List<SkillDayCalculator>();
			// now we don't know the timezone stretch the period one day more in start and end
			//var periodToLoad = new DateOnlyPeriod(period.StartDate.AddDays(-8), period.EndDate.AddDays(2));
			var testSkillDays = _skillDayRepository.FindReadOnlyRange(periodToLoad, skillsToLoad, scenario).ToLookup(k => k.Skill);

			periodToLoad = SkillDayCalculator.GetPeriodToLoad(period);
			return work(period, scenario, skillsToLoad, periodToLoad, testSkillDays, calculators);
		}

		private IDictionary<ISkill, IEnumerable<ISkillDay>> calculateSkillSkillDayDictionaryWithoutPeriodInflation(DateOnlyPeriod period, IScenario scenario, IEnumerable<ISkill> skillsToLoad, DateOnlyPeriod periodToLoad)
		{
			IList<SkillDayCalculator> calculators = new List<SkillDayCalculator>();
			var testSkillDays = _skillDayRepository.FindReadOnlyRange(periodToLoad, skillsToLoad, scenario).ToLookup(k => k.Skill);

			//periodToLoad = SkillDayCalculator.GetPeriodToLoad(period);
			return work(period, scenario, skillsToLoad, periodToLoad, testSkillDays, calculators);
		}

		private IDictionary<ISkill, IEnumerable<ISkillDay>> work(DateOnlyPeriod period, IScenario scenario, IEnumerable<ISkill> skillsToLoad,
			DateOnlyPeriod periodToLoad, ILookup<ISkill, ISkillDay> testSkillDays, IList<SkillDayCalculator> calculators)
		{
			foreach (var skill in skillsToLoad)
			{
				skill.SkillType.StaffingCalculatorService = _staffingCalculatorServiceFacade;
				var skillDays = testSkillDays[skill].OrderBy(s => s.CurrentDate).ToArray();

				if (skill is IMultisiteSkill multisiteSkill)
				{
					IList<IMultisiteDay> multisiteDays =
						_multisiteDayRepository.FindRange(periodToLoad, multisiteSkill, scenario).ToList();
					var multisiteCalculator = new MultisiteSkillDayCalculator(multisiteSkill, skillDays,
						multisiteDays, period);

					var childSkillDays =
						_skillDayRepository.FindReadOnlyRange(periodToLoad, multisiteSkill.ChildSkills, scenario)
							.ToLookup(g => (IChildSkill) g.Skill);

					foreach (var childSkill in multisiteSkill.ChildSkills)
					{
						multisiteCalculator.SetChildSkillDays(childSkill, childSkillDays[childSkill].OrderBy(s => s.CurrentDate).ToList());
					}

					multisiteCalculator.InitializeChildSkills();

					calculators.Add(multisiteCalculator);
				}
				else
				{
					calculators.Add(new SkillDayCalculator(skill, skillDays, period));
					foreach (ISkillDay skillDay in skillDays)
					{
						skillDay.RecalculateDailyTasks();
					}
				}
			}

			var calculatorsForAllSkills = calculators.OfType<MultisiteSkillDayCalculator>().SelectMany(c =>
				c.MultisiteSkill.ChildSkills.Select(s =>
					new Tuple<ISkill, IEnumerable<ISkillDay>>(s, c.GetVisibleChildSkillDays(s))));

			return calculators.Select(c => new Tuple<ISkill, IEnumerable<ISkillDay>>(c.Skill, c.SkillDays))
				.Concat(calculatorsForAllSkills).ToDictionary(c => c.Item1, v => (IEnumerable<ISkillDay>) v.Item2.ToList());
		}
	}

	
	
}