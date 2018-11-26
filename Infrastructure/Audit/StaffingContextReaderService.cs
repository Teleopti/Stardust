using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Audit
{
	public class StaffingContextReaderService : IStaffingContextReaderService
	{
		private readonly IStaffingAuditRepository _staffingAuditRepository;
		private readonly IUserCulture _userCulture;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;

		public StaffingContextReaderService(IStaffingAuditRepository staffingAuditRepository, IUserCulture userCulture, ICommonAgentNameProvider commonAgentNameProvider)
		{
			_staffingAuditRepository = staffingAuditRepository;
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
					TimeStamp = audit.TimeStamp,
					Context = Resources.AuditTrailStaffingContext,
					Action = Resources.ResourceManager.GetString(audit.Action, _userCulture.GetCulture()) ?? audit.Action,
					ActionPerformedBy = extractPersonAuditInfo(audit.ActionPerformedBy, commonAgentNameSetting)
				};
				if (audit.Action.Equals(StaffingAuditActionConstants.ImportStaffing))
					auditServiceModel.Data = $"File name: {audit.ImportFileName}";
				else
				{
					//var deserialized = JsonConvert.DeserializeObject<ClearBpoActionObj>(audit.Data);
					var bpoName = audit.BpoName;
					var startDate = audit.ClearPeriodStart.Value.ToString("d", _userCulture.GetCulture());
					var endDate = audit.ClearPeriodEnd.Value.ToString("d", _userCulture.GetCulture());
					auditServiceModel.Data = $"{Resources.AuditTrailBpoName}: {bpoName}, " +
											 $"{Resources.AuditTrailPeriodStart} {startDate} {Resources.AuditTrailPeriodEnd} {endDate}";
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
