﻿using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	internal enum ZoomLevel
	{
		DayView,
		WeekView,
		PeriodView,
		Overview,
		RequestView,
		RestrictionView
	}

	internal enum ControlType
	{
		ShiftEditor,
		SchedulerGridMain,
		SchedulerGridSkillData,
		Request
	}

	internal enum OptimizationMethod
	{
		Optimize,
		BackToLegalShift
	}
}
