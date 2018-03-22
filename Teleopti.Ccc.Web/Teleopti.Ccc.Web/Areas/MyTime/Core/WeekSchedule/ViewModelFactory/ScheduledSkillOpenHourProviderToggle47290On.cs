using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduledSkillOpenHourProviderToggle47290On : ScheduledSkillOpenHourProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IOvertimeRequestOpenPeriodProvider _overtimeRequestOpenPeriodProvider;

		public ScheduledSkillOpenHourProviderToggle47290On(ILoggedOnUser loggedOnUser,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider,
			ISkillTypeRepository skillTypeRepository, IOvertimeRequestOpenPeriodProvider overtimeRequestOpenPeriodProvider) :
			base(loggedOnUser, supportedSkillsInIntradayProvider, staffingDataAvailablePeriodProvider)
		{
			_loggedOnUser = loggedOnUser;
			_skillTypeRepository = skillTypeRepository;
			_overtimeRequestOpenPeriodProvider = overtimeRequestOpenPeriodProvider;
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
			var phoneSkillType = _skillTypeRepository.LoadAll()
				.FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			var days = period.DayCollection();

			foreach (var day in days)
			{
				var skillTypesInPeriod = _overtimeRequestOpenPeriodProvider.GetOvertimeRequestOpenPeriods(person,
						day.ToDateTimePeriod(personTimeZone)).Where(o => o.AutoGrantType != OvertimeRequestAutoGrantType.Deny)
					.Select(o => o.SkillType ?? phoneSkillType);
				skillTypes.AddRange(skillTypesInPeriod);
			}

			return skillTypes;
		}

		private bool isSkillTypeMatchedInOpenPeriod(IPersonSkill personSkill, HashSet<ISkillType> skillTypes)
		{
			return skillTypes.Contains(personSkill.Skill.SkillType);
		}
	}
}