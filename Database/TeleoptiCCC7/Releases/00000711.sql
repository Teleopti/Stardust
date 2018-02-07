----------------  
--Name: ChundanX
--Date: 2016-07-18
--Desc: Feature #39695 - Purge read model data
---------------- 
if not exists (select 1 from PurgeSetting where [Key] = 'DaysToKeepReadmodels')
	insert into PurgeSetting ([Key], [Value]) values('DaysToKeepReadmodels', 30)
GO
