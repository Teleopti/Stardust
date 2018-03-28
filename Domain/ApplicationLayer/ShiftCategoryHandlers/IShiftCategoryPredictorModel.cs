using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers
{
	public interface IShiftCategoryPredictorModel
	{
		double StartTime { get; set; }
		double EndTime { get; set; }
		DayOfWeek DayOfWeek { get; set; }
		string ShiftCategory { get; set; }
	}
}