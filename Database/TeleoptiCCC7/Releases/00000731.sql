-- Convert to seconds
UPDATE [dbo].[RtaRule] SET ThresholdTime = ThresholdTime / 10000000
GO

-- dropping column dependencies, will be recreated later when Programmability is applied, if those files still exist
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadRtaMappingsFor]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LoadRtaMappingsFor] 
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadAllRtaMappings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LoadAllRtaMappings]
GO
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_RtaMapping]'))
DROP VIEW [dbo].[v_RtaMapping]
GO
ALTER TABLE [dbo].[RtaRule] ALTER COLUMN [ThresholdTime] int NOT NULL
GO
