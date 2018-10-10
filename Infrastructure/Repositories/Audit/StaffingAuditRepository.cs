using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Repositories.Audit
{
	public class StaffingAuditRepository : Repository<IStaffingAudit>, IStaffingAuditRepository
	{
		public StaffingAuditRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}
	}
}
