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

        public void CreateMaxSeatSkills(DateTimePeriod requestedPeriod, ICccTimeZoneInfo timeZoneInfo)
        {
            var dateOnlyPeriod = requestedPeriod.ToDateOnlyPeriod(timeZoneInfo);
            var sitesWithMaxSeats = _maxSeatSitesExtractor.MaxSeatSites(dateOnlyPeriod);

            _createSkillsFromMaxSeatSites.CreateSkillList(sitesWithMaxSeats);
            _createPersonalSkillsFromMaxSeatSites.Process(dateOnlyPeriod, _personsInOrganization);
            _schedulerSkillDayHelper.AddSkillDaysToStateHolder(ForecastSource.MaxSeatSkill, 0);
        }
    }
}