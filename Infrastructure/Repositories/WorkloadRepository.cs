using NHibernate;
using Teleopti.Ccc.Domain.Repositories;
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

				public WorkloadRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
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
