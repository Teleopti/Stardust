using System;

namespace Teleopti.Ccc.WinCode.Common
{
	public interface IScheduleTargetTimeCalculator
	{
		/// <summary>
		/// Calculate target time
		/// </summary>
		/// <returns></returns>
		TimeSpan CalculateTargetTime();
	}
}
