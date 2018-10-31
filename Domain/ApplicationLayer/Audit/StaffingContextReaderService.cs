using System;
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
					auditServiceModel.Data = $"File name: {audit.ImportFileName}";
				else
				{
					//var deserialized = JsonConvert.DeserializeObject<ClearBpoActionObj>(audit.Data);
					var bpoName = _skillCombinationResourceRepository.LoadActiveBpos()
						.FirstOrDefault(x => x.Id.Equals(audit.BpoId)).Source;
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
	}

	public class AuditServiceModel
	{
		public DateTime TimeStamp { get; set; }
		public string ActionPerformedBy{ get; set; }
		public string Action { get; set; }
		public string Context { get; set; }
		public string Data { get; set; }
	}
}