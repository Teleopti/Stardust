using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IScheduleProvider
	{
		IEnumerable<IScheduleDay> GetScheduleForPeriod(DateOnlyPeriod period, IScheduleDictionaryLoadOptions options = null);
		IEnumerable<IScheduleDay> GetScheduleForPersons(DateOnly date, IEnumerable<IPerson> persons);
	}
}