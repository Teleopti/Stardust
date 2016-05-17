IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_absence_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_absence_update]
GO

CREATE PROCEDURE [mart].[etl_dim_absence_update]
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
UPDATE [mart].[dim_absence]
   SET [absence_name] = @absence_name
           ,[display_color] = @display_color
		   ,[in_contract_time] = @in_contract_time
		   ,[in_contract_time_name] = @in_contract_time_name
		   ,[in_paid_time] = @in_paid_time
		   ,[in_paid_time_name] = @in_paid_time_name
		   ,[in_work_time] = @in_work_time
		   ,[in_work_time_name] = @in_work_time_name
           ,[datasource_id] = @datasource_id
           ,[update_date] = GETUTCDATE()
		   ,[datasource_update_date] = @datasource_update_date
           ,[is_deleted] = @is_deleted
		   ,[display_color_html] = @display_color_html
		   ,[absence_shortname] = @absence_shortname
	WHERE absence_code = @absence_code AND
		  business_unit_id = @business_unit_id
END
GO
