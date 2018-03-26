using System.Collections.Generic;
using numl.Model;

namespace Teleopti.Ccc.Infrastructure.MachineLearning
{
	public class PredictCategory
	{
		public Model Train(IEnumerable<ShiftCategoryPredictorModel> data)
		{
			var d = Descriptor.Create<ShiftCategoryPredictorModel>();
			
			var generator = new numl.Supervised.DecisionTree.DecisionTreeGenerator();
			generator.Descriptor = d;
			var result = generator.Generate(data);

			return new Model(result);
		}
	}
}
