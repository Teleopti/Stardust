using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
{
	public class PersonAccessContextReaderService
	{
		private readonly IPersonAccessAuditRepository _personAccessAuditRepository;
		private readonly IUserCulture _userCulture;
		private readonly IApplicationRoleRepository _applicationRoleRepository;

		public PersonAccessContextReaderService(IPersonAccessAuditRepository personAccessAuditRepository, IUserCulture userCulture, IApplicationRoleRepository applicationRoleRepository)
		{
			_personAccessAuditRepository = personAccessAuditRepository;
			_userCulture = userCulture;
			_applicationRoleRepository = applicationRoleRepository;
		}

		public IEnumerable<AuditServiceModel> LoadAll()
		{
			var personAccessAudits = _personAccessAuditRepository.LoadAll();

			return getAuditServiceModel(personAccessAudits);

		}

		private IEnumerable<AuditServiceModel> getAuditServiceModel(IEnumerable<IPersonAccess> personAccessAudit)
		{
			var auditServiceModelList = new List<AuditServiceModel>();
			foreach (var audit in personAccessAudit)
			{
				var auditServiceModel = new AuditServiceModel
				{
					TimeStamp = audit.TimeStamp, Context = "PersonAccess", Action = audit.Action,
					ActionPerformedBy = audit.ActionPerformedBy
				};
				var deserializedRole = JsonConvert.DeserializeObject<PersonAccessModel>(audit.Data);
				var appRole = _applicationRoleRepository.Load(deserializedRole.RoleId);
				auditServiceModel.Data = $"Role: {appRole.Name} Action: {audit.Action}";
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
	}
	public enum PersonAuditActionResult
	{
		Change,
		NoChange,
		NotPermitted
	}

	public enum PersonAuditActionType
	{
		GrantRole,
		RevokeRole,
		SingleGrantRole,
		SingleRevokeRole,
		MultiGrantRole,
		MultiRevokeRole
	}
}