

----------------  
--Name: Robin Karlsson
--Date: 2013-09-26
--Desc: Bug #24859 - Fix messed up values in shifts
---------------- 
update ActivityExtender
set earlystart = 0 where EarlyStart = 1

update ActivityExtender
set LateStart = 0 where LateStart = 1
GO

----------------  
--Name: David Jonsson
--Date: 2013-10-02
--Desc: Bug #24989 - Make database compatible with SQL 2005
---------------- 
ALTER TABLE [dbo].[OvertimeAvailability]
ALTER COLUMN [DateOfOvertime] [datetime] NOT NULL
