using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
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
		private readonly IPurgeSettingRepository _purgeSettingRepository;
		private readonly INow _now;

		public StaffingContextReaderService(IStaffingAuditRepository staffingAuditRepository, ISkillCombinationResourceRepository skillCombinationResourceRepository, IUserCulture userCulture, IPurgeSettingRepository purgeSettingRepository, INow now)
		{
			_staffingAuditRepository = staffingAuditRepository;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_userCulture = userCulture;
			_purgeSettingRepository = purgeSettingRepository;
			_now = now;
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

		public IEnumerable<AuditServiceModel> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate)
		{
			var staffingAudit = _staffingAuditRepository.LoadAudits(personId, startDate, endDate);

			return getAuditServiceModel(staffingAudit);
		}

		public void PurgeAudits()
		{
			var purgeSettings = _purgeSettingRepository.FindAllPurgeSettings();
			var monthsToKeepAuditEntry = purgeSettings.SingleOrDefault(p => p.Key == "MonthsToKeepAudit");
			var dateForPurging = _now.UtcDateTime().AddMonths(-(monthsToKeepAuditEntry?.Value ?? 3));
			_staffingAuditRepository.PurgeOldAudits(dateForPurging);
		}
	}
}
