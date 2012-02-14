/* 
Trunk initiated: 
2011-04-27 
16:54
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Ola Håkansson 
--Date: 2010-04-29
--Desc: Because new field in WorkShiftRuleSet  
----------------  

ALTER TABLE dbo.WorkShiftRuleSet ADD
	OnlyForRestrictions bit NOT NULL CONSTRAINT DF_WorkShiftRuleSet_OnlyForRestrictions DEFAULT 0
GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (326,'7.1.326') 
