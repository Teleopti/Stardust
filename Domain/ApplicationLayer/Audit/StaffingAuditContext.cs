using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
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
			var staffingAudit = new StaffingAudit(_loggedOnUser.CurrentUser(), StaffingAuditActionConstants.ClearBpo, JsonConvert.SerializeObject(clearBpoAction), "BPO");
			staffingAudit.TimeStamp = _now.UtcDateTime();
			_staffingAuditRepository.Add(staffingAudit);
		}

		public void Handle(ImportBpoActionObj importBpoAction)
		{
			var staffingAudit = new StaffingAudit(_loggedOnUser.CurrentUser(), StaffingAuditActionConstants.ImportBpo,  importBpoAction.FileName, "BPO");
			staffingAudit.TimeStamp = _now.UtcDateTime();
			_staffingAuditRepository.Add(staffingAudit);
		}
	}


	public class ImportBpoActionObj
	{
		public string FileContent { get; set; }
		public string FileName { get; set; }
	}



	public class ClearBpoActionObj
	{
		public Guid BpoGuid { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}

	public static class StaffingAuditActionConstants
	{
		public const string ImportBpo = "ImportBpo";
		public const string ClearBpo = "ClearBpoStaffing";
	}



}