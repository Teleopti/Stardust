using System;
using System.Linq;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Forecast source repository
    /// </summary>
    public class WorkloadRepository : Repository<IWorkload>, IWorkloadRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkloadRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public WorkloadRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public WorkloadRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
        {
        }

        /// <summary>
        /// Removes the specified workload.
        /// Also deletes its WorkloadDays.
        /// </summary>
        /// <param name="entity">The workload.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-11-02
        /// </remarks>
        public override void Remove(IWorkload entity)
        {
            //workload.RemoveTemplate();
            base.Remove(entity);

            //string hql;
            //ISQLQuery sqlQuery;
            if (Session.Transaction != null)
            {
                ExecuteWorkloadDayDelete(entity);
            }
            else
            {
                using (ITransaction transaction = Session.BeginTransaction())
                {
                    ExecuteWorkloadDayDelete(entity);
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Executes the workload day delete.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-11-03
        /// </remarks>
        private void ExecuteWorkloadDayDelete(IWorkload workload)
        {
            string hql;
            ISQLQuery sqlQuery;
            hql = @"DELETE FROM WorkloadDay WHERE WorkloadDayBase IN (SELECT Id FROM WorkloadDayBase WHERE Workload = :workloadId)";
            sqlQuery = Session.CreateSQLQuery(hql);
            sqlQuery.SetGuid("workloadId", workload.Id.Value);
            sqlQuery.ExecuteUpdate();

            hql = @"DELETE FROM WorkloadDayTemplate WHERE WorkloadDayBase IN (SELECT Id FROM WorkloadDayBase WHERE Workload = :workloadId)";
            sqlQuery = Session.CreateSQLQuery(hql);
            sqlQuery.SetGuid("workloadId", workload.Id.Value);
            sqlQuery.ExecuteUpdate();

            hql = @"DELETE FROM TemplateTaskPeriod WHERE Parent IN (SELECT Id FROM WorkloadDayBase WHERE Workload = :workloadId)";
            sqlQuery = Session.CreateSQLQuery(hql);
            sqlQuery.SetGuid("workloadId", workload.Id.Value);
            sqlQuery.ExecuteUpdate();

            hql = @"DELETE FROM OpenHourList WHERE Parent IN (SELECT Id FROM WorkloadDayBase WHERE Workload = :workloadId)";
            sqlQuery = Session.CreateSQLQuery(hql);
            sqlQuery.SetGuid("workloadId", workload.Id.Value);
            sqlQuery.ExecuteUpdate();

            hql = @"DELETE FROM WorkloadDayBase WHERE Workload = :workloadId";
            sqlQuery = Session.CreateSQLQuery(hql);
            sqlQuery.SetGuid("workloadId", workload.Id.Value);
            sqlQuery.ExecuteUpdate();
        }

        public IWorkload LoadWorkload(IWorkload workload)
        {
            var queues = DetachedCriteria.For<Workload>()
                .Add(Restrictions.Eq("Id", workload.Id.GetValueOrDefault()))
                .SetFetchMode("QueueSourceCollection", FetchMode.Join);
            var templates = DetachedCriteria.For<Workload>()
                .Add(Restrictions.Eq("Id", workload.Id.GetValueOrDefault()))
                .SetFetchMode("TemplateWeekCollection", FetchMode.Join);
            var templateIds = DetachedCriteria.For<WorkloadDayTemplate>()
                .Add(Restrictions.Eq("Parent", workload))
                .SetProjection(Projections.Property("Id")).SetResultTransformer(Transformers.DistinctRootEntity);
            var openhours = DetachedCriteria.For<WorkloadDayBase>()
                .Add(Subqueries.PropertyIn("Id", templateIds))
                .SetFetchMode("OpenHourList", FetchMode.Join).SetResultTransformer(Transformers.DistinctRootEntity);
            var taskPeriods = DetachedCriteria.For<WorkloadDayBase>()
                 .Add(Subqueries.PropertyIn("Id", templateIds))
                 .SetFetchMode("TaskPeriodList", FetchMode.Join)
                 .SetResultTransformer(Transformers.DistinctRootEntity);
            var skill = DetachedCriteria.For<Skill>()
              .Add(Restrictions.Eq("Id", workload.Skill.Id.GetValueOrDefault()))
              .SetFetchMode("SkillType", FetchMode.Join)
              .SetFetchMode("TemplateWeekCollection", FetchMode.Join)
              .SetFetchMode("TemplateWeekCollection.TemplateSkillDataPeriodCollection", FetchMode.Join);

            var multiCriteria =
                Session.CreateMultiCriteria().Add(queues).Add(skill).Add(templates).Add(openhours).Add(taskPeriods);
            var fetchedWorkload =
                CollectionHelper.ToDistinctGenericCollection<IWorkload>(wrapMultiCriteria(multiCriteria)).FirstOrDefault();
            return fetchedWorkload;
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
