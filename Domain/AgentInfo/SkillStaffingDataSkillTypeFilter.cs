using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
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
			var defaultTimeZone = person.PermissionInformation.DefaultTimeZone();
			var phoneSkillType = _skillTypeRepository.LoadAll().FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Date);
			var filteredSkillStaffingDatas = new List<SkillStaffingData>();
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				var skillType = person.WorkflowControlSet.GetMergedOvertimeRequestOpenPeriod(
									skillStaffingDataGroup.Key.ToDateTimePeriod(defaultTimeZone),
									new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), defaultTimeZone)),
									person.PermissionInformation).SkillType ?? phoneSkillType;
				filteredSkillStaffingDatas.AddRange(skillStaffingDataGroup.Where(x => x.Skill.SkillType.Description.Name.Equals(skillType.Description.Name)));
			}
			return filteredSkillStaffingDatas;
		}
	}

	public class SkillStaffingDataSkillTypeFilterToggle47290Off : ISkillStaffingDataSkillTypeFilter
	{
		public IList<SkillStaffingData> Filter(IEnumerable<SkillStaffingData> skillStaffingDatas)
		{
			return skillStaffingDatas.ToList();
		}
	}
}