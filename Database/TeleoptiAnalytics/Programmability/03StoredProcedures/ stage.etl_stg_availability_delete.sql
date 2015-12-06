IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[etl_stg_availability_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [stage].[etl_stg_availability_delete]
GO



create PROCEDURE [stage].[etl_stg_availability_delete]
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;

    TRUNCATE TABLE  [stage].[stg_hourly_availability]
END


GO

