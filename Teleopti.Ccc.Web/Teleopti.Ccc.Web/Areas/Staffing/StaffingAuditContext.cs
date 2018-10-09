using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Web.Areas.Global.Aspect;
using Teleopti.Ccc.Web.Areas.Staffing.Controllers;

namespace Teleopti.Ccc.Web.Areas.Staffing
{
	public class StaffingAuditContext : IHandleContextAction<ClearBpoActionObj>, IHandleContextAction<ImportBpoActionObj>
	{
		private readonly IStaffingAuditRepository _staffingAuditRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public StaffingAuditContext(IStaffingAuditRepository staffingAuditRepository, ILoggedOnUser loggedOnUser)
		{
			_staffingAuditRepository = staffingAuditRepository;
			_loggedOnUser = loggedOnUser;
		}

		public void Handle(ClearBpoActionObj clearBpoAction)
		{
			var staffingAudit = new StaffingAudit(_loggedOnUser.CurrentUser(), null, "ClearBpoStaffing", "", "", null);
			_staffingAuditRepository.Persist(staffingAudit);
		}

		public void Handle(ImportBpoActionObj command)
		{
			throw new NotImplementedException();
		}
	}

	
}