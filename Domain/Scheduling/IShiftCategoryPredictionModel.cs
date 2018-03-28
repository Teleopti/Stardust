using System.IO;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IShiftCategoryPredictionModel
	{
		string Predict(ShiftCategoryExample model);
		void Store(Stream file);
	}
}