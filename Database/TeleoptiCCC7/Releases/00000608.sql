----------------  
--Name: Chundan Xu	
--Desc: Change the function description for Schedules in Anywhere. 
---------------- 
UPDATE [dbo].[ApplicationFunction]
Set [FunctionDescription] = 'xxMyTeamSchedules'
where [ForeignId]='0081'
GO