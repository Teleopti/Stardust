using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillDayRepository : Repository<ISkillDay>, ISkillDayRepository, IFillWithEmptySkillDays
	{
		private readonly WorkloadDayHelper _workloadDayHelper = new WorkloadDayHelper();

		public SkillDayRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public SkillDayRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
			
		}

		public ICollection<ISkillDay> FindRange(DateOnlyPeriod period,
												ISkill skill,
												IScenario scenario)
		{
			InParameter.NotNull(nameof(period), period);
			InParameter.NotNull(nameof(skill), skill);
			InParameter.NotNull(nameof(scenario), scenario);

			DetachedCriteria skillDayQuery = DetachedCriteria.For<SkillDay>("skillDay")
				.Add(Restrictions.Eq("skillDay.Scenario", scenario))
				.Add(Restrictions.Between("skillDay.CurrentDate", period.StartDate, period.EndDate))
				.Add(Restrictions.Eq("skillDay.Skill", skill));

			DetachedCriteria skillDaySubquery = DetachedCriteria.For<SkillDay>("skillDay")
				.Add(Restrictions.Eq("skillDay.Scenario", scenario))
				.Add(Restrictions.Between("skillDay.CurrentDate", period.StartDate, period.EndDate))
				.Add(Restrictions.Eq("skillDay.Skill", skill))
				.SetProjection(Projections.Property("skillDay.Id"));

            DetachedCriteria skilldayDataPeriod = getSkilldayDataPeriod(skillDaySubquery);

            DetachedCriteria skilldayScenario = getSkilldayScenario(scenario);
                
			DetachedCriteria workloadDayOpenHour = getWorkloadDayOpenHour(skillDaySubquery);

		    var multi = Session.CreateMultiCriteria()
		        .Add(skillDayQuery)
		        .Add(skilldayDataPeriod)
                .Add(skilldayScenario)
		        .Add(workloadDayOpenHour);

			var skillDays = CollectionHelper.ToDistinctGenericCollection<ISkillDay>(wrapMultiCriteria(multi));
			
            foreach (ISkillDay skillDay in skillDays)
			{
				skillDay.SetupSkillDay();
				skillDay.SkillDayCalculator = new SkillDayCalculator(skillDay.Skill, new List<ISkillDay> { skillDay },
																	 new DateOnlyPeriod());
			}
			return skillDays;
		}

		public bool HasSkillDaysWithinPeriod(DateOnly startDate, DateOnly endDate, IBusinessUnit businessUnit, IScenario scenario)
		{
			var result = Session.CreateSQLQuery(
					@"
IF EXISTS (SELECT * FROM SkillDay
WHERE Scenario = :scenario AND BusinessUnit = :buId AND SkillDayDate BETWEEN :startDate AND :endDate)
SELECT 1
ELSE
SELECT 0
")
				.SetGuid("scenario", scenario.Id.GetValueOrDefault())
				.SetGuid("buId", businessUnit.Id.GetValueOrDefault())
				.SetDateTime("startDate", startDate.Date)
				.SetDateTime("endDate", endDate.Date).UniqueResult<int>();
				
			return result > 0;
		}

        public ICollection<ISkillDay> FindReadOnlyRange(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario)
        {
            InParameter.NotNull(nameof(period), period);
            InParameter.NotNull(nameof(skills), skills);
            InParameter.NotNull(nameof(scenario), scenario);

            var skillDays = new List<ISkillDay>();
	        var defaultReadOnly = Session.DefaultReadOnly;
	        Session.DefaultReadOnly = true;
            foreach (var skillBatch in skills.Batch(300))
			{
				var skillsInBatch = skillBatch.ToArray();
				DetachedCriteria skillDayQuery = DetachedCriteria.For<SkillDay>("skillDay")
					.Add(Restrictions.Eq("skillDay.Scenario", scenario))
					.Add(Restrictions.Between("skillDay.CurrentDate", period.StartDate, period.EndDate))
					.Add(Restrictions.InG("skillDay.Skill", skillsInBatch));

				DetachedCriteria skillDaySubquery = DetachedCriteria.For<SkillDay>("skillDay")
					.Add(Restrictions.Eq("skillDay.Scenario", scenario))
					.Add(Restrictions.Between("skillDay.CurrentDate", period.StartDate, period.EndDate))
					.Add(Restrictions.InG("skillDay.Skill", skillsInBatch))
					.SetProjection(Projections.Property("skillDay.Id"));

				DetachedCriteria skilldayDataPeriod = getSkilldayDataPeriod(skillDaySubquery);

				DetachedCriteria skilldayScenario = getSkilldayScenario(scenario);

				DetachedCriteria workloadDayOpenHour = getWorkloadDayOpenHour(skillDaySubquery);

                var multi = Session
					.CreateMultiCriteria()
                    .Add(skillDayQuery)
                    .Add(skilldayDataPeriod)
                    .Add(skilldayScenario)
                    .Add(workloadDayOpenHour);

				var days = CollectionHelper.ToDistinctGenericCollection<ISkillDay>(wrapMultiCriteria(multi));
	            var grouped = days.GroupBy(d => d.Skill);
	            foreach (var group in grouped)
	            {
		            var calculator = new SkillDayCalculator(group.Key,group,period);
		            foreach (var skillDay in group)
		            {
						skillDay.SetupSkillDay();
						skillDay.SkillDayCalculator = calculator;
					}
	            }
				skillDays.AddRange(days);
            }
	        Session.DefaultReadOnly = defaultReadOnly;

            return skillDays;
        }

	    private static DetachedCriteria getWorkloadDayOpenHour(DetachedCriteria skilldaySubquery)
	    {
	        return DetachedCriteria.For<WorkloadDay>()
				.Add(Subqueries.PropertyIn("Parent", skilldaySubquery))
	            .SetFetchMode("OpenHourList", FetchMode.Join)
				.SetFetchMode("TaskPeriodList", FetchMode.Join);
	    }

	    private static DetachedCriteria getSkilldayScenario(IScenario scenario)
		{
			return DetachedCriteria.For<Scenario>("scenario")
				.Add(Restrictions.IdEq(scenario.Id));
		}

	    private static DetachedCriteria getSkilldayDataPeriod(DetachedCriteria skilldaySubquery)
	    {
	        return DetachedCriteria.For<SkillDataPeriod>("skillDataPeriod")
				.Add(Subqueries.PropertyIn("Parent", skilldaySubquery));
		}

		public ICollection<ISkillDay> GetAllSkillDays(DateOnlyPeriod period, ICollection<ISkillDay> skillDays, ISkill skill, IScenario scenario, Action<IEnumerable<ISkillDay>> optionalAction)
		{
		    var uniqueDays = period.DayCollection();
			var datesToProcess = uniqueDays.Except(skillDays.Select(s => s.CurrentDate)).ToArray();

			if (datesToProcess.Any())
			{
				IList<ISkillDay> skillDaysToRepository = new List<ISkillDay>();
				foreach (var uniqueDate in datesToProcess)
				{
					ISkillDay skillDay = new SkillDay();
					skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new List<ISkillDay> { skillDay }, new DateOnlyPeriod());
					ISkillDayTemplate skillDayTemplate = skill.GetTemplateAt((int)uniqueDate.DayOfWeek);

					skillDay.CreateFromTemplate(uniqueDate, skill, scenario, skillDayTemplate);

					skillDays.Add(skillDay);
					skillDaysToRepository.Add(skillDay);
				}
				optionalAction?.Invoke(skillDaysToRepository);
			}

			_workloadDayHelper.CreateLongtermWorkloadDays(skill, skillDays);

			var ret = skillDays.OrderBy(wd => wd.CurrentDate).ToList();
			return ret;
		}

		public DateOnly FindLastSkillDayDate(IWorkload workload, IScenario scenario)
		{
			InParameter.NotNull(nameof(workload), workload);
			DateOnly? latestDate = Session.CreateCriteria(typeof(SkillDay))
				.Add(Restrictions.Eq("Scenario", scenario))
				.CreateAlias("Skill", "skill")
				.CreateAlias("skill.WorkloadCollection", "wlColl", JoinType.InnerJoin)
				.Add(Restrictions.Eq("wlColl.Id", workload.Id))
				.SetProjection(Projections.Max("CurrentDate"))
				.UniqueResult<DateOnly?>();

			if (!latestDate.HasValue)
			{
				latestDate = new DateOnly(DateTime.Today.AddMonths(-1));
			}
			return latestDate.Value;
		}

		public void Delete(DateOnlyPeriod dateTimePeriod, ISkill skill, IScenario scenario)
		{
			ICollection<ISkillDay> skillDays = FindRange(dateTimePeriod, skill, scenario);
			foreach (ISkillDay skillDay in skillDays)
			{
				Remove(skillDay);
			}
			UnitOfWork.PersistAll();
		}

		public ISkillDay FindLatestUpdated(ISkill skill, IScenario scenario, bool withLongterm)
		{
			ICriterion longTerm = Restrictions.Eq("wld.TemplateReference.TemplateName", TemplateReference.LongtermTemplateKey);
			ICriterion templateNameNull = Restrictions.IsNull("wld.TemplateReference.TemplateName");
			ICriteria query = Session.CreateCriteria(typeof(SkillDay), "sd")
				.CreateAlias("WorkloadDayCollection", "wld")
				.Add(Restrictions.Eq("sd.Skill", skill))
				.Add(Restrictions.Eq("sd.Scenario", scenario));

			if (!withLongterm)
			{
				longTerm = Restrictions.Not(longTerm);
				query.Add(Restrictions.Or(longTerm, templateNameNull));
			}
			else
			{
				query.Add(Restrictions.Eq("wld.TemplateReference.TemplateName", TemplateReference.LongtermTemplateKey));
			}

			ISkillDay skillDay = query.AddOrder(Order.Desc("sd.UpdatedOn"))
				.SetMaxResults(1)
				.UniqueResult<SkillDay>();

			return skillDay;
		}

		public IEnumerable<ISkillDay> FindUpdatedSince(IScenario scenario,DateTime lastCheck)
		{
			return Session.CreateCriteria<SkillDay>("sd")
				.Add(Restrictions.Eq("sd.Scenario", scenario))
				.Add(Restrictions.Gt("sd.UpdatedOn",lastCheck))
	            .SetFetchMode("sd.Skill", FetchMode.Join)
				.CreateAlias("sd.Skill","skill")
				.Add(Restrictions.Not(Property.ForName("skill.class").Eq(typeof(ChildSkill))))
				.List<ISkillDay>();
		}

	    private static object wrapMultiCriteria(IMultiCriteria multi)
        {
            try
            {
                return multi.List()[0];
            }
            catch (Exception ex)
            {
                //temp fix until NH is upgraded - hack just for this very branch
                throw new DataSourceException(ex.Message, ex);
            }
        }
	}
}
