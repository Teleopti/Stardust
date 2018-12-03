using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IStudentAvailabilityProvider
	{
		IStudentAvailabilityRestriction GetStudentAvailabilityForDate(IEnumerable<IScheduleDay> scheduleDays, DateOnly date);
		IStudentAvailabilityRestriction GetStudentAvailabilityForDay(IStudentAvailabilityDay studentAvailabilityDay);
		IStudentAvailabilityDay GetStudentAvailabilityDayForDate(DateOnly date);
		IEnumerable<IStudentAvailabilityDay> GetStudentAvailabilityDayForPeriod(DateOnlyPeriod period);
	}
}