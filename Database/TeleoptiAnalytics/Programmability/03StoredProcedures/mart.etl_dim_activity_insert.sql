IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_activity_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_activity_insert]
GO

CREATE PROCEDURE [mart].[etl_dim_activity_insert]
	@activity_code uniqueidentifier,
    @activity_name nvarchar(100),
    @display_color int,
    @in_ready_time bit,
    @in_ready_time_name nvarchar(50),
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
	@display_color_html char(7)
AS
BEGIN
INSERT INTO [mart].[dim_activity]
           ([activity_code]
           ,[activity_name]
           ,[display_color]
           ,[in_ready_time]
           ,[in_ready_time_name]
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
           ,[display_color_html])
     SELECT 
           @activity_code
           ,@activity_name
           ,@display_color
           ,@in_ready_time
           ,@in_ready_time_name
           ,@in_contract_time
           ,@in_contract_time_name
           ,@in_paid_time
           ,@in_paid_time_name
           ,@in_work_time
           ,@in_work_time_name
           ,@business_unit_id
           ,@datasource_id
		   ,GETUTCDATE()
		   ,GETUTCDATE()
           ,@datasource_update_date
           ,@is_deleted
           ,@display_color_html
	WHERE NOT EXISTS (SELECT * FROM [mart].[dim_activity] WHERE activity_code = @activity_code AND
			business_unit_id = @business_unit_id)
END
GO
