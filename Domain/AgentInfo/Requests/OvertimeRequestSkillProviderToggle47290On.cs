using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeRequestSkillProviderToggle47290On : IOvertimeRequestSkillProvider
	{
		private readonly IPrimaryPersonSkillFilter _primaryPersonSkillFilter;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly INow _now;

		public OvertimeRequestSkillProviderToggle47290On(IPrimaryPersonSkillFilter primaryPersonSkillFilter, ISkillTypeRepository skillTypeRepository, INow now)
		{
			_primaryPersonSkillFilter = primaryPersonSkillFilter;
			_skillTypeRepository = skillTypeRepository;
			_now = now;
		}

		public IEnumerable<ISkill> GetAvailableSkills(IPerson person, DateTimePeriod requestPeriod)
		{
			var overtimeRequestOpenPeriod = person.WorkflowControlSet.GetMergedOvertimeRequestOpenPeriod(
				requestPeriod,
				new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), person.PermissionInformation.DefaultTimeZone())),
				person.PermissionInformation);
			var period = requestPeriod.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			return _primaryPersonSkillFilter.Filter(getSupportedPersonSkills(person, period, overtimeRequestOpenPeriod))
				.Select(s => s.Skill).ToList();
		}

		public IEnumerable<ISkill> GetAvailableSkillsBySkillType(IPerson person, DateTimePeriod requestPeriod)
		{
			var period = requestPeriod.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			return _primaryPersonSkillFilter.Filter(getSupportedPersonSkillsInOpenPeriods(person, period))
				.Select(s => s.Skill).ToList();
		}

		private IEnumerable<IPersonSkill> getSupportedPersonSkills(IPerson person, DateOnlyPeriod period,
			IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod)
		{
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (!personPeriod.Any())
				return new IPersonSkill[] { };

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Where(p => isSkillTypeMatchedInOpenPeriod(p, overtimeRequestOpenPeriod));

			return !personSkills.Any() ? new IPersonSkill[] { } : personSkills.Distinct();
		}

		private IEnumerable<IPersonSkill> getSupportedPersonSkillsInOpenPeriods(IPerson person, DateOnlyPeriod period)
		{
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (!personPeriod.Any())
				return new IPersonSkill[] { };

			var overtimeRequestOpenPeriods = person.WorkflowControlSet.OvertimeRequestOpenPeriods.Where(o =>
				o.AutoGrantType != OvertimeRequestAutoGrantType.Deny);

			var phoneSkillType = _skillTypeRepository.LoadAll()
				.FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Where(p => isSkillTypeMatchedInOpenPeriods(p, overtimeRequestOpenPeriods, phoneSkillType));

			return !personSkills.Any() ? new IPersonSkill[] { } : personSkills.Distinct();
		}

		private bool isSkillTypeMatchedInOpenPeriod(IPersonSkill personSkill, IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod)
		{
			var skillTypeInOvertimeRequestOpenPeriod = overtimeRequestOpenPeriod.SkillType ??
													   _skillTypeRepository.LoadAll().First(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			return personSkill.Skill.SkillType.Description.Name.Equals(skillTypeInOvertimeRequestOpenPeriod.Description.Name);
		}

		private bool isSkillTypeMatchedInOpenPeriods(IPersonSkill personSkill,
			IEnumerable<IOvertimeRequestOpenPeriod> overtimeRequestOpenPeriods, ISkillType defaultSkillType)
		{
			return overtimeRequestOpenPeriods.Any(o =>
				(o.SkillType ?? defaultSkillType).Description.Name.Equals(personSkill.Skill.SkillType.Description.Name));
		}
	}
}