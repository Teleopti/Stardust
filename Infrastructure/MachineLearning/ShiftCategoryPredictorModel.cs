using System;
using numl.Model;

namespace Teleopti.Ccc.Infrastructure.MachineLearning
{
	public class ShiftCategoryPredictorModel
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