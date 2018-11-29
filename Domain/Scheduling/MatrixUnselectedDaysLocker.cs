using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class MatrixUnselectedDaysLocker
	{
		private readonly IEnumerable<IScheduleMatrixPro> _matrixList;
		private readonly DateOnlyPeriod _selectedPeriod;

		public MatrixUnselectedDaysLocker(IEnumerable<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod)
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
						matrix.LockDay(dateOnly);
				}
			}

		}
	}
}