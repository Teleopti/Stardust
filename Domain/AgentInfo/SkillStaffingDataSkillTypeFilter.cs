using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class SkillStaffingDataSkillTypeFilter : ISkillStaffingDataSkillTypeFilter
	{
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IOvertimeRequestOpenPeriodMerger _overtimeRequestOpenPeriodMerger;

		public SkillStaffingDataSkillTypeFilter(ISkillTypeRepository skillTypeRepository, ILoggedOnUser loggedOnUser, IOvertimeRequestOpenPeriodMerger overtimeRequestOpenPeriodMerger)
		{
			_skillTypeRepository = skillTypeRepository;
			_loggedOnUser = loggedOnUser;
			_overtimeRequestOpenPeriodMerger = overtimeRequestOpenPeriodMerger;
		}

		public IList<SkillStaffingData> Filter(IEnumerable<SkillStaffingData> skillStaffingDatas)
		{
			var person = _loggedOnUser.CurrentUser();
			var phoneSkillType = _skillTypeRepository.LoadAll().FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Date);
			var filteredSkillStaffingDatas = new List<SkillStaffingData>();
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				var mergedOvertimeRequestOpenPeriods = _overtimeRequestOpenPeriodMerger.GetMergedOvertimeRequestOpenPeriods(person.WorkflowControlSet.OvertimeRequestOpenPeriods, person.PermissionInformation, skillStaffingDataGroup.Key.ToDateOnlyPeriod())
														.Where(mergedOpenPeriod => mergedOpenPeriod.AutoGrantType != OvertimeRequestAutoGrantType.Deny);


				var skillTypeNames = mergedOvertimeRequestOpenPeriods.Select(o => (o.SkillType ?? phoneSkillType).Description.Name);

				filteredSkillStaffingDatas.AddRange(
					skillStaffingDataGroup.Where(x => skillTypeNames.Contains(x.Skill.SkillType.Description.Name)));
			}
			return filteredSkillStaffingDatas;
		}
	}
}