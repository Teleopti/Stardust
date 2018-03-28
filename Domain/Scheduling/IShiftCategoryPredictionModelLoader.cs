namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IShiftCategoryPredictionModelLoader
	{
		IShiftCategoryPredictionModel Load(string model);
	}
}