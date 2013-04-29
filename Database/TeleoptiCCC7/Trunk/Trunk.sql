----------------  
--Name: Robin Karlsson
--Date: 2013-04-24
--Desc: Bug #23047. Empty read models to consider deleted schedule when done.
----------------  
--TRUNCATE TABLE [ReadModel].[ScheduleDay] nope! dont => #23288 //DavidJ
TRUNCATE TABLE [ReadModel].[ScheduleProjectionReadOnly]
GO