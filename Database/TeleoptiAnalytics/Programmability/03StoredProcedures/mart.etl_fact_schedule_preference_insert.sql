IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_preference_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_preference_insert]
GO

-- =============================================
-- Author:		Pontus
-- Description:	Insert fact schedule preference data
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_schedule_preference_insert]
	 @date_id int
	,@interval_id smallint
	,@person_id int
	,@scenario_id smallint
	,@preference_type_id int
	,@shift_category_id int
	,@day_off_id int
	,@preferences_requested int
	,@preferences_fulfilled int
	,@preferences_unfulfilled int
	,@business_unit_id int
	,@datasource_id smallint
	,@insert_date smalldatetime
	,@update_date smalldatetime
	,@datasource_update_date smalldatetime
	,@must_haves int
	,@absence_id int
AS
BEGIN
INSERT INTO [mart].[fact_schedule_preference]
           ([date_id]
           ,[interval_id]
           ,[person_id]
           ,[scenario_id]
           ,[preference_type_id]
           ,[shift_category_id]
           ,[day_off_id]
           ,[preferences_requested]
           ,[preferences_fulfilled]
           ,[preferences_unfulfilled]
           ,[business_unit_id]
           ,[datasource_id]
           ,[insert_date]
           ,[update_date]
           ,[datasource_update_date]
           ,[must_haves]
           ,[absence_id])
     VALUES
           (@date_id
			,@interval_id
			,@person_id
			,@scenario_id
			,@preference_type_id
			,@shift_category_id
			,@day_off_id
			,@preferences_requested
			,@preferences_fulfilled
			,@preferences_unfulfilled
			,@business_unit_id
			,@datasource_id
			,@insert_date
			,@update_date
			,@datasource_update_date
			,@must_haves
			,@absence_id)
END
GO

