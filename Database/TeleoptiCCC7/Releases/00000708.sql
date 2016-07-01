----------------  
--Name: Chundan
--Date: 2016-07-01
--Desc: delete permission AngelMyTeamSchedules
----------------
UPDATE [dbo].[ApplicationFunction]
SET IsDeleted = 1
WHERE FunctionCode = 'AngelMyTeamSchedules'
