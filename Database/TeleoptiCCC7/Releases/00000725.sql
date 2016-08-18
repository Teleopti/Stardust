IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadPersonOrganizationData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LoadPersonOrganizationData]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadAllPersonOrganizationData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LoadAllPersonOrganizationData]
GO

