using System.IO;
using numl.Supervised;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Infrastructure.MachineLearning
{
	public class ShiftCategoryPredictionModel : IShiftCategoryPredictionModel
	{
		private readonly IModel _model;

#pragma warning disable CS3001 // Argument type is not CLS-compliant
		public ShiftCategoryPredictionModel(IModel model)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
		{
			_model = model;
		}

		public string Predict(ShiftCategoryExample model)
		{
			return _model.Predict(model).ShiftCategory;
		}

		public void Store(Stream file)
		{
			_model.Save(file);
		}
	}
}