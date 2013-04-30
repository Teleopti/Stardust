using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AllowanceRandomProvider : IAllowanceProvider
	{
		IEnumerable<Tuple<DateOnly, TimeSpan>> IAllowanceProvider.GetAllowanceForPeriod(DateOnlyPeriod period)
		{
			var random = new Random();
			return
				period.DayCollection().Select(day => new Tuple<DateOnly, TimeSpan>(day, TimeSpan.FromHours(random.Next(1, 100))));
		}

	}
}