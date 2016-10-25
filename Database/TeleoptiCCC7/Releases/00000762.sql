IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'LoadAllRtaMappings')
	DROP PROCEDURE [dbo].[LoadAllRtaMappings]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'LoadRtaMappingsFor')
	DROP PROCEDURE [dbo].[LoadRtaMappingsFor]
GO

IF EXISTS(select * FROM sys.views where name = 'v_PersonOrganizationData')
	DROP VIEW [dbo].[v_PersonOrganizationData]
GO

IF EXISTS(select * FROM sys.views where name = 'v_RtaMapping')
	DROP VIEW [dbo].[v_RtaMapping]
GO

