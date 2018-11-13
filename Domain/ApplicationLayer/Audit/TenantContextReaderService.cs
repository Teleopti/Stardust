﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
{
	public class TenantContextReaderService
	{
		private readonly ITenantAuditRepository _tenantAuditRepository;
		private readonly IUserCulture _userCulture;

		public TenantContextReaderService(ITenantAuditRepository tenantAuditRepository, IUserCulture userCulture)
		{
			_tenantAuditRepository = tenantAuditRepository;
			_userCulture = userCulture;
		}

		public IEnumerable<AuditServiceModel> LoadAll()
		{
			var tenantAudit = _tenantAuditRepository.LoadAll();

			return getAuditServiceModel(tenantAudit);

		}

		private IEnumerable<AuditServiceModel> getAuditServiceModel(IEnumerable<ITenantAudit> tenantAudit)
		{
			var auditServiceModelList = new List<AuditServiceModel>();
			foreach (var audit in tenantAudit)
			{
				var auditServiceModel = new AuditServiceModel
				{
					TimeStamp = audit.TimeStamp,
					Context = "TenantAudit",
					Action = audit.Action,
					ActionPerformedBy = audit.ActionPerformedBy.ToString()
				};
				auditServiceModel.Data = "";
				auditServiceModelList.Add(auditServiceModel);
			}
			return auditServiceModelList;
		}

		public class TenantAuditModel
		{
			public DateTime StartDate;
			public DateTime EndDate;
			public string Name;
		}

		public IEnumerable<AuditServiceModel> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate)
		{
			var staffingAudit = _tenantAuditRepository.LoadAudits(personId, startDate, endDate);

			return getAuditServiceModel(staffingAudit);
		}
	}
}