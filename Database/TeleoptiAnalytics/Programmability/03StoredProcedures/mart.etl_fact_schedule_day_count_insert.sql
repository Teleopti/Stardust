IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_day_count_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_day_count_insert]
GO

-- =============================================
-- Author:		Jonas
-- Create date: 2014-12-12
-- Description:	Insert schedule day count row from schedule change event in client
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_schedule_day_count_insert]
	@shift_startdate_local_id int,
	@person_id int,
	@business_unit_id int,
	@scenario_id smallint,
	@starttime smalldatetime,
	@shift_category_id int,
	@absence_id int,
	@day_off_id int
AS
BEGIN
	SET NOCOUNT ON;

	MERGE [mart].[fact_schedule_day_count] AS target  
    USING (
			SELECT 
				@shift_startdate_local_id,
				@person_id,
				@scenario_id
			) AS src 
					(
						[shift_startdate_local_id]
					   ,[person_id]
					   ,[scenario_id]
					)
		ON (
			target.shift_startdate_local_id = src.shift_startdate_local_id
			AND target.person_id = src.person_id
			AND target.scenario_id = src.scenario_id
			)
	WHEN MATCHED THEN
		UPDATE SET
			[starttime] = @starttime
			,[shift_category_id] = @shift_category_id
			,[day_off_id] = @day_off_id
			,[absence_id] = @absence_id
			,[datasource_update_date] = GETDATE()
	WHEN NOT MATCHED THEN
		INSERT (
					[shift_startdate_local_id]
				   ,[person_id]
				   ,[scenario_id]
				   ,[starttime]
				   ,[shift_category_id]
				   ,[day_off_id]
				   ,[absence_id]
				   ,[day_count]
				   ,[business_unit_id]
				   ,[datasource_update_date]
				)
		VALUES (
					@shift_startdate_local_id
				   ,@person_id
				   ,@scenario_id
				   ,@starttime
				   ,@shift_category_id
				   ,@day_off_id
				   ,@absence_id
				   ,1
				   ,@business_unit_id
				   ,GETDATE()
				);
END

GO


