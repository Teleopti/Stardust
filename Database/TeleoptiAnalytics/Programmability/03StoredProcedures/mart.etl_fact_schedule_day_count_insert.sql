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
	@day_off_name nvarchar(50),
	@day_off_shortname nvarchar(25),
	@day_off_id int
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [mart].[fact_schedule_day_count]
           ([shift_startdate_local_id]
           ,[person_id]
           ,[scenario_id]
           ,[starttime]
           ,[shift_category_id]
           ,[day_off_id]
           ,[absence_id]
           ,[day_count]
           ,[business_unit_id]
           ,[datasource_update_date])
     VALUES
           (@shift_startdate_local_id
           ,@person_id
           ,@scenario_id
           ,@starttime
           ,@shift_category_id
           ,@day_off_id
           ,@absence_id
           ,1
           ,@business_unit_id
           ,GETDATE())

END

GO


