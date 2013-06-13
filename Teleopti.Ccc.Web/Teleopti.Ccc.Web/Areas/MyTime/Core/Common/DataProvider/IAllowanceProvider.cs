﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IAllowanceProvider
	{
		IEnumerable<Tuple<DateOnly, TimeSpan, TimeSpan, bool>> GetAllowanceForPeriod(DateOnlyPeriod period);
	}
}