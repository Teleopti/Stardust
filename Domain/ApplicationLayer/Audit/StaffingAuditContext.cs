﻿using System;
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
			var staffingAudit = new StaffingAudit(_loggedOnUser.CurrentUser(), StaffingAuditActionConstants.ClearBPO, "BPO", "", Guid.NewGuid(), clearBpoAction.StartDate,clearBpoAction.EndDate);
			staffingAudit.TimeStamp = _now.UtcDateTime();
			_staffingAuditRepository.Add(staffingAudit);
		}

		public void Handle(ImportBpoActionObj importBpoAction)
		{
			var staffingAudit = new StaffingAudit(_loggedOnUser.CurrentUser(), StaffingAuditActionConstants.ImportBPO, "BPO", "2015-10-05IMPORTFILEFORTELIA");
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
		public const string ImportBPO = "ImportBPO";
		public const string ClearBPO = "ClearBpoStaffing";
	}

}