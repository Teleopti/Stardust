using numl.Supervised;

namespace Teleopti.Ccc.Infrastructure.MachineLearning
{
	public class Model
	{
		private readonly IModel _model;

#pragma warning disable CS3001 // Argument type is not CLS-compliant
		public Model(IModel model)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
		{
			_model = model;
		}

		public string Predict(ShiftCategoryPredictorModel model)
		{
			return _model.Predict(model).ShiftCategory;
		}

		public string Predict(double start, double end)
		{
			return Predict(new ShiftCategoryPredictorModel {StartTime = start, EndTime = end});
		}

		public void Store(string fileName)
		{
			_model.Save(fileName);
		}

		public static Model Load(string fileName)
		{
			var model = new numl.Supervised.DecisionTree.DecisionTreeModel();
			return new Model(model.Load(fileName));
		}
	}
}