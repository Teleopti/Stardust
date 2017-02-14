using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class CalculateOvertimeSuggestionProvider
	{
		private readonly IPersonForOvertimeProvider _personProviderForOvertime;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillStaffingIntervalProvider _skillStaffingIntervalProvider;
		private readonly IExtractSkillForecastIntervals _extractSkillForecastIntervals;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;

		public CalculateOvertimeSuggestionProvider(IPersonForOvertimeProvider personProviderForOvertime,
			IPersonRepository personRepository, ISkillStaffingIntervalProvider skillStaffingIntervalProvider, IExtractSkillForecastIntervals extractSkillForecastIntervals, ISkillCombinationResourceRepository skillCombinationResourceRepository)
		{
			_personProviderForOvertime = personProviderForOvertime;
			_personRepository = personRepository;
			_skillStaffingIntervalProvider = skillStaffingIntervalProvider;

			_extractSkillForecastIntervals = extractSkillForecastIntervals;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
		}

		public IList<SkillIntervalsForOvertime> GetOvertimeSuggestions(IList<Guid> skillIds, DateTime startDateTime, DateTime endDateTime )
		{
			// persons that we maybe can add overtime to
			// sorted by the biggest difference between weekly target and actually scheduled, maybe
			var pers = _personProviderForOvertime.Persons(skillIds, startDateTime, endDateTime);
			var thePerson = _personRepository.FindPeople((IEnumerable<Guid>) pers.Select(x => x.PersonId));
			//load the persons with the data needed, personskills for example

			var period = new DateTimePeriod(startDateTime.ToUniversalTime(), endDateTime.ToUniversalTime());

			// load skillcombinationdata
			var resources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			var intervals = _skillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(period, resources);

			// filter persons smart when demand is filled on A and there is still demand on B
			// so we just try on add overtime that has Skill B
			
			// loop in some way and add overtime, then check if the demand is fulfilled

			return new List<SkillIntervalsForOvertime>();
		}
	}
	public class SkillIntervalsForOvertime
	{
		public Guid SkillId { get; set; }
		public DateTime Time { get; set; }
		public double NewStaffing { get; set; }
	}
}