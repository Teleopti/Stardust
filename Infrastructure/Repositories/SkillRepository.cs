using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Multi;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
    /// SkillRepository class
    /// </summary>
    public class SkillRepository : Repository<ISkill>, ISkillRepository
    {
        public SkillRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public SkillRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        /// <summary>
        /// Finds all and include workload and queues.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-12-07
        /// </remarks>
        public ICollection<ISkill> FindAllWithWorkloadAndQueues()
        {
            var skills = Session.CreateCriteria(typeof (Skill))
                .Fetch("Activity")
                .Fetch("SkillType")
                .Fetch("WorkloadCollection")
                .AddOrder(Order.Asc("Name"))
				.Future<ISkill>();

			Session.CreateCriteria(typeof(Workload))
				.Fetch("QueueSourceCollection")
				.Future<Workload>();

            Session.CreateCriteria(typeof (QueueSource))
				.Future<QueueSource>();
			
            return skills.Distinct().ToList();
        }

        /// <summary>
        /// Finds all without multisite skills.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-07
        /// </remarks>
        public ICollection<ISkill> FindAllWithoutMultisiteSkills()
        {
            ICollection<ISkill> retList = Session.CreateCriteria(typeof(Skill),"skill")
                .Add(Restrictions.Not(Property.ForName("skill.class").Eq(typeof(MultisiteSkill))))
                .AddOrder(Order.Asc("Name"))
                .List<ISkill>();

            return retList;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public ICollection<ISkill> FindAllWithSkillDays(DateOnlyPeriod periodWithSkillDays)
        {
            DetachedCriteria subQuery = DetachedCriteria.For(typeof(SkillDay))
                .SetProjection(Projections.Property("Skill"))
                .Add(Property.ForName("Skill").EqProperty("skill.Id"))
                .Add(Restrictions.Between("CurrentDate", periodWithSkillDays.StartDate,
                                          periodWithSkillDays.EndDate));

			var skills = Session.CreateCriteria(typeof(Skill), "skill")
				.Fetch("SkillType")
				.Fetch("Activity")
				.Fetch("WorkloadCollection")
				.Add(Subqueries.Exists(subQuery))
				.Add(Restrictions.Not(Property.ForName("skill.class").Eq(typeof(ChildSkill))))
				.Future<ISkill>();

			Session.CreateCriteria<MultisiteSkill>("skill")
				.Fetch("SkillType")
				.Fetch("Activity")
				.Fetch("WorkloadCollection")
				.Fetch("ChildSkills")
				.Add(Subqueries.Exists(subQuery))
				.Future<MultisiteSkill>();

			Session.CreateCriteria<ChildSkill>()
				.Fetch("SkillType")
				.Fetch("Activity")
				.Fetch("WorkloadCollection")
				.Future<ChildSkill>();

			Session.CreateCriteria<Workload>()
				.Fetch("QueueSourceCollection")
				.Future<Workload>();

			Session.CreateCriteria<QueueSource>()
				.Future<QueueSource>();
			
			return skills.Concat(skills.OfType<IMultisiteSkill>().SelectMany(m => m.ChildSkills)).Distinct().ToList();
        }

        public ISkill LoadSkill(ISkill skill)
        {
            var workloadIds = getWorkloadIds(skill);
            var workloads = getWorkloads(skill);
            var queues = getQueues(workloadIds);
            var templateIds = getWorkloadDayTemplateIds(workloadIds);
            var templates = getWorkloadTemplates(workloadIds);
            var openhours = getOpenhours(templateIds);
            var taskPeriods = getTaskPeriods(templateIds);
            var skillDetail = getSkillDetail(skill);

            var multiCriteria = Session.CreateQueryBatch().Add<Skill>(workloads).Add<Workload>(queues).Add<Workload>(templates).Add<WorkloadDayBase>(openhours).Add<WorkloadDayBase>(taskPeriods).Add<Skill>(skillDetail);
            var fetchedSkill = CollectionHelper.ToDistinctGenericCollection<ISkill>(multiCriteria.GetResult<Skill>(0)).FirstOrDefault();

            return fetchedSkill;
        }

		public IEnumerable<ISkill> LoadAllSkills()
		{
			DetachedCriteria workloadSubquery = DetachedCriteria.For<Workload>("w")
				.SetProjection(Projections.Property("w.Id"));

			var skills = Session.CreateCriteria<Skill>("skill")
				.Fetch("WorkloadCollection")
				.Future<Skill>();

			Session.CreateCriteria<Workload>("workload")
				.Fetch("TemplateWeekCollection")
				.Future<Workload>();
			
			Session.CreateCriteria<WorkloadDayTemplate>()
				.Add(Subqueries.PropertyIn("Parent", workloadSubquery))
				.Fetch("OpenHourList")
				.Fetch("TaskPeriodList")
				.Future<WorkloadDayTemplate>();

			return skills.Distinct().ToList();
		}

		public IMultisiteSkill LoadMultisiteSkill(ISkill skill)
        {
            var workloadIds = getWorkloadIds(skill);
            var workloads = getWorkloads(skill);
            var queues = getQueues(workloadIds);
            var templateIds = getWorkloadDayTemplateIds(workloadIds);
            var templates = getWorkloadTemplates(workloadIds);
            var openhours = getOpenhours(templateIds);
            var taskPeriods = getTaskPeriods(templateIds);

            var multisiteDayTemplates = DetachedCriteria.For<Skill>()
                .Add(Restrictions.Eq("Id", skill.Id.GetValueOrDefault()))
                .Fetch("TemplateMultisiteWeekCollection");

            var templateMultisitePeriods = DetachedCriteria.For<MultisiteDayTemplate>()
                .Add(Restrictions.Eq("Parent", skill))
                .Fetch("TemplateMultisitePeriodCollection")
                .SetResultTransformer(Transformers.DistinctRootEntity);

            var multisiteDayTemplateIds = DetachedCriteria.For<MultisiteDayTemplate>()
                .Add(Restrictions.Eq("Parent", skill))
                .SetProjection(Projections.Property("Id")).SetResultTransformer(Transformers.DistinctRootEntity);

            var templateMultisitePeriodIds = DetachedCriteria.For<TemplateMultisitePeriod>()
                .Add(Subqueries.PropertyIn("Parent", multisiteDayTemplateIds))
                .SetProjection(Projections.Property("Id")).SetResultTransformer(Transformers.DistinctRootEntity);

            var distributions = DetachedCriteria.For<TemplateMultisitePeriod>()
                .Add(Subqueries.PropertyIn("Id",templateMultisitePeriodIds))
                .Fetch("Distribution")
                .SetResultTransformer(Transformers.DistinctRootEntity);

            var childSkills = DetachedCriteria.For<Skill>()
                .Add(Restrictions.Eq("Id", skill.Id.GetValueOrDefault())).Fetch("ChildSkills");
            var childSkillsSubQuery = DetachedCriteria.For<Skill>()
                .Add(Restrictions.Eq("ParentSkill", skill))
                .SetProjection(Projections.Property("Id")).SetResultTransformer(Transformers.DistinctRootEntity);
            var childSkillsDetail = getChildSkillDetail(childSkillsSubQuery);

            var multiCriteria =
                Session.CreateQueryBatch().Add<Skill>(workloads).Add<Workload>(queues).Add<Workload>(templates).Add<WorkloadDayBase>(openhours).
                    Add<WorkloadDayBase>(taskPeriods).Add<Skill>(multisiteDayTemplates).Add<MultisiteDayTemplate>(templateMultisitePeriods).Add<TemplateMultisitePeriod>(distributions)
					.Add<Skill>(childSkills).Add<Skill>(childSkillsDetail).Add<Skill>(getSkillDetail(skill));

            return CollectionHelper.ToDistinctGenericCollection<IMultisiteSkill>(multiCriteria.GetResult<Skill>(0)).FirstOrDefault();
        }

        private DetachedCriteria getSkillDetail(ISkill skill)
        {
            return DetachedCriteria.For<Skill>()
                .Add(Restrictions.Eq("Id", skill.Id.GetValueOrDefault()))
                .Fetch("SkillType")
                .Fetch("TemplateWeekCollection")
                .Fetch("TemplateWeekCollection.TemplateSkillDataPeriodCollection");
        }

        private DetachedCriteria getChildSkillDetail(DetachedCriteria subQuery)
        {
            return DetachedCriteria.For<Skill>()
                .Add(Subqueries.PropertyIn("Id", subQuery))
                .Fetch("SkillType")
                .Fetch("TemplateWeekCollection")
                .Fetch("TemplateWeekCollection.TemplateSkillDataPeriodCollection");
        }

        private DetachedCriteria getTaskPeriods(DetachedCriteria templateIds)
        {
            return DetachedCriteria.For<WorkloadDayBase>()
                .Add(Subqueries.PropertyIn("Id", templateIds))
                .Fetch("TaskPeriodList")
                .SetResultTransformer(Transformers.DistinctRootEntity);
        }

        private DetachedCriteria getOpenhours(DetachedCriteria templateIds)
        {
            return DetachedCriteria.For<WorkloadDayBase>()
                .Add(Subqueries.PropertyIn("Id", templateIds))
                .Fetch("OpenHourList").SetResultTransformer(Transformers.DistinctRootEntity);
        }

        private DetachedCriteria getWorkloadTemplates(DetachedCriteria workloadIds)
        {
            return DetachedCriteria.For<Workload>()
                .Add(Subqueries.PropertyIn("Id", workloadIds))
                .Fetch("TemplateWeekCollection");
        }

        private DetachedCriteria getWorkloadDayTemplateIds(DetachedCriteria workloadIds)
        {
            return DetachedCriteria.For<WorkloadDayTemplate>()
                .Add(Subqueries.PropertyIn("Parent", workloadIds))
                .SetProjection(Projections.Property("Id")).SetResultTransformer(Transformers.DistinctRootEntity);
        }

		private DetachedCriteria getQueues(DetachedCriteria workloadIds)
        {
            return DetachedCriteria.For<Workload>()
                .Add(Subqueries.PropertyIn("Id", workloadIds))
                .Fetch("QueueSourceCollection");
        }

        private DetachedCriteria getWorkloads(ISkill skill)
        {
            return DetachedCriteria.For<Skill>()
                .Add(Restrictions.Eq("Id", skill.Id.GetValueOrDefault()))
                .Fetch("WorkloadCollection");
        }

		private DetachedCriteria getWorkloadIds(ISkill skill)
        {
            return DetachedCriteria.For<Workload>()
                .Add(Restrictions.Eq("Skill", skill))
                .SetProjection(Projections.Property("Id")).SetResultTransformer(Transformers.DistinctRootEntity);
        }
		
		public IEnumerable<ISkill> LoadInboundTelephonySkills(int defaultResolution)
        {
            return Session.GetNamedQuery("loadInboundTelephonySkills")
                .SetEnum("forecastSource", ForecastSource.InboundTelephony)
                .SetInt32("defaultResolution", defaultResolution)
                .List<ISkill>();
        }

		public ICollection<ISkill> LoadSkills(IEnumerable<Guid> skillIdList)
		{
			ICollection<ISkill> retList = Session.CreateCriteria(typeof(Skill), "skill")
				.Add(Restrictions.InG("Id", skillIdList))
				.AddOrder(Order.Asc("Name"))
				.List<ISkill>();

			return retList;
		}
		
		public IEnumerable<ISkill> FindSkillsContain(string searchString, int maxHits)
		{
			if (maxHits < 1)
				return Enumerable.Empty<ISkill>();

			return Session.CreateCriteria<Skill>()
				.Add(Restrictions.Like("Name", searchString, MatchMode.Anywhere))
				.SetMaxResults(maxHits)
				.List<ISkill>();
		}
		
			public IEnumerable<ISkill> FindSkillsWithAtLeastOneQueueSource()
			{
				return Session.GetNamedQuery("findSkillsWithAtLeastOneQueueSource")
					.SetResultTransformer(Transformers.DistinctRootEntity)
					.List<ISkill>()
					.OrderBy(y => y.Name);
			}

		public IEnumerable<SkillOpenHoursLight> FindOpenHoursForSkills(IEnumerable<Guid> skillIds)
		{
			var query = Session.CreateSQLQuery(
				@"SELECT w.Skill as SkillId, s.TimeZone as TimeZoneId, wdt.WeekdayIndex, ohl.Minimum as StartTimeTicks, ohl.Maximum as EndTimeTicks  
 FROM [WorkloadDayTemplate] wdt WITH (NOLOCK)
 inner join  OpenHourList ohl WITH (NOLOCK) on ohl.Parent=wdt.WorkloadDayBase 
 inner join workload w WITH (NOLOCK) on w.Id=wdt.Parent
 inner join skill s WITH (NOLOCK) on w.Skill=s.id  WHERE Skill IN(:skillIdList) AND s.IsDeleted = 0 AND w.IsDeleted = 0");

			return query.SetParameterList("skillIdList", skillIds)
				.SetResultTransformer(Transformers.AliasToBean(typeof(SkillOpenHoursLight)))
				.SetReadOnly(true)
				.List<SkillOpenHoursLight>();
		}
	}
}