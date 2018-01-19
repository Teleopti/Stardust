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
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();
		private readonly ISkillTypeRepository _skillTypeRepository;

		public OvertimeRequestSkillProviderToggle47290On(IPrimaryPersonSkillFilter primaryPersonSkillFilter, ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider
			, ISkillTypeRepository skillTypeRepository)
		{
			_primaryPersonSkillFilter = primaryPersonSkillFilter;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_skillTypeRepository = skillTypeRepository;
		}

		public IEnumerable<ISkill> GetAvailableSkills(IPerson person, DateTimePeriod requestPeriod, IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod)
		{
			var period = requestPeriod.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			return _primaryPersonSkillFilter.Filter(getSupportedPersonSkills(person, period,overtimeRequestOpenPeriod)).Select(s => s.Skill).ToList();
		}

		private IEnumerable<IPersonSkill> getSupportedPersonSkills(IPerson person, DateOnlyPeriod period, IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod)
		{
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (!personPeriod.Any())
				return new IPersonSkill[] { };

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Where(p => _supportedSkillsInIntradayProvider.CheckSupportedSkill(p.Skill));

			personSkills = filterBySkillTypeInOpenPeriod(personSkills, overtimeRequestOpenPeriod);

			return !personSkills.Any() ? new IPersonSkill[] { } : personSkills.Distinct();
		}

		private IEnumerable<IPersonSkill> filterBySkillTypeInOpenPeriod(IEnumerable<IPersonSkill> personSkills, IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod)
		{
			var skillTypeInOvertimeRequestOpenPeriod = overtimeRequestOpenPeriod.SkillType ??
													   _skillTypeRepository.LoadAll().First(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			personSkills = personSkills.Where(a =>
				a.Skill.SkillType.Description.Name.Equals(skillTypeInOvertimeRequestOpenPeriod.Description.Name)).ToArray();
			return personSkills;
		}
	}
}