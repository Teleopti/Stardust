﻿namespace Teleopti.Interfaces.Domain
{
	public interface IBusinessRulesForPersonalAccountUpdate
	{
		INewBusinessRuleCollection FromScheduleRange(IScheduleRange scheduleRange, bool disableValidation = false);
	}
}
