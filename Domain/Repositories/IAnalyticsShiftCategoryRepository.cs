using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsShiftCategoryRepository
	{
		IList<AnalyticsShiftCategory> ShiftCategories();
		void AddShiftCategory(AnalyticsShiftCategory analyticsShiftCategory);
		void UpdateShiftCategory(AnalyticsShiftCategory analyticsShiftCategory);
		AnalyticsShiftCategory ShiftCategory(Guid shiftCategoryId);
	}
}