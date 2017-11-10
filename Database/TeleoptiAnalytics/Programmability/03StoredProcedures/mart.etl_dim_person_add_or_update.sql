IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_person_add_or_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_person_add_or_update]
GO
-- =============================================
-- Description:	add or update dim_person
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_person_add_or_update] 
	@person_code uniqueidentifier
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
	,@datasource_update_date smalldatetime 
	,@windows_domain nvarchar(50) 
	,@windows_username nvarchar(50) 
	,@valid_to_date_id_maxDate int 
	,@valid_to_interval_id_maxDate int 
	,@valid_from_date_id_local int 
	,@valid_to_date_id_local int 
	,@valid_from_date_local smalldatetime 
	,@valid_to_date_local smalldatetime
AS

MERGE mart.dim_person AS target  
    USING (SELECT 
				@person_code
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
				,@datasource_update_date
				,@windows_domain
				,@windows_username
				,@valid_to_date_id_maxDate 
				,@valid_to_interval_id_maxDate 
				,@valid_from_date_id_local 
				,@valid_to_date_id_local 
				,@valid_from_date_local
				,@valid_to_date_local
				) AS src 
					(
						person_code, 
						valid_from_date, 
						valid_to_date, 
						valid_from_date_id, 
						valid_from_interval_id, 
						valid_to_date_id, 
						valid_to_interval_id, 
						person_period_code, 
						person_name, 
						first_name, 
						last_name, 
						employment_number, 
						employment_type_code, 
						employment_type_name, 
						contract_code, 
						contract_name, 
						parttime_code, 
						parttime_percentage, 
						team_id, 
						team_code, 
						team_name, 
						site_id, 
						site_code, 
						site_name, 
						business_unit_id, 
						business_unit_code, 
						business_unit_name, 
						skillset_id, 
						email, 
						note, 
						employment_start_date, 
						employment_end_date, 
						time_zone_id, 
						is_agent, 
						datasource_update_date, 
						windows_domain, 
						windows_username, 
						valid_to_date_id_maxDate, 
						valid_to_interval_id_maxDate, 
						valid_from_date_id_local, 
						valid_to_date_id_local, 
						valid_from_date_local, 
						valid_to_date_local
					)
		ON (target.person_period_code = src.person_period_code)  
    WHEN MATCHED THEN   
        UPDATE SET 
				target.valid_from_date = src.valid_from_date
				,target.valid_to_date =  src.valid_to_date
				,target.valid_from_date_id = src.valid_from_date_id
				,target.valid_from_interval_id = src.valid_from_interval_id
				,target.valid_to_date_id = src.valid_to_date_id
				,target.valid_to_interval_id = src.valid_to_interval_id
				,target.person_name = src.person_name
				,target.first_name = src.first_name
				,target.last_name = src.last_name
				,target.employment_number = src.employment_number
				,target.employment_type_code = src.employment_type_code
				,target.employment_type_name = src.employment_type_name
				,target.contract_code = src.contract_code
				,target.contract_name = src.contract_name
				,target.parttime_code = src.parttime_code
				,target.parttime_percentage = src.parttime_percentage
				,target.team_id = src.team_id
				,target.team_code = src.team_code
				,target.team_name = src.team_name
				,target.site_id = src.site_id
				,target.site_code = src.site_code
				,target.site_name = src.site_name
				,target.business_unit_id = src.business_unit_id
				,target.business_unit_code = src.business_unit_code
				,target.business_unit_name = src.business_unit_name
				,target.skillset_id = src.skillset_id
				,target.email = src.email
				,target.note = src.note
				,target.employment_start_date = src.employment_start_date
				,target.employment_end_date = src.employment_end_date
				,target.time_zone_id = src.time_zone_id
				,target.is_agent = src.is_agent
				,target.update_date = GETDATE()
				,target.datasource_update_date = src.datasource_update_date
				,target.windows_domain = src.windows_domain
				,target.windows_username = src.windows_username
				,target.valid_to_date_id_maxDate = src.valid_to_date_id_maxDate
				,target.valid_to_interval_id_maxDate = src.valid_to_interval_id_maxDate
				,target.valid_from_date_id_local = src.valid_from_date_id_local
				,target.valid_to_date_id_local = src.valid_to_date_id_local
				,target.valid_from_date_local = src.valid_from_date_local
				,target.valid_to_date_local = src.valid_to_date_local
WHEN NOT MATCHED THEN  
    INSERT (
				person_code, 
				valid_from_date, 
				valid_to_date, 
				valid_from_date_id, 
				valid_from_interval_id, 
				valid_to_date_id, 
				valid_to_interval_id, 
				person_period_code, 
				person_name, 
				first_name, 
				last_name, 
				employment_number, 
				employment_type_code, 
				employment_type_name, 
				contract_code, 
				contract_name, 
				parttime_code, 
				parttime_percentage, 
				team_id, 
				team_code, 
				team_name, 
				site_id, 
				site_code, 
				site_name, 
				business_unit_id, 
				business_unit_code, 
				business_unit_name, 
				skillset_id, 
				email, 
				note, 
				employment_start_date, 
				employment_end_date, 
				time_zone_id, 
				is_agent, 
				is_user, 
				datasource_id, 
				datasource_update_date, 
				to_be_deleted, 
				windows_domain, 
				windows_username, 
				valid_to_date_id_maxDate, 
				valid_to_interval_id_maxDate, 
				valid_from_date_id_local, 
				valid_to_date_id_local, 
				valid_from_date_local, 
				valid_to_date_local
			)
    VALUES (
			src.person_code, 
			src.valid_from_date, 
			src.valid_to_date, 
			src.valid_from_date_id, 
			src.valid_from_interval_id, 
			src.valid_to_date_id, 
			src.valid_to_interval_id, 
			src.person_period_code, 
			src.person_name, 
			src.first_name, 
			src.last_name, 
			src.employment_number, 
			src.employment_type_code, 
			src.employment_type_name, 
			src.contract_code, 
			src.contract_name, 
			src.parttime_code, 
			src.parttime_percentage, 
			src.team_id, 
			src.team_code, 
			src.team_name, 
			src.site_id, 
			src.site_code, 
			src.site_name, 
			src.business_unit_id, 
			src.business_unit_code, 
			src.business_unit_name, 
			src.skillset_id, 
			src.email, 
			src.note, 
			src.employment_start_date, 
			src.employment_end_date, 
			src.time_zone_id, 
			src.is_agent, 
			0, 
			1, 
			src.datasource_update_date, 
			0, 
			src.windows_domain, 
			src.windows_username, 
			src.valid_to_date_id_maxDate, 
			src.valid_to_interval_id_maxDate, 
			src.valid_from_date_id_local, 
			src.valid_to_date_id_local, 
			src.valid_from_date_local, 
			src.valid_to_date_local
			);

GO