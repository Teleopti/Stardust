using System;
using numl.Model;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers;

namespace Teleopti.Ccc.Infrastructure.MachineLearning
{
	public class ShiftCategoryPredictorModel : IShiftCategoryPredictorModel
	{
		[Feature]
		public double StartTime { get; set; }

		[Feature]
		public double EndTime { get; set; }

		[Feature]
		public DayOfWeek DayOfWeek { get; set; }

		[StringLabel]
		public string ShiftCategory { get; set; }
	}
}