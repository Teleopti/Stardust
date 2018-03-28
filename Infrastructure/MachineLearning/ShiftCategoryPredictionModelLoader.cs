using System.IO;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Infrastructure.MachineLearning
{
	public class ShiftCategoryPredictionModelLoader : IShiftCategoryPredictionModelLoader
	{
		public IShiftCategoryPredictionModel Load(string model)
		{
			var m = new numl.Supervised.DecisionTree.DecisionTreeModel();

			using (var ms = new MemoryStream())
			{
				using (var writer = new StreamWriter(ms))
				{
					writer.Write(model);
					writer.Flush();

					ms.Position = 0;
					return new ShiftCategoryPredictionModel(m.Load(ms));
				}
			}
		}
	}
}