IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_stg_queue]'))
DROP VIEW [mart].[v_stg_queue]
GO
CREATE VIEW [mart].[v_stg_queue]
AS
SELECT * FROM stage.stg_queue

GO


