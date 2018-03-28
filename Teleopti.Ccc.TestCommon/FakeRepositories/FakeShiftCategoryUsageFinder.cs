using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeShiftCategoryUsageFinder : IShiftCategoryUsageFinder
	{
		private List<ShiftCategoryExample> storage = new List<ShiftCategoryExample>();

		public IEnumerable<ShiftCategoryExample> Find()
		{
			return storage;
		}

		public void Has(params ShiftCategoryExample[] shiftCategoryExamples)
		{
			storage.AddRange(shiftCategoryExamples);
		}
	}
}