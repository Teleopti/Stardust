using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Audit
{
	public class ScheduleAuditTrailReport : IScheduleAuditTrailReport
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ScheduleAuditTrailReport(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}
		public IEnumerable<IPerson> RevisionPeople()
		{
			return _currentUnitOfWork.Current().Session().GetNamedQuery("RevisionPeople").List<IPerson>();
		}
	}
}
