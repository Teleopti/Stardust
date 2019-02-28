using System;
using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Forecast source repository
    /// </summary>
    public class WorkloadRepository : Repository<IWorkload>, IWorkloadRepository
    {
		public static WorkloadRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new WorkloadRepository(currentUnitOfWork, null, null);
		}

		public static WorkloadRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new WorkloadRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public WorkloadRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
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
    }
}
