using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Tracking
{
    public class PersonAccountProjectionService : IPersonAccountProjectionService
    {
        private readonly IAccount _account;
        private readonly IPerson _person;
        
        public PersonAccountProjectionService(IAccount account)
        {
            _account = account;
            _person = account.Owner.Person;
        }
		
        public IList<IScheduleDay> CreateProjection(IScheduleStorage storage, IScenario scenario)
        {
	        var timeZone = _person.PermissionInformation.DefaultTimeZone();

	        var range = storage.ScheduleRangeBasedOnAbsence(_account.Period().ToDateTimePeriod(timeZone), scenario,
		        _person, _account.Owner.Absence);
	        return range.ScheduledDayCollection(_account.Period()).ToList();
        }
    }
}
