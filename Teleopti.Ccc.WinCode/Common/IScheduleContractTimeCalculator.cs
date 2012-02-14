using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

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
