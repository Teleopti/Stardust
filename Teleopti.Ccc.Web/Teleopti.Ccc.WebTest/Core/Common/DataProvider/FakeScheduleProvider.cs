using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;

using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	public class FakeScheduleProvider : IScheduleProvider
	{
		private readonly List<IScheduleDay> _scheduleDays;
		public ScheduleDictionaryLoadOptions LatestScheduleLoadOptions;

		public FakeScheduleProvider(params IScheduleDay[] scheduleDays)
		{
			_scheduleDays = new List<IScheduleDay>();
			foreach (var scheduleDay in scheduleDays)
			{
				_scheduleDays.Add(scheduleDay);
			}			
		}

		public IEnumerable<IScheduleDay> GetScheduleForPeriod(DateOnlyPeriod period, ScheduleDictionaryLoadOptions options = null)
		{
			LatestScheduleLoadOptions = options;
			return _scheduleDays.Where(sd => period.Contains(sd.DateOnlyAsPeriod.DateOnly));
		}

		public IEnumerable<IScheduleDay> GetScheduleForStudentAvailability(DateOnlyPeriod period, ScheduleDictionaryLoadOptions options = null)
		{
			LatestScheduleLoadOptions = options;
			return _scheduleDays;
		}

	    public IEnumerable<IScheduleDay> GetScheduleForPersons(DateOnly date, IEnumerable<IPerson> persons, bool loadNotes = false)
		{
			LatestScheduleLoadOptions = new ScheduleDictionaryLoadOptions(false, loadNotes);
			return _scheduleDays.Where(sd =>
			{
				return persons.Any(p => p == sd.Person || p.Id == sd.Person.Id) && sd.DateOnlyAsPeriod.DateOnly == date;
			});
		}

		public IEnumerable<IScheduleDay> GetScheduleForPersonsInPeriod(DateOnlyPeriod period, IEnumerable<IPerson> persons,
			ScheduleDictionaryLoadOptions options)
		{
			LatestScheduleLoadOptions = options;
			return _scheduleDays.Where(sd =>
			{
				return persons.Any(p => p == sd.Person || p.Id == sd.Person.Id) && period.Contains(sd.DateOnlyAsPeriod.DateOnly);
			});
		}

		public void AddScheduleDay(IScheduleDay sd)
		{
			_scheduleDays.Add(sd);
		}
	}
}