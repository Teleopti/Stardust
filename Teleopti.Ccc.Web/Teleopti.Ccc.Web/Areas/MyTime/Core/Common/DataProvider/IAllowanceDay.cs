using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IAllowanceDay
	{
		DateOnly Date { get; set; }
		TimeSpan Time { get; set; }
		TimeSpan Heads { get; set; }
		double AllowanceHeads { get; set; }
		bool Availability { get; set; }
		bool UseHeadCount { get; set; }
	}
}