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
			: base(currentUnitOfWork, null, null)
		{

		}

		public IList<IWorkflowControlSet> LoadAllSortByName()
		{
			Session.CreateCriteria<WorkflowControlSet>()
				.Fetch("MustMatchSkills")
				.List();
			Session.CreateCriteria<WorkflowControlSet>()
				.Fetch("AllowedPreferenceDayOffs")
				.List();
			Session.CreateCriteria<WorkflowControlSet>()
				  .Fetch("AllowedPreferenceShiftCategories")
				  .List();
			Session.CreateCriteria<WorkflowControlSet>()
				  .Fetch("AllowedPreferenceAbsences")
				  .List();
			Session.CreateCriteria<WorkflowControlSet>()
				  .Fetch("AllowedAbsencesForReport")
				  .List();
			var list = Session.CreateCriteria<WorkflowControlSet>()
					 .AddOrder(Order.Asc("Name"))
					 .Fetch("AbsenceRequestOpenPeriods")
					 .Fetch("AbsenceRequestOpenPeriods.Absence")
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