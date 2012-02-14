using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ScheduleMatrixListPeriodFinder
	{
		private readonly IList<IScheduleMatrixPro> _matrixList;

		public ScheduleMatrixListPeriodFinder(IList<IScheduleMatrixPro> matrixList)
		{
			if (matrixList == null)
				throw new ArgumentNullException("matrixList");

			if(matrixList.Count == 0)
				throw new ArgumentException(UserTexts.Resources.Empty, "matrixList");

			_matrixList = matrixList;
		}

		public DateOnlyPeriod FindOuterWeekPeriod()
		{
			var max = DateOnly.MinValue;
			var min = DateOnly.MaxValue;

			foreach (var matrix in _matrixList)
			{
				var days = matrix.OuterWeeksPeriodDays;

				if (days.Count == 0) continue;
			
				var lastDay = days.Last().Day;
				var firstDay = days.First().Day;

				if (lastDay >= max) max = lastDay;
				if (firstDay <= min) min = firstDay;
			}

			return new DateOnlyPeriod(min, max);
		}
	}
}
