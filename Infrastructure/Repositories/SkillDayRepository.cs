using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Ropository for SkillDay
	/// </summary>
	/// <remarks>
	/// Created by: robink
	/// Created date: 2008-01-08
	/// </remarks>
	public class SkillDayRepository : Repository<ISkillDay>, ISkillDayRepository, IFillWithEmptySkillDays
	{
		private readonly WorkloadDayHelper _workloadDayHelper = new WorkloadDayHelper();

		public SkillDayRepository(IUnitOfWorkFactory unitOfWorkFactory)
#pragma warning disable 618
			: base(unitOfWorkFactory)
#pragma warning restore 618
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="SkillDayRepository"/> class.
		/// </summary>
		/// <param name="unitOfWork">The unitofwork</param>
		public SkillDayRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{
		}

		public SkillDayRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
			
		}

		/// <summary>
		/// Finds the range.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <param name="skill">The skill.</param>
		/// <param name="scenario">The scenario.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-04-01
		/// </remarks>
		public ICollection<ISkillDay> FindRange(DateOnlyPeriod period,
												ISkill skill,
												IScenario scenario)
		{
			InParameter.NotNull("period", period);
			InParameter.NotNull("skillCollection", skill);
			InParameter.NotNull("scenario", scenario);

			DetachedCriteria skilldaySubquery = getSkilldaySubquery(period, skill, scenario);

		    DetachedCriteria skilldayWorkloadDay = getSkilldayWorkloadDay(skilldaySubquery);

            DetachedCriteria skilldayDataPeriod = getSkilldayDataPeriod(skilldaySubquery);

            DetachedCriteria skilldayScenario = getSkilldayScenario(skilldaySubquery);
                
			DetachedCriteria workloadDayTaskPeriod = getWorkloadDayTaskPeriod(skilldaySubquery);

			DetachedCriteria workloadDayOpenHour = getWorkloadDayOpenHour(skilldaySubquery);

		    var multi = Session.CreateMultiCriteria()
		        .Add(skilldayWorkloadDay)
		        .Add(skilldayDataPeriod)
                .Add(skilldayScenario)
		        .Add(workloadDayTaskPeriod)
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

        public ICollection<ISkillDay> FindRange(DateOnlyPeriod period, IList<ISkill> skills, IScenario scenario)
        {
            InParameter.NotNull("period", period);
            InParameter.NotNull("skillCollection", skills);
            InParameter.NotNull("scenario", scenario);

            var skillDays = new List<ISkillDay>();
            foreach (var skillBatch in skills.Batch(200))
            {
                var skilldaySubquery = DetachedCriteria.For<SkillDay>("skillDay")
                .Add(Restrictions.Eq("skillDay.Scenario", scenario))
                .Add(Restrictions.Between("skillDay.CurrentDate", period.StartDate, period.EndDate))
                .Add(Restrictions.In("Skill", skillBatch.ToArray()))
                
                .SetProjection(Projections.Property("skillDay.Id"));

                DetachedCriteria skilldayWorkloadDay = getSkilldayWorkloadDay(skilldaySubquery);

                DetachedCriteria skilldayDataPeriod = getSkilldayDataPeriod(skilldaySubquery);

                DetachedCriteria skilldayScenario = getSkilldayScenario(skilldaySubquery);

                DetachedCriteria workloadDayTaskPeriod = getWorkloadDayTaskPeriod(skilldaySubquery);

                DetachedCriteria workloadDayOpenHour = getWorkloadDayOpenHour(skilldaySubquery);

                var multi = Session.CreateMultiCriteria()
                    .Add(skilldayWorkloadDay)
                    .Add(skilldayDataPeriod)
                    .Add(skilldayScenario)
                    .Add(workloadDayTaskPeriod)
                    .Add(workloadDayOpenHour);

                skillDays.AddRange(CollectionHelper.ToDistinctGenericCollection<ISkillDay>(wrapMultiCriteria(multi)));
            }
            

            foreach (ISkillDay skillDay in skillDays)
            {
                skillDay.SetupSkillDay();
                skillDay.SkillDayCalculator = new SkillDayCalculator(skillDay.Skill, new List<ISkillDay> { skillDay },
                                                                     new DateOnlyPeriod());
            }
            return skillDays;
        }

	    private static DetachedCriteria getWorkloadDayOpenHour(DetachedCriteria skilldaySubquery)
	    {
	        return DetachedCriteria.For<WorkloadDay>()
	            .Add(Subqueries.PropertyIn("Parent", skilldaySubquery))
	            .SetFetchMode("OpenHourList", FetchMode.Join);
	    }

	    private static DetachedCriteria getWorkloadDayTaskPeriod(DetachedCriteria skilldaySubquery)
	    {
	        return DetachedCriteria.For<WorkloadDay>()
	            .Add(Subqueries.PropertyIn("Parent", skilldaySubquery))
	            .SetFetchMode("TaskPeriodList", FetchMode.Join);
	    }

	    private static DetachedCriteria getSkilldayScenario(DetachedCriteria skilldaySubquery)
	    {
	        return DetachedCriteria.For<SkillDay>("skillDay")
	            .Add(Subqueries.PropertyIn("Id", skilldaySubquery))
	            .SetFetchMode("Scenario", FetchMode.Join);
	    }

	    private static DetachedCriteria getSkilldayDataPeriod(DetachedCriteria skilldaySubquery)
	    {
	        return DetachedCriteria.For<SkillDay>("skillDay")
	            .Add(Subqueries.PropertyIn("Id", skilldaySubquery))
	            .SetFetchMode("SkillDataPeriodCollection", FetchMode.Join);
	    }

	    private static DetachedCriteria getSkilldayWorkloadDay(DetachedCriteria skilldaySubquery)
	    {
	        return DetachedCriteria.For<SkillDay>("skillDay")
	            .Add(Subqueries.PropertyIn("Id", skilldaySubquery))
	            .SetFetchMode("WorkloadDayCollection", FetchMode.Join);
	    }

	    private static DetachedCriteria getSkilldaySubquery(DateOnlyPeriod period, ISkill skill, IScenario scenario)
	    {
	        return DetachedCriteria.For<SkillDay>("skillDay")
	            .Add(Restrictions.Eq("skillDay.Scenario", scenario))
	            .Add(Restrictions.Between("skillDay.CurrentDate", period.StartDate, period.EndDate))
	            .Add(skillCriterion(skill))
	            .SetProjection(Projections.Property("skillDay.Id"));
	    }

		/// <summary>
		/// Gets all skill days.
		/// Temprorary function! GUI should use skill days from now on...
		/// </summary>
		/// <param name="period">The period.</param>
		/// <param name="skillDays">The skill days.</param>
		/// <param name="skill">The skill.</param>
		/// <param name="scenario">The scenario.</param>
		/// <param name="addToRepository">if set to <c>true</c> [add to repository].</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-25
		/// </remarks>
		public ICollection<ISkillDay> GetAllSkillDays(DateOnlyPeriod period, ICollection<ISkillDay> skillDays, ISkill skill, IScenario scenario, Action<IEnumerable<ISkillDay>> optionalAction)
		{
		    var uniqueDays = period.DayCollection();
			var datesToProcess = uniqueDays.Except(skillDays.Select(s => s.CurrentDate));

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
				if (optionalAction!=null) optionalAction(skillDaysToRepository);
			}

			_workloadDayHelper.CreateLongtermWorkloadDays(skill, skillDays);

			var ret = skillDays.OrderBy(wd => wd.CurrentDate).ToList();
			return ret;
		}

		private static ICriterion skillCriterion(ISkill skill)
		{
			return Restrictions.Eq("skillDay.Skill", skill);
		}

        

		/// <summary>
		/// Finds the last skill day date.
		/// </summary>
		/// <param name="workload">The workload.</param>
		/// <param name="scenario">The scenario.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2008-04-08
		/// </remarks>
		public DateOnly FindLastSkillDayDate(IWorkload workload, IScenario scenario)
		{
			InParameter.NotNull("workload", workload);
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

		/// <summary>
		/// Deletes the specified date time period.
		/// </summary>
		/// <param name="dateTimePeriod">The date time period.</param>
		/// <param name="skill">The skill.</param>
		/// <param name="scenario">The scenario.</param>
		/// <remarks>
		/// Created by: henryg
		/// Created date: 2008-12-11
		/// </remarks>
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
