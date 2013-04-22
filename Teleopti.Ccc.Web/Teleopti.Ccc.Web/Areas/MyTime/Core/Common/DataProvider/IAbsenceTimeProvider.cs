using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{	
	/// <summary>
	/// Gets absence time in minutes per day
	/// </summary>
	/// <remarks>
	/// Created by: marias	
	/// Created date: 2013-04-22
	/// </remarks>
	public interface IAbsenceTimeProvider
	{
		IEnumerable<IAbsenceAgents> GetAbsenceTimeForPeriod(DateOnlyPeriod period);
	}

	public interface IAbsenceTimeDay
	{
		DateTime Date { get; set; }
		double AbsenceTime { get; set; }
	}
}