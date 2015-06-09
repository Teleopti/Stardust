using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	public class FakeScheduleProvider : IScheduleProvider
	{
		private readonly IScheduleDay[] _scheduleDays;

		public FakeScheduleProvider(params IScheduleDay[] scheduleDays)
		{
			_scheduleDays = scheduleDays;
		}

		public IEnumerable<IScheduleDay> GetScheduleForPeriod(DateOnlyPeriod period, IScheduleDictionaryLoadOptions options = null)
		{
			return _scheduleDays;
		}

		public IEnumerable<IScheduleDay> GetScheduleForStudentAvailability(DateOnlyPeriod period, IScheduleDictionaryLoadOptions options = null)
		{
			return _scheduleDays;
		}

		public IEnumerable<IScheduleDay> GetScheduleForPersons(DateOnly date, IEnumerable<IPerson> persons)
		{
			
			return _scheduleDays.Where(sd =>
			{
				return persons.Any(p => p == sd.Person || p.Id == sd.Person.Id) && sd.DateOnlyAsPeriod.DateOnly == date;
			});
			
		}
	}
}