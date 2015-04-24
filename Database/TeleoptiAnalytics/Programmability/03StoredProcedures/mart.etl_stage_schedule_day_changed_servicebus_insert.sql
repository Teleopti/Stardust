IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_stage_schedule_day_changed_servicebus_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_stage_schedule_day_changed_servicebus_insert]
GO

-- =============================================
-- Author:		Micke
-- Create date: 2015-04-23
-- Description:	Insert schedule day changed row from schedule change event in client
-- =============================================
CREATE PROCEDURE [mart].[etl_stage_schedule_day_changed_servicebus_insert]
	@schedule_date_local smalldatetime,
	@person_code uniqueidentifier,
	@scenario_id uniqueidentifier,
	@business_unit_code uniqueidentifier,
	@datasource_update_date smalldatetime
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [stage].[stg_schedule_changed_servicebus]
			   ([schedule_date_local]
			   ,[person_code]
			   ,[scenario_code]
			   ,[business_unit_code]
			   ,[datasource_id]
			   ,[datasource_update_date])
		 VALUES
			   (@schedule_date_local
			   ,@person_code
			   ,@scenario_id
			   ,@business_unit_code
			   ,1
			   ,@datasource_update_date)
END


GO