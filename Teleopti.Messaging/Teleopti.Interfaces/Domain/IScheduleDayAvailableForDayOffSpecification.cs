using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
	public interface IScheduleDayAvailableForDayOffSpecification
	{
		bool IsSatisfiedBy(IScheduleDay part);
	}
}
