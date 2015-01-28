----------------  
--Name: Robin Karlsson
--Date: 2015-01-28
--Desc: Remove unused table for avilable people in role
----------------  
SET NOCOUNT ON
	
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AvailablePersonsInApplicationRole_AvailableData]') AND parent_object_id = OBJECT_ID(N'[dbo].[AvailablePersonsInApplicationRole]'))
ALTER TABLE [dbo].[AvailablePersonsInApplicationRole] DROP CONSTRAINT [FK_AvailablePersonsInApplicationRole_AvailableData]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AvailablePersonsInApplicationRole_Person]') AND parent_object_id = OBJECT_ID(N'[dbo].[AvailablePersonsInApplicationRole]'))
ALTER TABLE [dbo].[AvailablePersonsInApplicationRole] DROP CONSTRAINT [FK_AvailablePersonsInApplicationRole_Person]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AvailablePersonsInApplicationRole_InsertedOn]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AvailablePersonsInApplicationRole] DROP CONSTRAINT [DF_AvailablePersonsInApplicationRole_InsertedOn]
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AvailablePersonsInApplicationRole]') AND type in (N'U'))
DROP TABLE [dbo].[AvailablePersonsInApplicationRole]
GO

SET NOCOUNT OFF
GO

