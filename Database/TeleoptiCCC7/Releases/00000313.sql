/* 
Trunk initiated: 
2011-01-04 
14:04
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2010-12-17
--Desc: Preparing for Speed up of RotationGet 
----------------  
CREATE NONCLUSTERED INDEX [IX_RotationDay_Parent] ON [dbo].[RotationDay] ([Parent]) INCLUDE ([Id],[Day])
CREATE NONCLUSTERED INDEX [IX_AvailabilityDay_Parent] ON [dbo].[AvailabilityDay] ([Parent]) INCLUDE ([Id],[Day])
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (313,'7.1.313') 
