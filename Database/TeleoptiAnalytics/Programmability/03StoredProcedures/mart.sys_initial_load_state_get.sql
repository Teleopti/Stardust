IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_initial_load_state_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_initial_load_state_get]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		JN
-- Create date: 2009-11-16
-- Description:	Find out if ETL Tools Initial Load has been executed or not or if it´s in an invalid state.
--				This is made by examining three key tables: mart.dim_interval, mart.dim_date and mart.dim_time_zone.
--				Returns an integer variable called @etl_tool_state telling the state of the ETL Tool.
--				0: ETL Tool (Initial Load) have never been executed
--				1: ETL Tool is in an invalid state
--				2: ETL Tool is in an valid state
-- =============================================
CREATE PROCEDURE [mart].[sys_initial_load_state_get]
AS
BEGIN
	DECLARE @dim_interval_count int, 
			@dim_date_count int, 
			@dim_time_zone_count int, 
			@all_count int,
			@etl_tool_state int -- 0: Never executed; 1: Invalid state; 2: Valid state
			
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SET @etl_tool_state = 2
	SELECT @dim_interval_count = COUNT(*) FROM mart.dim_interval
	SELECT @dim_date_count = COUNT(*) FROM mart.dim_date
	SELECT @dim_time_zone_count = COUNT(*) FROM mart.dim_time_zone
	SET @all_count = @dim_interval_count + @dim_date_count + @dim_time_zone_count

	IF @all_count = 0
	BEGIN
		-- ETL Tool (Initial Load) have never been executed
		SET @etl_tool_state = 0
	END
	ELSE IF (@dim_interval_count = 0) AND (@all_count > 0) OR
			(@dim_date_count = 0) AND (@all_count > 0) OR
			(@dim_time_zone_count = 0) AND (@all_count > 0)
	BEGIN
		-- ETL Tool is in an invalid state
		SET @etl_tool_state = 1
	END

	SELECT @etl_tool_state
END

GO