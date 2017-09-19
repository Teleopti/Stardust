using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	[Flags]
	public enum BusinessRuleFlags
	{
		None = 0,
		DataPartOfAgentDay = 1 << 0,
		MinWeeklyRestRule = 1 << 1,
		MinWeekWorkTimeRule = 1 << 2,
		NewDayOffRule = 1 << 3,
		NewMaxWeekWorkTimeRule = 1 << 4,
		NewNightlyRestRule = 1 << 5,
		NewPersonAccountRule = 1 << 6,
		NewShiftCategoryLimitationRule = 1 << 7,
		NonMainShiftActivityRule = 1 << 8,
		OpenHoursRule = 1 << 9,
		WeekShiftCategoryLimitationRule = 1 << 10,
		SiteOpenHoursRule = 1 << 11,
		NotOverwriteLayerRule = 1 << 12,
		ShiftTradeValidationFailedRule = 1 << 13
	}
}