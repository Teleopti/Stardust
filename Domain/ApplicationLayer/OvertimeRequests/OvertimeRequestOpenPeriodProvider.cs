using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestOpenPeriodProvider : IOvertimeRequestOpenPeriodProvider
	{
		private readonly IOvertimeRequestOpenPeriodMerger _overtimeRequestOpenPeriodMerger;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();

		public OvertimeRequestOpenPeriodProvider(IOvertimeRequestOpenPeriodMerger overtimeRequestOpenPeriodMerger)
		{
			_overtimeRequestOpenPeriodMerger = overtimeRequestOpenPeriodMerger;
		}

		public IList<OvertimeRequestSkillTypeFlatOpenPeriod> GetOvertimeRequestOpenPeriods(IPerson person, DateTimePeriod period)
		{
			if (person.WorkflowControlSet == null)
				return null;

			var permissionInformation = person.PermissionInformation;
			var agentTimeZone = permissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = period.ToDateOnlyPeriod(agentTimeZone);
			var personPeriod = person.PersonPeriods(dateOnlyPeriod).ToArray();
			if (!personPeriod.Any())
				return null;

			var personSkillTypeDescriptions = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p)).Select(p => p.Skill.SkillType.Description).ToList();

			var margedPeriod = _overtimeRequestOpenPeriodMerger.GetMergedOvertimeRequestOpenPeriods(person.WorkflowControlSet.OvertimeRequestOpenPeriods, permissionInformation, dateOnlyPeriod);
			if (margedPeriod.Any())
			{
				return margedPeriod
					.Where(x => x.AutoGrantType != OvertimeRequestAutoGrantType.Deny &&
								personSkillTypeDescriptions.Contains(x.SkillType.Description)).ToList();
			}

			return null;
		}
	}
}