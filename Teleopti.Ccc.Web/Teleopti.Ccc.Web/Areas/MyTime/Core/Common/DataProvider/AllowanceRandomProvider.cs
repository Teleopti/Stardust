using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AllowanceRandomProvider : IAllowanceProvider
	{

		IEnumerable<IAllowanceDay> IAllowanceProvider.GetAllowanceForPeriod(DateOnlyPeriod period)
		{
			var random = new Random();
			return period.DayCollection().Select(day => new AllowanceDay() { Date = day, Allowance = random.Next(1, 100) }).Cast<IAllowanceDay>().ToList();
		}
	}
}