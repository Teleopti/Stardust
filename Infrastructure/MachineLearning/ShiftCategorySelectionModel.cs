using System.IO;
using numl.Supervised;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers;

namespace Teleopti.Ccc.Infrastructure.MachineLearning
{
	public class ShiftCategorySelectionModel : IShiftCategorySelectionModel
	{
		private readonly IModel _model;

#pragma warning disable CS3001 // Argument type is not CLS-compliant
		public ShiftCategorySelectionModel(IModel model)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
		{
			_model = model;
		}

		public string Predict(IShiftCategoryPredictorModel model)
		{
			return _model.Predict((ShiftCategoryPredictorModel)model).ShiftCategory;
		}

		public string Predict(double start, double end)
		{
			return Predict(new ShiftCategoryPredictorModel {StartTime = start, EndTime = end});
		}

		public void Store(Stream file)
		{
			_model.Save(file);
		}

		public static ShiftCategorySelectionModel Load(Stream file)
		{
			var model = new numl.Supervised.DecisionTree.DecisionTreeModel();
			return new ShiftCategorySelectionModel(model.Load(file));
		}
	}
}