/* 
Trunk initiated: 
2010-09-07 
09:18
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Robin Karlsson
--Date: 2010-09-07
--Desc: Avoiding duplicate rows in the table 
----------------  
ALTER TABLE [dbo].[UserDetail] ADD UNIQUE NONCLUSTERED 
(
	[Person] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
 
----------------  
--Name: Anders Forsberg
--Date: 2010-09-20
--Desc: Fix Shifts - Auto pos activities - default value must be 15 min instead of 0
-- Added after the actual release script so some internal  test env may need manual updates.
----------------  
update activityextender
set AutoPosIntervalSegment = 9000000000
where extendertype = 'AutoPos'

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (294,'7.1.294') 
