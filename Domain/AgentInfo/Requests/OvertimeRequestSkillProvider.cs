using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeRequestSkillProvider : IOvertimeRequestSkillProvider
	{
		private readonly IPrimaryPersonSkillFilter _primaryPersonSkillFilter;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IOvertimeRequestOpenPeriodMerger _overtimeRequestOpenPeriodMerger;

		public OvertimeRequestSkillProvider(IPrimaryPersonSkillFilter primaryPersonSkillFilter, ISkillTypeRepository skillTypeRepository, IOvertimeRequestOpenPeriodMerger overtimeRequestOpenPeriodMerger)
		{
			_primaryPersonSkillFilter = primaryPersonSkillFilter;
			_skillTypeRepository = skillTypeRepository;
			_overtimeRequestOpenPeriodMerger = overtimeRequestOpenPeriodMerger;
		}

		public IEnumerable<ISkill> GetAvailableSkillsBySkillType(IPerson person, DateTimePeriod requestPeriod)
		{
			var period = requestPeriod.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			return _primaryPersonSkillFilter.Filter(getSupportedPersonSkillsInOpenPeriods(person, period), person)
				.Select(s => s.Skill).ToList();
		}

		private IEnumerable<IPersonSkill> getSupportedPersonSkillsInOpenPeriods(IPerson person, DateOnlyPeriod period)
		{
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (!personPeriod.Any())
				return new IPersonSkill[] { };

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p)).ToList();

			if (person.WorkflowControlSet.OvertimeRequestOpenPeriods.Any())
			{
				var mergedOvertimeRequestOpenPeriods = _overtimeRequestOpenPeriodMerger
					.GetMergedOvertimeRequestOpenPeriods(person.WorkflowControlSet.OvertimeRequestOpenPeriods,
						person.PermissionInformation, period).Where(p => p.AutoGrantType != OvertimeRequestAutoGrantType.Deny);

				var phoneSkillType = _skillTypeRepository.LoadAll()
					.FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
				personSkills = personSkills.Where(p =>
					isSkillTypeMatchedInOpenPeriods(p, mergedOvertimeRequestOpenPeriods, phoneSkillType)).ToList();
			}

			return !personSkills.Any() ? new IPersonSkill[] { } : personSkills.Distinct();
		}

		private bool isSkillTypeMatchedInOpenPeriods(IPersonSkill personSkill,
			IEnumerable<OvertimeRequestSkillTypeFlatOpenPeriod> overtimeRequestOpenPeriods, ISkillType defaultSkillType)
		{
			return overtimeRequestOpenPeriods.Any(o =>
				{
					if (o.SkillType != null)
					{
						return o.SkillType.Equals(personSkill.Skill.SkillType);
					}
					return defaultSkillType.Equals(personSkill.Skill.SkillType);
				}
			);
		}
	}
}