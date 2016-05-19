IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[v_RtaMapping]'))
DROP VIEW [dbo].[v_RtaMapping]
GO

ALTER TABLE dbo.RtaStateGroup
DROP COLUMN IsLogOutState