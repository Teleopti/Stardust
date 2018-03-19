using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
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

		public IEnumerable<ISkill> GetAvailableSkillsBySkillType(IPerson person, DateTimePeriod requestPeriod)
		{
			var period = requestPeriod.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			return _primaryPersonSkillFilter.Filter(getSupportedPersonSkillsInOpenPeriods(person, period))
				.Select(s => s.Skill).ToList();
		}

		private IEnumerable<IPersonSkill> getSupportedPersonSkillsInOpenPeriods(IPerson person, DateOnlyPeriod period)
		{
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (!personPeriod.Any())
				return new IPersonSkill[] { };

			var overtimeRequestOpenPeriods = person.WorkflowControlSet.OvertimeRequestOpenPeriods.Where(o => isPeriodMatched(o, person, period));

			var phoneSkillType = _skillTypeRepository.LoadAll()
				.FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p)).Where(p =>
				isSkillTypeMatchedInOpenPeriods(p, overtimeRequestOpenPeriods, phoneSkillType)).ToList();

			return !personSkills.Any() ? new IPersonSkill[] { } : personSkills.Distinct();
		}

		private bool isSkillTypeMatchedInOpenPeriods(IPersonSkill personSkill,
			IEnumerable<IOvertimeRequestOpenPeriod> overtimeRequestOpenPeriods, ISkillType defaultSkillType)
		{
			return overtimeRequestOpenPeriods.Any(o =>
				(o.SkillType ?? defaultSkillType).Description.Name.Equals(personSkill.Skill.SkillType.Description.Name));
		}

		private bool isPeriodMatched(IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod, IPerson person,
			DateOnlyPeriod requestPeriod)
		{
			var openPeriod = overtimeRequestOpenPeriod.GetPeriod(new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(),
				person.PermissionInformation.DefaultTimeZone())));
			return requestPeriod.Intersection(openPeriod).HasValue;
		}
	}
}