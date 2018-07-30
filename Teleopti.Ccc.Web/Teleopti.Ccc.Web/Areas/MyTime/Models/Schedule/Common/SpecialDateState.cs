using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common
{
	[Flags]
	public enum SpecialDateState
	{
		None = 0,
		Selected = 1,
		Today = 2
	}
}