using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Helper
{
	public static class DateExtensions
	{
		public static DateTime LimitMin(this DateTime dateTime)
		{
			if (DateHelper.MinSmallDateTime > dateTime)
				return DateHelper.MinSmallDateTime;

			return dateTime;
		}
	}
}