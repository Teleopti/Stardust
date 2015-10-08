using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

        public SkillRepository(IUnitOfWorkFactory unitOfWorkFactory)
#pragma warning disable 618
            : base(unitOfWorkFactory)
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
            ICriteria query1 = Session.CreateCriteria(typeof (Skill))
                .SetFetchMode("Activity", FetchMode.Join)
                .SetFetchMode("SkillType", FetchMode.Join)
                .SetFetchMode("WorkloadCollection", FetchMode.Join)
                .AddOrder(Order.Asc("Name"));

            ICriteria query2 = Session.CreateCriteria(typeof (Workload))
                                .SetFetchMode("QueueSourceCollection", FetchMode.Join);
            ICriteria query3 = Session.CreateCriteria(typeof (QueueSource));

            var criteria = Session.CreateMultiCriteria()
                                        .Add(query1)
                                        .Add(query2)
                                        .Add(query3);

            var skills = CollectionHelper.ToDistinctGenericCollection<ISkill>(wrapMultiCriteria(criteria));
            return skills;
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
                .SetFetchMode("SkillType", FetchMode.Join)
                .Add(Subqueries.Exists(subQuery))
                .Add(Restrictions.Not(Property.ForName("skill.class").Eq(typeof(ChildSkill))))
                .List<ISkill>();

            var guidList = skills.Select(s => s.Id.GetValueOrDefault());

            Session.CreateCriteria(typeof(Skill), "skill")
                .Add(Restrictions.In("Id", guidList.ToArray()))
                .SetFetchMode("WorkloadCollection", FetchMode.Join)
                .SetFetchMode("WorkloadCollection.QueueSourceCollection", FetchMode.Join)
                .List<ISkill>();

            //Made like this because of an error where the proxy couldn't be resolved to the correct type
            foreach (var multisiteSkill in skills.OfType<IMultisiteSkill>().ToList())
            {
                multisiteSkill.ChildSkills.ForEach(s =>
                                                       {
                                                           LazyLoadingManager.Initialize(s);
                                                           LazyLoadingManager.Initialize(s.Activity);
                                                           LazyLoadingManager.Initialize(s.WorkloadCollection);
                                                           foreach (var workload in s.WorkloadCollection)
                                                           {
                                                               LazyLoadingManager.Initialize(workload.QueueSourceCollection);
                                                           }
                                                           skills.Add(s);
                                                       });
            }
            skills.ForEach(s =>
            {
                LazyLoadingManager.Initialize(s);
                LazyLoadingManager.Initialize(s.Activity);
            });
            var unique = new HashSet<ISkill>(skills);
            return new List<ISkill>(unique);
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
            var multiCriteria = Session.CreateMultiCriteria().Add(workloads).Add(queues).Add(templates).Add(openhours).Add(taskPeriods).Add(skillDetail);
            var fetchedSkill = CollectionHelper.ToDistinctGenericCollection<ISkill>(wrapMultiCriteria(multiCriteria)).FirstOrDefault();

            return fetchedSkill;
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
                .SetFetchMode("TemplateMultisiteWeekCollection", FetchMode.Join);

            var templateMultisitePeriods = DetachedCriteria.For<MultisiteDayTemplate>()
                .Add(Restrictions.Eq("Parent", skill))
                .SetFetchMode("TemplateMultisitePeriodCollection", FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity);

            var multisiteDayTemplateIds = DetachedCriteria.For<MultisiteDayTemplate>()
                .Add(Restrictions.Eq("Parent", skill))
                .SetProjection(Projections.Property("Id")).SetResultTransformer(Transformers.DistinctRootEntity);

            var templateMultisitePeriodIds = DetachedCriteria.For<TemplateMultisitePeriod>()
                .Add(Subqueries.PropertyIn("Parent", multisiteDayTemplateIds))
                .SetProjection(Projections.Property("Id")).SetResultTransformer(Transformers.DistinctRootEntity);

            var distributions = DetachedCriteria.For<TemplateMultisitePeriod>()
                .Add(Subqueries.PropertyIn("Id",templateMultisitePeriodIds))
                .SetFetchMode("Distribution", FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity);

            var childSkills = DetachedCriteria.For<Skill>()
                .Add(Restrictions.Eq("Id", skill.Id.GetValueOrDefault())).SetFetchMode("ChildSkills", FetchMode.Join);
            var childSkillsSubQuery = DetachedCriteria.For<Skill>()
                .Add(Restrictions.Eq("ParentSkill", skill))
                .SetProjection(Projections.Property("Id")).SetResultTransformer(Transformers.DistinctRootEntity);
            var childSkillsDetail = getChildSkillDetail(childSkillsSubQuery);

            var multiCriteria =
                Session.CreateMultiCriteria().Add(workloads).Add(queues).Add(templates).Add(openhours).
                    Add(taskPeriods).Add(multisiteDayTemplates).Add(templateMultisitePeriods).Add(distributions).Add(
                        childSkills).Add(childSkillsDetail).Add(getSkillDetail(skill));

            var fetchedSkill = CollectionHelper.ToDistinctGenericCollection<IMultisiteSkill>(wrapMultiCriteria(multiCriteria)).FirstOrDefault();
            return fetchedSkill;
        }

        private DetachedCriteria getSkillDetail(ISkill skill)
        {
            return DetachedCriteria.For<Skill>()
                .Add(Restrictions.Eq("Id", skill.Id.GetValueOrDefault()))
                .SetFetchMode("SkillType", FetchMode.Join)
                .SetFetchMode("TemplateWeekCollection", FetchMode.Join)
                .SetFetchMode("TemplateWeekCollection.TemplateSkillDataPeriodCollection", FetchMode.Join);
        }

        private DetachedCriteria getChildSkillDetail(DetachedCriteria subQuery)
        {
            return DetachedCriteria.For<Skill>()
                .Add(Subqueries.PropertyIn("Id", subQuery))
                .SetFetchMode("SkillType", FetchMode.Join)
                .SetFetchMode("TemplateWeekCollection", FetchMode.Join)
                .SetFetchMode("TemplateWeekCollection.TemplateSkillDataPeriodCollection", FetchMode.Join);
        }

        private DetachedCriteria getTaskPeriods(DetachedCriteria templateIds)
        {
            return DetachedCriteria.For<WorkloadDayBase>()
                .Add(Subqueries.PropertyIn("Id", templateIds))
                .SetFetchMode("TaskPeriodList", FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity);
        }

        private DetachedCriteria getOpenhours(DetachedCriteria templateIds)
        {
            return DetachedCriteria.For<WorkloadDayBase>()
                .Add(Subqueries.PropertyIn("Id", templateIds))
                .SetFetchMode("OpenHourList", FetchMode.Join).SetResultTransformer(Transformers.DistinctRootEntity);
        }

        private DetachedCriteria getWorkloadTemplates(DetachedCriteria workloadIds)
        {
            return DetachedCriteria.For<Workload>()
                .Add(Subqueries.PropertyIn("Id", workloadIds))
                .SetFetchMode("TemplateWeekCollection", FetchMode.Join);
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
                .SetFetchMode("QueueSourceCollection", FetchMode.Join);
        }

        private DetachedCriteria getWorkloads(ISkill skill)
        {
            return DetachedCriteria.For<Skill>()
                .Add(Restrictions.Eq("Id", skill.Id.GetValueOrDefault()))
                .SetFetchMode("WorkloadCollection", FetchMode.Join);
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

	    public int MinimumResolution()
	    {
		    return Session.CreateCriteria<Skill>()
				.SetProjection(Projections.Min("DefaultResolution"))
				.UniqueResult<int>();
	    }

			public IEnumerable<ISkill> FindSkillsWithAtLeastOneQueueSource()
			{
				return Session.GetNamedQuery("findSkillsWithAtLeastOneQueueSource")
					.SetResultTransformer(Transformers.DistinctRootEntity)
					.List<ISkill>();
			}
    }
}