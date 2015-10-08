using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class WorkflowControlSetRepository : Repository<IWorkflowControlSet>, IWorkflowControlSetRepository
    {
        public WorkflowControlSetRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

				public WorkflowControlSetRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        public IList<IWorkflowControlSet> LoadAllSortByName()
        {
            var list = Session.CreateCriteria<WorkflowControlSet>()
                .AddOrder(Order.Asc("Name"))
                .SetFetchMode("AbsenceRequestOpenPeriods",FetchMode.Join)
                .SetFetchMode("AbsenceRequestOpenPeriods.Absence",FetchMode.Join)
                .SetFetchMode("MustMatchSkills", FetchMode.Join)
                .SetFetchMode("AllowedPreferenceDayOffs", FetchMode.Join)
                .SetFetchMode("AllowedPreferenceShiftCategories", FetchMode.Join)
                .SetFetchMode("AllowedPreferenceAbsences", FetchMode.Join)
                .SetFetchMode("AllowedAbsencesForReport", FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IWorkflowControlSet>();
            foreach (var workflowControlSet in list)
            {
                LazyLoadingManager.Initialize(workflowControlSet.UpdatedBy);
            }
            return list;
        }
    }
}