using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IAbsenceTimeRandomProvider
	{
		IEnumerable<IAbsenceAgents> GetAbsenceTimeForPeriod(DateOnlyPeriod period);
	}

	public interface IAbsenceAgents
	{
		DateTime Date { get; set; }
		double AbsenceTime { get; set; }
	}
}