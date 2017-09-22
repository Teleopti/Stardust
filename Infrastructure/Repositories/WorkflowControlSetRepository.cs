using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class WorkflowControlSetRepository : Repository<IWorkflowControlSet>, IWorkflowControlSetRepository
	{
#pragma warning disable 618
		public WorkflowControlSetRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
		{
		}

		public WorkflowControlSetRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{

		}

		public IList<IWorkflowControlSet> LoadAllSortByName()
		{
			Session.CreateCriteria<WorkflowControlSet>()
				.SetFetchMode("MustMatchSkills", FetchMode.Join)
				.List();
			Session.CreateCriteria<WorkflowControlSet>()
				.SetFetchMode("AllowedPreferenceDayOffs", FetchMode.Join)
				.List();
			Session.CreateCriteria<WorkflowControlSet>()
				  .SetFetchMode("AllowedPreferenceShiftCategories", FetchMode.Join)
				  .List();
			Session.CreateCriteria<WorkflowControlSet>()
				  .SetFetchMode("AllowedPreferenceAbsences", FetchMode.Join)
				  .List();
			Session.CreateCriteria<WorkflowControlSet>()
				  .SetFetchMode("AllowedAbsencesForReport", FetchMode.Join)
				  .List();
			var list = Session.CreateCriteria<WorkflowControlSet>()
					 .AddOrder(Order.Asc("Name"))
					 .SetFetchMode("AbsenceRequestOpenPeriods", FetchMode.Join)
					 .SetFetchMode("AbsenceRequestOpenPeriods.Absence", FetchMode.Join)
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