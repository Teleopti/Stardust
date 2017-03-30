using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class BlockInfoFactory
	{
		public BlockInfo Execute(IEnumerable<IScheduleMatrixPro> allMatrixes, DateOnly startDate, Func<IScheduleMatrixPro, DateOnly, DateOnlyPeriod?> periodForOneMatrix)
		{
			var currentPeriod = periodForDate(allMatrixes, startDate, periodForOneMatrix);
			if (!currentPeriod.HasValue)
				return null;

			while (true)
			{
				var startPeriod = periodForDate(allMatrixes, currentPeriod.Value.StartDate, periodForOneMatrix);
				var endPeriod = periodForDate(allMatrixes, currentPeriod.Value.EndDate, periodForOneMatrix);
				var newPeriod = startPeriod.MaximumPeriod(endPeriod);
				if (!newPeriod.HasValue || newPeriod.Value == currentPeriod.Value)
				{
					return new BlockInfo(currentPeriod.Value);
				}
				currentPeriod = newPeriod;
			}
			throw new Exception("If you end up here, the world has gone crazy");
		}

		private static DateOnlyPeriod? periodForDate(IEnumerable<IScheduleMatrixPro> allMatrixes, DateOnly startDate, Func<IScheduleMatrixPro, DateOnly, DateOnlyPeriod?> periodForOneMatrix)
		{
			DateOnlyPeriod? totalPeriod = null;
			foreach (var scheduleMatrixPro in allMatrixes)
			{
				totalPeriod = totalPeriod.MaximumPeriod(periodForOneMatrix(scheduleMatrixPro, startDate));
			}
			return totalPeriod;
		}
	}
}