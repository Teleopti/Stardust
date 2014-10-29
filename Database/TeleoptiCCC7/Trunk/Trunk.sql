-----------------
--Name: Micke
--Date: 2014-10-29
--Desc: removes seconds from values
----------------  
update [dbo].[SchedulePeriod]
set [BalanceIn] = ROUND([BalanceIn]/10000000/60, 0)*60*10000000
where [BalanceIn] > 0
GO

update [dbo].[SchedulePeriod]
set [BalanceOut] = ROUND([BalanceOut]/10000000/60, 0)*60*10000000
where [BalanceOut] > 0
GO

update [dbo].[SchedulePeriod]
set [Extra] = ROUND([Extra]/10000000/60, 0)*60*10000000
where [Extra] > 0
GO