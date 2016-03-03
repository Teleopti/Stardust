using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
    public class MaxSeatSkillCreator
    {
        private readonly IMaxSeatSitesExtractor _maxSeatSitesExtractor;
        private readonly ICreateSkillsFromMaxSeatSites _createSkillsFromMaxSeatSites;
        private readonly ICreatePersonalSkillsFromMaxSeatSites _createPersonalSkillsFromMaxSeatSites;
	    private readonly ISchedulerSkillDayHelper _schedulerSkillDayHelper;

	    public MaxSeatSkillCreator(IMaxSeatSitesExtractor maxSeatSitesExtractor, ICreateSkillsFromMaxSeatSites createSkillsFromMaxSeatSites,
			ICreatePersonalSkillsFromMaxSeatSites createPersonalSkillsFromMaxSeatSites, ISchedulerSkillDayHelper schedulerSkillDayHelper)
        {
            _maxSeatSitesExtractor = maxSeatSitesExtractor;
            _createSkillsFromMaxSeatSites = createSkillsFromMaxSeatSites;
            _createPersonalSkillsFromMaxSeatSites = createPersonalSkillsFromMaxSeatSites;
	        _schedulerSkillDayHelper = schedulerSkillDayHelper;
        }

		public MaxSeatCretorResult CreateMaxSeatSkills(DateOnlyPeriod requestedPeriod, IScenario scenario, IList<IPerson> personsInOrganization, IEnumerable<ISkill> stateHolderSkills)
		{
			var holderSkills = stateHolderSkills as ISkill[] ?? stateHolderSkills.ToArray();
			var minIntervalLength = holderSkills.Any() ? holderSkills.Min(s => s.DefaultResolution) : 15;
			var sitesWithMaxSeats = _maxSeatSitesExtractor.MaxSeatSites(requestedPeriod, personsInOrganization);
			var maxSeatSkills = _createSkillsFromMaxSeatSites.CreateSkillList(sitesWithMaxSeats, minIntervalLength).ToList();
            _createPersonalSkillsFromMaxSeatSites.Process(requestedPeriod, personsInOrganization);
			var extendedPeriod = new DateOnlyPeriod(requestedPeriod.StartDate.AddDays(-8), requestedPeriod.EndDate.AddDays(8));
			var maxSeatSkillDays = _schedulerSkillDayHelper.AddMaxSeatSkillDaysToStateHolder(extendedPeriod, maxSeatSkills, scenario);

			return new MaxSeatCretorResult { SkillDaysToAddToStateholder = maxSeatSkillDays, SkillsToAddToStateholder = maxSeatSkills };
        }

	    public class MaxSeatCretorResult
	    {
			public IEnumerable<ISkill> SkillsToAddToStateholder { get; set; }
			public IDictionary<ISkill, IList<ISkillDay>> SkillDaysToAddToStateholder { get; set; }

	    }
    }
}