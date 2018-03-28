using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeShiftCategoryUsageFinder : IShiftCategoryUsageFinder
	{
		private List<IShiftCategoryPredictorModel> storage = new List<IShiftCategoryPredictorModel>();

		public IEnumerable<IShiftCategoryPredictorModel> Find()
		{
			return storage;
		}

		public void Has(params IShiftCategoryPredictorModel[] shiftCategoryPredictorModels)
		{
			storage.AddRange(shiftCategoryPredictorModels);
		}
	}
}