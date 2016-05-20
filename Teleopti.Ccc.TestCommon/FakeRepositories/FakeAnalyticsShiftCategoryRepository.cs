using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsShiftCategoryRepository : IAnalyticsShiftCategoryRepository
	{
		private readonly List<AnalyticsShiftCategory> fakeShiftCategories;

		public FakeAnalyticsShiftCategoryRepository()
		{
			fakeShiftCategories = new List<AnalyticsShiftCategory>();
		}

		public FakeAnalyticsShiftCategoryRepository(List<AnalyticsShiftCategory> analyticsShiftCategories)
		{
			fakeShiftCategories = analyticsShiftCategories;
		}

		public IList<AnalyticsShiftCategory> ShiftCategories()
		{
			return fakeShiftCategories;
		}

		public void AddShiftCategory(AnalyticsShiftCategory analyticsShiftCategory)
		{
			analyticsShiftCategory.ShiftCategoryId = fakeShiftCategories.Any() ? fakeShiftCategories.Max(a => a.ShiftCategoryId) + 1 : 1;
			fakeShiftCategories.Add(analyticsShiftCategory);
		}

		public void UpdateShiftCategory(AnalyticsShiftCategory analyticsShiftCategory)
		{
			analyticsShiftCategory.ShiftCategoryId = fakeShiftCategories.First(a => a.ShiftCategoryCode == analyticsShiftCategory.ShiftCategoryCode).ShiftCategoryId;
			fakeShiftCategories.RemoveAll(a => a.ShiftCategoryCode == analyticsShiftCategory.ShiftCategoryCode);
			fakeShiftCategories.Add(analyticsShiftCategory);
		}
	}
}