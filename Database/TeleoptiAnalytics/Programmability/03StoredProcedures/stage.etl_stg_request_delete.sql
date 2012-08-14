IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Stage].[etl_stg_request_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Stage].[etl_stg_request_delete]
GO


create PROCEDURE [stage].[etl_stg_request_delete]
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;

    TRUNCATE TABLE stage.stg_request
END


GO