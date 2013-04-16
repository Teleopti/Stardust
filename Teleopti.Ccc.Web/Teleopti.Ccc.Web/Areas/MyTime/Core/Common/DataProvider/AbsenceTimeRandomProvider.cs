using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AbsenceTimeRandomProvider : IAbsenceTimeProvider
	{
		public IEnumerable<IAbsenceAgents> GetAbsenceTimeForPeriod(DateOnlyPeriod period)
		{
			var random = new Random();
			return period.DayCollection().Select(day => new AbsenceAgents() { Date = day, AbsenceTime = random.Next(1,23) }).Cast<IAbsenceAgents>().ToList();
		}
	}
}