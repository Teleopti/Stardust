using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IEmptyDaysInBlockOutsideSelectedHandler
	{
		IList<DateOnly> CheckDates(IList<DateOnly> blockDates, IScheduleMatrixPro matrixPro);
	}

	public class EmptyDaysInBlockOutsideSelectedHandler : IEmptyDaysInBlockOutsideSelectedHandler
	{
		public IList<DateOnly> CheckDates(IList<DateOnly> blockDates, IScheduleMatrixPro matrixPro)
		{
#pragma warning disable 612,618
			var insidePeriod = matrixPro.SelectedPeriod;
#pragma warning restore 612,618
			var datesOutside = new List<DateOnly>();
			foreach (var blockDate in blockDates)
			{
				var part = matrixPro.GetScheduleDayByKey(blockDate).DaySchedulePart();
				if (part.SignificantPart().Equals(SchedulePartView.MainShift))
					return blockDates;

				if(!insidePeriod.Contains(blockDate))
					datesOutside.Add(blockDate);
			}
			foreach (var dateOnly in datesOutside)
			{
				blockDates.Remove(dateOnly);
			}
			return blockDates;
		}
	}
}