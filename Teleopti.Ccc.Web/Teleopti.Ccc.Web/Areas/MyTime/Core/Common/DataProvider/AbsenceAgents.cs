using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AbsenceAgents : IAbsenceAgents
	{
		public DateTime Date { get; set; }
		public double AbsenceTime { get; set; }
		public int HeadCounts { get; set; }
	}
}