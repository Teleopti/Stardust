﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public interface IFutureData
	{
		IEnumerable<IWorkloadDayBase> Fetch(IWorkload workload, ICollection<ISkillDay> skillDays, DateOnlyPeriod futurePeriod);
	}
}