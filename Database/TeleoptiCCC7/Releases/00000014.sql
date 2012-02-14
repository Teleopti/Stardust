/* 
BuildTime is: 
2008-12-03 
12:06
*/ 
/* 
Trunk initiated: 
2008-11-26 
12:27
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Madhuranga Pinnagoda 
--Date: 2008-12-02  
--Desc: Removing of [RotationRestriction] DayOff column type to IDayOff([uniqueidentifier]) & its reference key relationship.
----------------  
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_RotationRestriction_DayOff]') AND parent_object_id = OBJECT_ID(N'[RotationRestriction]'))
ALTER TABLE [RotationRestriction] DROP CONSTRAINT [FK_RotationRestriction_DayOff]
GO
ALTER TABLE RotationRestriction
DROP COLUMN DayOff
GO
ALTER TABLE RotationRestriction
ADD DayOff [uniqueidentifier] NULL
GO
ALTER TABLE [dbo].[RotationRestriction]  WITH CHECK ADD  CONSTRAINT [FK_RotationRestriction_DayOff] FOREIGN KEY([DayOff])
REFERENCES [dbo].[DayOff] ([Id])
GO
ALTER TABLE [dbo].[RotationRestriction] CHECK CONSTRAINT [FK_RotationRestriction_DayOff]
GO

----------------  
--Name: Madhuranga Pinnagoda 
--Date: 2008-12-02  
--Desc: Removing of [RotationDayRestriction] DayOff column type to IDayOff([uniqueidentifier]) & its reference key relationship.
----------------  

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_RotationDayRestriction_DayOff]') AND parent_object_id = OBJECT_ID(N'[RotationDayRestriction]'))
ALTER TABLE [RotationDayRestriction] DROP CONSTRAINT [FK_RotationDayRestriction_DayOff]
GO
ALTER TABLE RotationDayRestriction
DROP COLUMN DayOff
GO
ALTER TABLE RotationDayRestriction
ADD DayOff [uniqueidentifier] NULL

GO
ALTER TABLE [dbo].[RotationDayRestriction]  WITH CHECK ADD  CONSTRAINT [FK_RotationDayRestriction_DayOff] FOREIGN KEY([DayOff])
REFERENCES [dbo].[DayOff] ([Id])
GO
ALTER TABLE [dbo].[RotationDayRestriction] CHECK CONSTRAINT [FK_RotationDayRestriction_DayOff]
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (14,'7.0.14') 
