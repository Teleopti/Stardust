IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_absence_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_absence_insert]
GO

CREATE PROCEDURE [mart].[etl_dim_absence_insert]
	@absence_code uniqueidentifier,
	@absence_name nvarchar(100),
	@display_color int,
	@in_contract_time bit,
	@in_contract_time_name nvarchar(50),
	@in_paid_time bit,
	@in_paid_time_name nvarchar(50),
	@in_work_time bit,
	@in_work_time_name nvarchar(50),
	@business_unit_id int,
	@datasource_id smallint,
	@datasource_update_date smalldatetime,
	@is_deleted bit,
	@display_color_html char(7),
	@absence_shortname nvarchar(25)	
AS
BEGIN
	INSERT INTO [mart].[dim_absence]
           ([absence_code]
           ,[absence_name]
           ,[display_color]
		   ,[in_contract_time]
		   ,[in_contract_time_name]
		   ,[in_paid_time]
		   ,[in_paid_time_name]
		   ,[in_work_time]
		   ,[in_work_time_name]
           ,[business_unit_id]
           ,[datasource_id]
           ,[insert_date]
           ,[update_date]
		   ,[datasource_update_date]
           ,[is_deleted]
		   ,[display_color_html]
		   ,[absence_shortname])
	SELECT 
            @absence_code,
			@absence_name,
			@display_color,
			@in_contract_time,
			@in_contract_time_name,
			@in_paid_time,
			@in_paid_time_name,
			@in_work_time,
			@in_work_time_name,
			@business_unit_id,
			@datasource_id,
			GETUTCDATE(),
			GETUTCDATE(),
			@datasource_update_date,
			@is_deleted,
			@display_color_html,
			@absence_shortname
	WHERE NOT EXISTS (SELECT * FROM [mart].[dim_absence] WHERE absence_code = @absence_code AND
			business_unit_id = @business_unit_id)
END
GO

