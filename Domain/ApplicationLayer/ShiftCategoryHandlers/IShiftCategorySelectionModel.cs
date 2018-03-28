using System.IO;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers
{
	public interface IShiftCategorySelectionModel
	{
		string Predict(IShiftCategoryPredictorModel model);
		string Predict(double start, double end);
		void Store(Stream file);
	}
}