using System.Collections.Generic;
using System.Linq;
using numl.Model;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers;

namespace Teleopti.Ccc.Infrastructure.MachineLearning
{
	public class PredictCategory : IPredictCategory
	{
		public IShiftCategorySelectionModel Train(IEnumerable<IShiftCategoryPredictorModel> data)
		{
			var descriptor = Descriptor.Create<ShiftCategoryPredictorModel>();
			
			var generator = new numl.Supervised.DecisionTree.DecisionTreeGenerator();
			generator.Descriptor = descriptor;
			var result = generator.Generate(data.Select(d => new ShiftCategoryPredictorModel
			{
				DayOfWeek = d.DayOfWeek,
				StartTime = d.StartTime,
				EndTime = d.EndTime,
				ShiftCategory = d.ShiftCategory
			}));

			return new ShiftCategorySelectionModel(result);
		}
	}
}
