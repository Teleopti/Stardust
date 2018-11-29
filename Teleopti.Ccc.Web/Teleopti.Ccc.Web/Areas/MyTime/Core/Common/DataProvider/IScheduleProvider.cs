using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IScheduleProvider
	{
		IEnumerable<IScheduleDay> GetScheduleForPeriod(DateOnlyPeriod period, ScheduleDictionaryLoadOptions options = null);

		/// <summary>
		/// This method will ignore published date and view unpublished schdule permission setting to get 
		/// student availability after published date even if current user has no permission to view unpublished schedule.
		/// It should only be used to get schedule to retrieve student availability.
		/// Refer to bug #33327: Agents can no longer see Availability they entered for dates that have not been published.
		/// </summary>
		IEnumerable<IScheduleDay> GetScheduleForStudentAvailability(DateOnlyPeriod period, ScheduleDictionaryLoadOptions options = null);

		IEnumerable<IScheduleDay> GetScheduleForPersons(DateOnly date, IEnumerable<IPerson> persons, bool loadNotes = false);
		IEnumerable<IScheduleDay> GetScheduleForPersonsInPeriod(DateOnlyPeriod period, IEnumerable<IPerson> persons, ScheduleDictionaryLoadOptions options = null);
	}
}