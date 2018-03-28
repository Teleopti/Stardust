using System.Collections.Generic;
using System.Linq;
using numl.Model;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers;

namespace Teleopti.Ccc.Infrastructure.MachineLearning
{
	public class PredictShiftCategory : IPredictShiftCategory
	{
		public IShiftCategoryPredictionModel Train(IEnumerable<ShiftCategoryExample> data)
		{
			var descriptor = Descriptor.Create<ShiftCategoryExampleForTraining>();
			
			var generator = new numl.Supervised.DecisionTree.DecisionTreeGenerator();
			generator.Descriptor = descriptor;
			var result = generator.Generate(data.Select(d => new ShiftCategoryExampleForTraining
			{
				DayOfWeek = d.DayOfWeek,
				StartTime = d.StartTime,
				EndTime = d.EndTime,
				ShiftCategory = d.ShiftCategory
			}));

			return new ShiftCategoryPredictionModel(result);
		}
	}
}
