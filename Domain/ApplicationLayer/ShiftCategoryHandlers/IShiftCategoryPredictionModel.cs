using System.IO;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers
{
	public interface IShiftCategoryPredictionModel
	{
		string Predict(ShiftCategoryExample model);
		void Store(Stream file);
	}
}