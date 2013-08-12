﻿using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IMatrixUnselectedDaysLocker
	{
		void Execute();
	}

	public class MatrixUnselectedDaysLocker : IMatrixUnselectedDaysLocker
	{
		private readonly IList<IScheduleMatrixPro> _matrixList;
		private readonly DateOnlyPeriod _selectedPeriod;

		public MatrixUnselectedDaysLocker(IList<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod)
		{
			_matrixList = matrixList;
			_selectedPeriod = selectedPeriod;
		}

		public void Execute()
		{
			foreach (var matrix in _matrixList)
			{
				foreach (var effectivePeriodDay in matrix.EffectivePeriodDays)
				{
					var dateOnly = effectivePeriodDay.Day;
					if (!_selectedPeriod.Contains(dateOnly))
						matrix.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
				}
			}
			
		}
	}
}