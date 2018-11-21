using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Audit
{
	public class StaffingContextReaderService : IStaffingContextReaderService
	{
		private readonly IStaffingAuditRepository _staffingAuditRepository;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IUserCulture _userCulture;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;

		public StaffingContextReaderService(IStaffingAuditRepository staffingAuditRepository, ISkillCombinationResourceRepository skillCombinationResourceRepository, IUserCulture userCulture, ICommonAgentNameProvider commonAgentNameProvider)
		{
			_staffingAuditRepository = staffingAuditRepository;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_userCulture = userCulture;
			_commonAgentNameProvider = commonAgentNameProvider;
		}

		private IEnumerable<AuditServiceModel> getAuditServiceModel(IEnumerable<IStaffingAudit> staffingAudit)
		{
			var auditServiceModelList = new List<AuditServiceModel>();
			var commonAgentNameSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			foreach (var audit in staffingAudit)
			{
				var auditServiceModel = new AuditServiceModel()
				{
					TimeStamp = audit.TimeStamp, Context = "Staffing", Action = audit.Action,
					ActionPerformedBy = extractPersonAuditInfo(audit.ActionPerformedBy, commonAgentNameSetting)
				};
				if (audit.Action.Equals(StaffingAuditActionConstants.ImportBpo))
					auditServiceModel.Data = $"File name: {audit.ImportFileName}";
				else
				{
					//var deserialized = JsonConvert.DeserializeObject<ClearBpoActionObj>(audit.Data);
					var bpoName = _skillCombinationResourceRepository.GetSourceBpoByGuid(audit.BpoId.GetValueOrDefault());
					var startDate = audit.ClearPeriodStart.Value.ToString("d", _userCulture.GetCulture());
					var endDate = audit.ClearPeriodEnd.Value.ToString("d", _userCulture.GetCulture());
					auditServiceModel.Data = $"BPO name: {bpoName}{Environment.NewLine}Period from {startDate} to {endDate}";
				}

				auditServiceModelList.Add(auditServiceModel);
			}

			return auditServiceModelList;
		}

		private string extractPersonAuditInfo(string person, ICommonNameDescriptionSetting commonAgentNameSetting)
		{
			var personInfo = JsonConvert.DeserializeObject<PersonAuditInfo>(person);
			return commonAgentNameSetting.BuildFor(personInfo);
		}

		public IEnumerable<AuditServiceModel> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate)
		{
			var staffingAudit = _staffingAuditRepository.LoadAudits(personId, startDate, endDate);

			return getAuditServiceModel(staffingAudit);
		}

		
	}
}
