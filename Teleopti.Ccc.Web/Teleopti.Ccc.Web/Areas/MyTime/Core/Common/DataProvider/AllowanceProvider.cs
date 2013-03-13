using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AllowanceProvider : IAllowanceProvider
	{

		IEnumerable<IAllowanceDay> IAllowanceProvider.GetAllowanceForPeriod(DateOnlyPeriod period)
		{
			//fulkod
			var random = new Random();
			return period.DayCollection().Select(day => new AllowanceDay() {Date = day, Allowance = random.Next(1, 10)}).Cast<IAllowanceDay>().ToList();
		}
	}

	public class AllowanceDay : IAllowanceDay
	{
		public DateTime Date { get; set; }
		public int Allowance { get; set; }
	}
}