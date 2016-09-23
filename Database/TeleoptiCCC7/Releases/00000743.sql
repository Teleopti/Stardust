IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_ExternalLogon]'))
DROP VIEW [dbo].[v_ExternalLogon]
GO
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_PersonOrganizationData]'))
DROP VIEW [dbo].[v_PersonOrganizationData]
GO
ALTER TABLE [dbo].[ExternalLogOn] ALTER COLUMN [AcdLogOnOriginalId]nvarchar(65)
GO