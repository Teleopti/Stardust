using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

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

		public IEnumerable<IScheduleDay> GetScheduleForPersons(DateOnly date, IEnumerable<IPerson> persons)
		{
			return _scheduleDays;
		}
	}
}