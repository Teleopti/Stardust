using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AllowanceDay : IAllowanceDay
	{
		public DateOnly Date { get; set; }
		public TimeSpan Time { get; set; }
		public TimeSpan Heads { get; set; }
		public double AllowanceHeads { get; set; }
		public bool Availability { get; set; }
		public bool UseHeadCount { get; set; }
	}
}