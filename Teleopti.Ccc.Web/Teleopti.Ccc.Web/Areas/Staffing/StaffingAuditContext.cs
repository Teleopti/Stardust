using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Common.Time;
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
		private readonly INow _now;


		public StaffingAuditContext(IStaffingAuditRepository staffingAuditRepository, ILoggedOnUser loggedOnUser,INow now)
		{
			_staffingAuditRepository = staffingAuditRepository;
			_loggedOnUser = loggedOnUser;
			_now = now;
		}

		public void Handle(ClearBpoActionObj clearBpoAction)
		{
			var staffingAudit = new StaffingAudit(_loggedOnUser.CurrentUser(),  "ClearBpoStaffing", "Success", JsonConvert.SerializeObject(clearBpoAction));
			staffingAudit.TimeStamp = _now.UtcDateTime();
			_staffingAuditRepository.Add(staffingAudit);
		}

		public void Handle(ImportBpoActionObj importBpoAction)
		{
			var staffingAudit = new StaffingAudit(_loggedOnUser.CurrentUser(), "ImportBpo", "Success", importBpoAction.FileName);
			staffingAudit.TimeStamp = _now.UtcDateTime();
			_staffingAuditRepository.Add(staffingAudit);
		}
	}

	
}