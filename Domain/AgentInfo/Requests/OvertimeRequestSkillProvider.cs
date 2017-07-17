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
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPrimaryPersonSkillFilter _primaryPersonSkillFilter;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();

		public OvertimeRequestSkillProvider(ILoggedOnUser loggedOnUser, IPrimaryPersonSkillFilter primaryPersonSkillFilter, ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider)
		{
			_loggedOnUser = loggedOnUser;
			_primaryPersonSkillFilter = primaryPersonSkillFilter;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
		}

		public IEnumerable<ISkill> GetAvailableSkills(DateTimePeriod requestPeriod)
		{
			var period = requestPeriod.ToDateOnlyPeriod(_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());
			return _primaryPersonSkillFilter.Filter(getSupportedPersonSkills(period)).Select(s => s.Skill).ToList();
		}

		private IEnumerable<IPersonSkill> getSupportedPersonSkills(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (!personPeriod.Any())
				return new IPersonSkill[] { };

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Where(p => _supportedSkillsInIntradayProvider.CheckSupportedSkill(p.Skill)).ToArray();

			return !personSkills.Any() ? new IPersonSkill[] { } : personSkills.Distinct();
		}
	}
}