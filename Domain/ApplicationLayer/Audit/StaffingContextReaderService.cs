﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
{
	public class StaffingContextReaderService
	{
		private readonly IStaffingAuditRepository _staffingAuditRepository;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IUserCulture _userCulture;

		public StaffingContextReaderService(IStaffingAuditRepository staffingAuditRepository, ISkillCombinationResourceRepository skillCombinationResourceRepository, IUserCulture userCulture)
		{
			_staffingAuditRepository = staffingAuditRepository;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_userCulture = userCulture;
		}

		public IEnumerable<AuditServiceModel> LoadAll()
		{
			var staffingAudit = _staffingAuditRepository.LoadAll();

			return getAuditServiceModel(staffingAudit);

		}

		private IEnumerable<AuditServiceModel> getAuditServiceModel(IEnumerable<IStaffingAudit> staffingAudit)
		{
			var auditServiceModelList = new List<AuditServiceModel>();
			foreach (var audit in staffingAudit)
			{
				var auditServiceModel = new AuditServiceModel()
				{
					TimeStamp = audit.TimeStamp, Context = "Staffing", Action = audit.Action,
					ActionPerformedBy = audit.ActionPerformedBy.Name.ToString(NameOrderOption.FirstNameLastName)
				};
				if (audit.Action.Equals(StaffingAuditActionConstants.ImportBpo))
					auditServiceModel.Data = $"File name: {audit.Data}";
				else
				{
					var deserialized = JsonConvert.DeserializeObject<ClearBpoActionObj>(audit.Data);
					//var bpoName = _skillCombinationResourceRepository.LoadActiveBpos()
					//	.FirstOrDefault(x => x.Id.Equals(deserialized.BpoGuid)).Source;
					var bpoName = _skillCombinationResourceRepository.GetSourceBpoByGuid(deserialized.BpoGuid);
					var startDate = deserialized.StartDate.Date.ToString("d", _userCulture.GetCulture());
					var endDate = deserialized.EndDate.Date.ToString("d", _userCulture.GetCulture());
					auditServiceModel.Data = $"BPO name: {bpoName} Period from {startDate} to {endDate}";
				}

				auditServiceModelList.Add(auditServiceModel);
			}

			return auditServiceModelList;
		}

		public IEnumerable<AuditServiceModel> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate)
		{
			var staffingAudit = _staffingAuditRepository.LoadAudits(personId, startDate, endDate);

			return getAuditServiceModel(staffingAudit);
		}
	}
}