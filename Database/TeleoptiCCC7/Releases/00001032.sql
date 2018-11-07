update [dbo].SchedulePeriod  set AverageWorkTimePerDay = NULL  where AverageWorkTimePerDay < 0
update [dbo].SchedulePeriod  set PeriodTime = NULL  where PeriodTime < 0
GO