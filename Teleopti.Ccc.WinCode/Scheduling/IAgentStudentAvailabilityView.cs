﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IAgentStudentAvailabilityView
	{
		void Update(TimeSpan? startTime, TimeSpan? endTime, bool endNextDay);
	}
}
