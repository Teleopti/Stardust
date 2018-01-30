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
			var person = _loggedOnUser.CurrentUser();
			var permissionInformation = person.PermissionInformation;
			var personTimeZone = permissionInformation.DefaultTimeZone();

			var overtimeRequestOpenPeriod = person.WorkflowControlSet.GetMergedOvertimeRequestOpenPeriod(
				period.ToDateTimePeriod(personTimeZone),
				new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), personTimeZone)),
				permissionInformation);

			return personSkills.Where(p => isSkillTypeMatchedInOpenPeriod(p, overtimeRequestOpenPeriod)).ToArray(); ;
		}

		private bool isSkillTypeMatchedInOpenPeriod(IPersonSkill personSkill, IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod)
		{
			var skillTypeInOvertimeRequestOpenPeriod = overtimeRequestOpenPeriod.SkillType ??
													   _skillTypeRepository.LoadAll().First(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			return personSkill.Skill.SkillType.Description.Name.Equals(skillTypeInOvertimeRequestOpenPeriod.Description.Name);
		}

	}
}