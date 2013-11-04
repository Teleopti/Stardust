using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IAbsenceAgents
	{
		DateTime Date { get; set; }
		double AbsenceTime { get; set; }
		int HeadCounts { get; set; }
	}
}