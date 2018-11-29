using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityDayFormMapper
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public StudentAvailabilityDayFormMapper(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public IStudentAvailabilityDay Map(StudentAvailabilityDayInput source, IStudentAvailabilityDay destination = null)
		{
			if (destination == null)
			{
				var person = _loggedOnUser.CurrentUser();
				var restriction = new StudentAvailabilityRestriction();
				map(source, restriction);
				destination = new StudentAvailabilityDay(person, source.Date,
					new List<IStudentAvailabilityRestriction> {restriction});
			}
			else
			{
				map(source, destination.RestrictionCollection.Single());
			}
			return destination;
		}

		private void map(StudentAvailabilityDayInput s, IStudentAvailabilityRestriction r)
		{
			r.StartTimeLimitation = new StartTimeLimitation(s.StartTime.Time, null);
			r.EndTimeLimitation = new EndTimeLimitation(null, TimeHelper.ParseTimeSpanFromTimeOfDay(s.EndTime.Time, s.NextDay));
		}
	}
}