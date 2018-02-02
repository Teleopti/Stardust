using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduledSkillOpenHourProviderToggle47290On : ScheduledSkillOpenHourProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly INow _now;
		private readonly ISkillTypeRepository _skillTypeRepository;

		public ScheduledSkillOpenHourProviderToggle47290On(ILoggedOnUser loggedOnUser, ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider, IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider, INow now, ISkillTypeRepository skillTypeRepository) : base(loggedOnUser, supportedSkillsInIntradayProvider, staffingDataAvailablePeriodProvider)
		{
			_loggedOnUser = loggedOnUser;
			_now = now;
			_skillTypeRepository = skillTypeRepository;
		}

		protected override IPersonSkill[] filterPersonSkills(IEnumerable<IPersonSkill> personSkills, DateOnlyPeriod period)
		{
			var skillTypes = getSkillTypesInRequestOpenPeriod(period);

			return personSkills.Where(p => isSkillTypeMatchedInOpenPeriod(p, skillTypes)).ToArray();
		}

		private HashSet<ISkillType> getSkillTypesInRequestOpenPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var permissionInformation = person.PermissionInformation;
			var personTimeZone = permissionInformation.DefaultTimeZone();

			var skillTypes = new HashSet<ISkillType>();
			var phoneSkillType = _skillTypeRepository.LoadAll().FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			var viewDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), personTimeZone));
			var days = period.DayCollection();
			if (days.Count > 1)
			{
				foreach (var day in days)
				{
					var skillType = person.WorkflowControlSet.GetMergedOvertimeRequestOpenPeriod(
										day.ToDateTimePeriod(personTimeZone),
										viewDate,
										permissionInformation).SkillType ?? phoneSkillType;
					skillTypes.Add(skillType);
				}
			}
			else
			{
				var skillType = person.WorkflowControlSet.GetMergedOvertimeRequestOpenPeriod(
									period.ToDateTimePeriod(personTimeZone),
									viewDate,
									permissionInformation).SkillType ?? phoneSkillType;
				skillTypes.Add(skillType);
			}

			return skillTypes;
		}

		private bool isSkillTypeMatchedInOpenPeriod(IPersonSkill personSkill, HashSet<ISkillType> skillTypes)
		{
			return skillTypes.Contains(personSkill.Skill.SkillType);
		}
	}
}