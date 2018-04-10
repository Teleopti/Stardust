using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestOpenPeriodProvider : IOvertimeRequestOpenPeriodProvider
	{
		private readonly INow _now;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();

		public OvertimeRequestOpenPeriodProvider(INow now, ISkillTypeRepository skillTypeRepository)
		{
			_now = now;
			_skillTypeRepository = skillTypeRepository;
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

			var defaultSkillType = getDefaultSkillType();

			var overtimeRequestOpenPeriodSkillTypeGroups =
				new SkillTypeFlatOvertimeOpenPeriodMapper().Map(person.WorkflowControlSet.OvertimeRequestOpenPeriods, defaultSkillType)
					.Where(x => personSkillTypeDescriptions.Contains((x.SkillType).Description) &&
								isPeriodMatched(x, person, dateOnlyPeriod))
					.GroupBy(o => o.SkillType ?? defaultSkillType);

			var skillTypeMergedOvertimeRequestOpenPeriods = new List<OvertimeRequestSkillTypeFlatOpenPeriod>();
			var overtimeRequestOpenPeriodMerger = new OvertimeRequestOpenPeriodMerger();
			foreach (var overtimeRequestOpenPeriodSkillTypeGroup in overtimeRequestOpenPeriodSkillTypeGroups)
			{
				if (overtimeRequestOpenPeriodSkillTypeGroup.Count() > 1)
				{
					skillTypeMergedOvertimeRequestOpenPeriods.Add(
						overtimeRequestOpenPeriodMerger.Merge(overtimeRequestOpenPeriodSkillTypeGroup));
				}
				else
				{
					skillTypeMergedOvertimeRequestOpenPeriods.AddRange(overtimeRequestOpenPeriodSkillTypeGroup);
				}
			}

			return skillTypeMergedOvertimeRequestOpenPeriods;
		}

		private bool isPeriodMatched(OvertimeRequestSkillTypeFlatOpenPeriod overtimeRequestOpenPeriod, IPerson person,
			DateOnlyPeriod requestPeriod)
		{
			return overtimeRequestOpenPeriod.OriginPeriod.GetPeriod(new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(),
				person.PermissionInformation.DefaultTimeZone()))).Contains(requestPeriod);
		}

		private ISkillType getDefaultSkillType()
		{
			var phoneSkillType = _skillTypeRepository.LoadAll()
				.FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			return phoneSkillType;
		}
	}
}