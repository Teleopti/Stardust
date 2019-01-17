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

		public IList<OvertimeRequestSkillTypeFlatOpenPeriod> GetOvertimeRequestOpenPeriods(IPerson person, DateOnly date)
		{
			if (person.WorkflowControlSet == null)
				return null;
			
			var personPeriod = person.Period(date);
			if (personPeriod == null)
				return null;

			var personSkillTypeDescriptions = _personalSkills.PersonSkills(personPeriod).Select(p => p.Skill.SkillType.Description).ToHashSet();

			var margedPeriod = _overtimeRequestOpenPeriodMerger.GetMergedOvertimeRequestOpenPeriods(person.WorkflowControlSet.OvertimeRequestOpenPeriods, person.PermissionInformation, date.ToDateOnlyPeriod());
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