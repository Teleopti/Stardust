using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
    public class MaxSeatSkillCreator
    {
        private readonly IMaxSeatSitesExtractor _maxSeatSitesExtractor;
        private readonly ICreateSkillsFromMaxSeatSites _createSkillsFromMaxSeatSites;
        private readonly ICreatePersonalSkillsFromMaxSeatSites _createPersonalSkillsFromMaxSeatSites;
        private readonly ISchedulerSkillDayHelper _schedulerSkillDayHelper;
        private readonly IEnumerable<IPerson> _personsInOrganization;


        public MaxSeatSkillCreator(IMaxSeatSitesExtractor maxSeatSitesExtractor, ICreateSkillsFromMaxSeatSites createSkillsFromMaxSeatSites,
            ICreatePersonalSkillsFromMaxSeatSites createPersonalSkillsFromMaxSeatSites, ISchedulerSkillDayHelper schedulerSkillDayHelper, IEnumerable<IPerson> personsInOrganization)
        {
            _maxSeatSitesExtractor = maxSeatSitesExtractor;
            _createSkillsFromMaxSeatSites = createSkillsFromMaxSeatSites;
            _createPersonalSkillsFromMaxSeatSites = createPersonalSkillsFromMaxSeatSites;
            _schedulerSkillDayHelper = schedulerSkillDayHelper;
            _personsInOrganization = personsInOrganization;
        }

        public void CreateMaxSeatSkills(DateOnlyPeriod requestedPeriod)
        {
            var sitesWithMaxSeats = _maxSeatSitesExtractor.MaxSeatSites(requestedPeriod);

            _createSkillsFromMaxSeatSites.CreateSkillList(sitesWithMaxSeats);
            _createPersonalSkillsFromMaxSeatSites.Process(requestedPeriod, _personsInOrganization);

			var extendedPeriod = new DateOnlyPeriod(requestedPeriod.StartDate.AddDays(-8), requestedPeriod.EndDate.AddDays(8));
			_schedulerSkillDayHelper.AddSkillDaysToStateHolder(extendedPeriod, ForecastSource.MaxSeatSkill, 0);
        }
    }
}