﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Backlog
{
	public class BacklogTaskForecastedTimePerDateCalculator
	{
		public TimeSpan CalculateForDate(DateOnly date, IDictionary<DateOnly, BacklogTask> taskDic)
		{
			var time = TimeSpan.Zero;
			foreach (var task in taskDic.Values)
			{
				time = time.Add(task.BacklogProductPlanTask.ForecastedTimeOnDate(date));
			}

			return time;
		}

		public double CalculateWorkForDate(DateOnly date, IDictionary<DateOnly, BacklogTask> taskDic)
		{
			var work = 0d;
			foreach (var task in taskDic.Values)
			{
				work += task.BacklogProductPlanTask.ForecastedWorkOnDate(date);
			}

			return work;
		}
	}
}