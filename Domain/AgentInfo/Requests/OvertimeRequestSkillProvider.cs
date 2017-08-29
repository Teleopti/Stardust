using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeRequestSkillProvider : IOvertimeRequestSkillProvider
	{
		private readonly IPrimaryPersonSkillFilter _primaryPersonSkillFilter;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();

		public OvertimeRequestSkillProvider(IPrimaryPersonSkillFilter primaryPersonSkillFilter, ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider)
		{
			_primaryPersonSkillFilter = primaryPersonSkillFilter;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
		}

		public IEnumerable<ISkill> GetAvailableSkills(IPerson person, DateTimePeriod requestPeriod)
		{
			var period = requestPeriod.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			return _primaryPersonSkillFilter.Filter(getSupportedPersonSkills(person, period)).Select(s => s.Skill).ToList();
		}

		private IEnumerable<IPersonSkill> getSupportedPersonSkills(IPerson person, DateOnlyPeriod period)
		{
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (!personPeriod.Any())
				return new IPersonSkill[] { };

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Where(p => _supportedSkillsInIntradayProvider.CheckSupportedSkill(p.Skill)).ToArray();

			return !personSkills.Any() ? new IPersonSkill[] { } : personSkills.Distinct();
		}
	}
}