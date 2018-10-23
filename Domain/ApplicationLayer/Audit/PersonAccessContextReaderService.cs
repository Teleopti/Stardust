using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
{
	public class PersonAccessContextReaderService
	{
		private readonly IPersonAccessAuditRepository _personAccessAuditRepository;
		private readonly IUserCulture _userCulture;

		public PersonAccessContextReaderService(IPersonAccessAuditRepository personAccessAuditRepository, IUserCulture userCulture)
		{
			_personAccessAuditRepository = personAccessAuditRepository;
			_userCulture = userCulture;
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
				//if (audit.Action.Equals(StaffingAuditActionConstants.ImportBpo))
				//	auditServiceModel.Data = $"File name: {audit.Data}";
				//else
				//{
				//	var deserialized = JsonConvert.DeserializeObject<ClearBpoActionObj>(audit.Data);
				//	var bpoName = _skillCombinationResourceRepository.LoadActiveBpos()
				//		.FirstOrDefault(x => x.Id.Equals(deserialized.BpoGuid)).Source;
				//	var startDate = deserialized.StartDate.Date.ToString("d", _userCulture.GetCulture());
				//	var endDate = deserialized.EndDate.Date.ToString("d", _userCulture.GetCulture());
				//	auditServiceModel.Data = $"BPO name: {bpoName}{Environment.NewLine}Period from {startDate} to {endDate}";
				//}
				//var x = new {RoleId = role.Id, Name = role.DescriptionText};
				var deserialized = JsonConvert.DeserializeObject<PersonAccessModel>(audit.Data);
				auditServiceModel.Data = $"RoleId: {deserialized.RoleId} Name: {deserialized.Name}";

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