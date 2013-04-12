using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IAbsenceTimeProvider
	{
		IEnumerable<IAbsenceTimeDay> GetAbsenceTimeForPeriod(DateOnlyPeriod period);
	}

	public interface IAbsenceTimeDay
	{
		DateTime Date { get; set; }
		double AbsenceTime { get; set; }
	}
}