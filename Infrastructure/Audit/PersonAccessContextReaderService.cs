using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Audit
{
	public class PersonAccessContextReaderService : IPersonAccessContextReaderService, IPurgeAudit
	{
		private readonly IPersonAccessAuditRepository _personAccessAuditRepository;
		private readonly IApplicationRoleRepository _applicationRoleRepository;
		private readonly IPurgeSettingRepository _purgeSettingRepository;
		private readonly INow _now;

		public PersonAccessContextReaderService(IPersonAccessAuditRepository personAccessAuditRepository, IApplicationRoleRepository applicationRoleRepository, IPurgeSettingRepository purgeSettingRepository, INow now)
		{
			_personAccessAuditRepository = personAccessAuditRepository;
			_applicationRoleRepository = applicationRoleRepository;
			_purgeSettingRepository = purgeSettingRepository;
			_now = now;
		}

		private IEnumerable<AuditServiceModel> getAuditServiceModel(IEnumerable<IPersonAccess> personAccessAudit)
		{
			var auditServiceModelList = new List<AuditServiceModel>();
			foreach (var audit in personAccessAudit)
			{
				var auditServiceModel = new AuditServiceModel
				{
					TimeStamp = audit.TimeStamp, Context = "PersonAccess", Action = audit.Action,
					ActionPerformedBy = audit.ActionPerformedBy.Name.ToString(NameOrderOption.FirstNameLastName)
				};
				var deserializedRole = JsonConvert.DeserializeObject<PersonAccessModel>(audit.Data);
				var appRole = _applicationRoleRepository.Load(deserializedRole.RoleId);
				auditServiceModel.Data = $"Person: {audit.ActionPerformedOn.Name} Role: {appRole.Name} Action: {audit.Action}";
				auditServiceModelList.Add(auditServiceModel);
			}

			return auditServiceModelList;
		}

		public class PersonAccessModel
		{
			public Guid RoleId;
			public string Name;
		}

		public IEnumerable<AuditServiceModel> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate)
		{
			var staffingAudit = _personAccessAuditRepository.LoadAudits(personId, startDate, endDate);
			return getAuditServiceModel(staffingAudit);
		}

		public void PurgeAudits()
		{
			var purgeSettings = _purgeSettingRepository.FindAllPurgeSettings();
			var monthsToKeepAuditEntry = purgeSettings.SingleOrDefault(p => p.Key == "MonthsToKeepAudit");
			var dateForPurging = _now.UtcDateTime().AddMonths(-(monthsToKeepAuditEntry?.Value ?? 3));
			_personAccessAuditRepository.PurgeOldAudits(dateForPurging);
		}
	}

	
}