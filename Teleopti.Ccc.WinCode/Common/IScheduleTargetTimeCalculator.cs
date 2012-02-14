using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
