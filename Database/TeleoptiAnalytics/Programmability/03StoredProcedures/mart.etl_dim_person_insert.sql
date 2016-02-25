IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_person_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_person_insert]
GO

-- =============================================
-- Description:	Insert new person periods
-- =============================================

CREATE PROCEDURE [mart].[etl_dim_person_insert]
	(@person_code uniqueidentifier 
	,@valid_from_date smalldatetime 
	,@valid_to_date smalldatetime 
	,@valid_from_date_id int 
	,@valid_from_interval_id int 
	,@valid_to_date_id int 
	,@valid_to_interval_id int 
	,@person_period_code uniqueidentifier 
	,@person_name nvarchar(200) 
	,@first_name nvarchar(30) 
	,@last_name nvarchar(30) 
	,@employment_number nvarchar(50) 
	,@employment_type_code int 
	,@employment_type_name nvarchar(50) 
	,@contract_code uniqueidentifier 
	,@contract_name nvarchar(50) 
	,@parttime_code uniqueidentifier 
	,@parttime_percentage nvarchar(50) 
	,@team_id int 
	,@team_code uniqueidentifier 
	,@team_name nvarchar(50) 
	,@site_id int 
	,@site_code uniqueidentifier 
	,@site_name nvarchar(50) 
	,@business_unit_id int 
	,@business_unit_code uniqueidentifier 
	,@business_unit_name nvarchar(50) 
	,@skillset_id int 
	,@email nvarchar(200) 
	,@note nvarchar(1024) 
	,@employment_start_date smalldatetime 
	,@employment_end_date smalldatetime 
	,@time_zone_id int 
	,@is_agent bit 
	,@is_user bit 
	,@datasource_id smallint 
	,@insert_date smalldatetime 
	,@update_date smalldatetime 
	,@datasource_update_date smalldatetime 
	,@to_be_deleted bit 
	,@windows_domain nvarchar(50) 
	,@windows_username nvarchar(50) 
	,@valid_to_date_id_maxDate int 
	,@valid_to_interval_id_maxDate int 
	,@valid_from_date_id_local int 
	,@valid_to_date_id_local int 
	,@valid_from_date_local smalldatetime 
	,@valid_to_date_local smalldatetime )
AS
BEGIN
	INSERT INTO [mart].[dim_person]
        ([person_code]
        ,[valid_from_date]
        ,[valid_to_date]
        ,[valid_from_date_id]
        ,[valid_from_interval_id]
        ,[valid_to_date_id]
        ,[valid_to_interval_id]
        ,[person_period_code]
        ,[person_name]
        ,[first_name]
        ,[last_name]
        ,[employment_number]
        ,[employment_type_code]
        ,[employment_type_name]
        ,[contract_code]
        ,[contract_name]
        ,[parttime_code]
        ,[parttime_percentage]
        ,[team_id]
        ,[team_code]
        ,[team_name]
        ,[site_id]
        ,[site_code]
        ,[site_name]
        ,[business_unit_id]
        ,[business_unit_code]
        ,[business_unit_name]
        ,[skillset_id]
        ,[email]
        ,[note]
        ,[employment_start_date]
        ,[employment_end_date]
        ,[time_zone_id]
        ,[is_agent]
        ,[is_user]
        ,[datasource_id]
        ,[insert_date]
        ,[update_date]
        ,[datasource_update_date]
        ,[to_be_deleted]
        ,[windows_domain]
        ,[windows_username]
        ,[valid_to_date_id_maxDate]
        ,[valid_to_interval_id_maxDate]
        ,[valid_from_date_id_local]
        ,[valid_to_date_id_local]
        ,[valid_from_date_local]
        ,[valid_to_date_local])
     VALUES
		(@person_code
		,@valid_from_date
		,@valid_to_date
		,@valid_from_date_id
		,@valid_from_interval_id
		,@valid_to_date_id
		,@valid_to_interval_id
		,@person_period_code
		,@person_name
		,@first_name
		,@last_name
		,@employment_number
		,@employment_type_code
		,@employment_type_name
		,@contract_code
		,@contract_name
		,@parttime_code
		,@parttime_percentage
		,@team_id
		,@team_code
		,@team_name
		,@site_id
		,@site_code
		,@site_name
		,@business_unit_id
		,@business_unit_code
		,@business_unit_name
		,@skillset_id
		,@email
		,@note
		,@employment_start_date
		,@employment_end_date
		,@time_zone_id
		,@is_agent
		,@is_user
		,@datasource_id
		,@insert_date
		,@update_date
		,@datasource_update_date
		,@to_be_deleted
		,@windows_domain
		,@windows_username
		,@valid_to_date_id_maxDate
		,@valid_to_interval_id_maxDate
		,@valid_from_date_id_local
		,@valid_to_date_id_local
		,@valid_from_date_local
		,@valid_to_date_local)

END

GO
