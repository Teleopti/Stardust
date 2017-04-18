using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
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
