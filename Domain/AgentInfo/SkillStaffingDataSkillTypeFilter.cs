using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class SkillStaffingDataSkillTypeFilter : ISkillStaffingDataSkillTypeFilter
	{
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly INow _now;
		private readonly ILoggedOnUser _loggedOnUser;

		public SkillStaffingDataSkillTypeFilter(ISkillTypeRepository skillTypeRepository, INow now, ILoggedOnUser loggedOnUser)
		{
			_skillTypeRepository = skillTypeRepository;
			_now = now;
			_loggedOnUser = loggedOnUser;
		}

		public IList<SkillStaffingData> Filter(IEnumerable<SkillStaffingData> skillStaffingDatas)
		{
			var person = _loggedOnUser.CurrentUser();
			var phoneSkillType = _skillTypeRepository.LoadAll().FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Date);
			var filteredSkillStaffingDatas = new List<SkillStaffingData>();
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				var overtimeRequestOpenPeriodGroups =
					new SkillTypeFlatOvertimeOpenPeriodMapper().Map(person.WorkflowControlSet.OvertimeRequestOpenPeriods, phoneSkillType)
						.Where(x =>isPeriodMatched(x, person, skillStaffingDataGroup.Key.ToDateOnlyPeriod()))
						.GroupBy(o => o.SkillType ?? phoneSkillType);

				var mergedOvertimeRequestOpenPeriods = overtimeRequestOpenPeriodGroups.Select(overtimeRequestOpenPeriodGroup => 
														new OvertimeRequestOpenPeriodMerger().Merge(overtimeRequestOpenPeriodGroup))
														.Where(mergedOpenPeriod => mergedOpenPeriod.AutoGrantType != OvertimeRequestAutoGrantType.Deny);

				var skillTypeNames = mergedOvertimeRequestOpenPeriods.Select(o => (o.SkillType ?? phoneSkillType).Description.Name);

				filteredSkillStaffingDatas.AddRange(
					skillStaffingDataGroup.Where(x => skillTypeNames.Contains(x.Skill.SkillType.Description.Name)));
			}
			return filteredSkillStaffingDatas;
		}

		private bool isPeriodMatched(OvertimeRequestSkillTypeFlatOpenPeriod overtimeRequestOpenPeriod, IPerson person,
			DateOnlyPeriod requestPeriod)
		{
			return overtimeRequestOpenPeriod.OriginPeriod.GetPeriod(new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(),
				person.PermissionInformation.DefaultTimeZone()))).Contains(requestPeriod);
		}
	}
}