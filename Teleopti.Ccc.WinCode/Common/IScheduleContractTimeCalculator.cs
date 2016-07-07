using System;

namespace Teleopti.Ccc.WinCode.Common
{
	public interface IScheduleContractTimeCalculator
	{
		/// <summary>
		/// Calculate contract time
		/// </summary>
		/// <returns></returns>
		TimeSpan CalculateContractTime();
	}
}
