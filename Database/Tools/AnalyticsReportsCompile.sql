SET NOCOUNT ON
BEGIN TRY
	--sp logic
	DECLARE @param_name nvarchar(50)
	DECLARE @SPname sysname
	DECLARE @schema nvarchar(100)
	DECLARE @dataTypeSpec TABLE ([Name] sysname, [DataType] sysname, [Length] smallint, [NumericPrecision] smallint, [NumericScale] smallint)
	DECLARE @parameter_id int
	DECLARE @report_id uniqueidentifier
	DECLARE @paramString nvarchar(2000)
	DECLARE @SQLString nvarchar(2000)
	SET @schema  = N'mart'


	--test data
	declare @scheduling_type_id int = 1
	declare @scenario_code uniqueidentifier = 'C2BAEB11-9E91-40FA-AA5A-1DD963BF834D'
	declare @time_zone_code nvarchar(20) = 'W. Europe Standard Time'
	declare @dateFrom smalldatetime ='2013-01-04 00:00:00'
	declare @dateTo smalldatetime ='2013-01-04 00:00:00'
	declare @QueueList varchar(10) = '12,2,3,6'
	declare @start_date datetime ='2013-01-04 00:00:00'
	declare @end_date datetime ='2013-01-04 00:00:00'
	declare @skill uniqueidentifier = 'C2BAEB11-9E91-40FA-AA5A-1DD963BF834D'
	DECLARE @datasource_id smallint = 1
	DECLARE @log_object_name nvarchar(100) = 'Teleopti CCC - File import'
	declare @bu_id uniqueidentifier = 'C2BAEB11-9E91-40FA-AA5A-1DD963BF834D'
	declare @business_hierarchy_code uniqueidentifier = 'C2BAEB11-9E91-40FA-AA5A-1DD963BF834D'
	declare @param int = 3
	declare @person_category_type_id int = 1
	declare @absence_set nvarchar(100) = '2'
	declare @activity_set nvarchar(100) = '1'
	declare @adherence_id int = 1
	declare @agent_code uniqueidentifier = 'C2BAEB11-9E91-40FA-AA5A-1DD963BF834D'
	declare @agent_person_code uniqueidentifier = 'C2BAEB12-9E91-40FA-AA5A-1DD963BF834D'
	declare @agent_set nvarchar(100) = 'C2BAEB13-9E91-40FA-AA5A-1DD963BF834D'
	declare @business_unit_code uniqueidentifier = 'C2BAEB14-9E91-40FA-AA5A-1DD963BF834D'
	declare @date_from datetime = '2013-01-04 00:00:00'
	declare @date_to datetime = '2013-01-04 00:00:00'
	declare @day_off_set nvarchar(100) = '1'
	declare @from_matrix bit = 1
	declare @group_page_agent_code uniqueidentifier = 'C2BAEB15-9E91-40FA-AA5A-1DD963BF834D'
	declare @group_page_agent_set nvarchar(100) = 'C2BAEB16-9E91-40FA-AA5A-1DD963BF834D'
	declare @group_page_code uniqueidentifier = 'C2BAEB17-9E91-40FA-AA5A-1DD963BF834D'
	declare @group_page_group_id int = 2
	declare @group_page_group_set nvarchar(100) = '2,3'
	declare @interval_from int = 3
	declare @interval_to int = 4
	declare @interval_type int = 1
	declare @language_id int = 1
	declare @overtime_set nvarchar(100) = '3,4,5'
	declare @person_code uniqueidentifier = 'C2BAEB18-9E91-40FA-AA5A-1DD963BF834D'
	declare @queue_set nvarchar(100) = '1,2,3,5'
	declare @scenario_id int = 1
	declare @shift_category_set nvarchar(100) = '1,4,5'
	declare @site_id int = 1
	declare @skill_set nvarchar(100) = '3,4,5'
	declare @sl_calc_id int = 1
	declare @sort_by int = 1
	declare @team_id int = 1
	declare @team_set nvarchar(100) = '1,2'
	declare @time_zone_id int = 1
	declare @workload_set nvarchar(100) = '3,4,5'
	declare @request_type_id int = -2
	declare @date_from_id int = 200
	declare @date_to_id int = 300
	declare @active bit = 1


	--All stored procedures used(?)
		DECLARE ReportSPs CURSOR FOR
		SELECT Id, replace(proc_name,'mart.','') FROM mart.report where proc_name <> ''
		UNION ALL
		select Id,replace(fill_proc_name,'mart.','') from mart.report_control where fill_proc_name<>'1'
		UNION ALL
		SELECT NEWID(),name FROM sys.procedures WHERE name like 'report_data_%'
		AND name <>'report_data_schedule_result_subSP'

		OPEN ReportSPs;
				FETCH NEXT FROM ReportSPs INTO @report_id, @SPname;
				WHILE @@FETCH_STATUS = 0
				BEGIN
				
				--re-init for each loop
				SET @paramString = ''
				DELETE FROM @dataTypeSpec
				
				--all parameters per stored procedure
				DECLARE params CURSOR FOR
					SELECT
					param.parameter_id AS [parameter_id],
					param.name AS [param_name]
					FROM
					sys.all_objects AS sp
					INNER JOIN sys.all_parameters AS param
						ON param.object_id=sp.object_id
					WHERE
					(sp.type = N'P' OR sp.type = N'RF' OR sp.type=N'PC')
					AND (sp.name=@SPname
					AND SCHEMA_NAME(sp.schema_id)=@schema)
					ORDER BY [parameter_id] ASC	
				OPEN params;
					FETCH NEXT FROM params INTO @parameter_id, @param_name;
					WHILE @@FETCH_STATUS = 0
					BEGIN
						
						INSERT INTO @dataTypeSpec
						exec sp_executesql N'SELECT
								param.name AS [Name],
								usrt.name AS [DataType],
								CAST(CASE WHEN baset.name IN (N''nchar'', N''nvarchar'') AND param.max_length <> -1 THEN param.max_length/2 ELSE param.max_length END AS int) AS [Length],
								CAST(param.precision AS int) AS [NumericPrecision],
								CAST(param.scale AS int) AS [NumericScale]
								FROM
								sys.all_objects AS sp
								INNER JOIN sys.all_parameters AS param ON param.object_id=sp.object_id
								LEFT OUTER JOIN sys.types AS usrt ON usrt.user_type_id = param.user_type_id
								LEFT OUTER JOIN sys.schemas AS sparam ON sparam.schema_id = usrt.schema_id
								LEFT OUTER JOIN sys.types AS baset ON (baset.user_type_id = param.system_type_id and baset.user_type_id = baset.system_type_id) or ((baset.system_type_id = param.system_type_id) and (baset.user_type_id = param.user_type_id) and (baset.is_user_defined = 0) and (baset.is_assembly_type = 1)) 
								LEFT OUTER JOIN sys.xml_schema_collections AS xscparam ON xscparam.xml_collection_id = param.xml_collection_id
								LEFT OUTER JOIN sys.schemas AS s2param ON s2param.schema_id = xscparam.schema_id
								WHERE
								(param.name=@_msparam_0)and((sp.type = @_msparam_1 OR sp.type = @_msparam_2 OR sp.type=@_msparam_3)and(sp.name=@_msparam_4 and SCHEMA_NAME(sp.schema_id)=@_msparam_5))',N'@_msparam_0 nvarchar(4000),@_msparam_1 nvarchar(4000),@_msparam_2 nvarchar(4000),@_msparam_3 nvarchar(4000),@_msparam_4 nvarchar(4000),@_msparam_5 nvarchar(4000)',@_msparam_0=@param_name,@_msparam_1=N'P',@_msparam_2=N'RF',@_msparam_3=N'PC',@_msparam_4=@SPname,@_msparam_5=@schema
						
						--prepare all parameters 
						SELECT @paramString = @paramString + @param_name + '=''' + 
						CASE
							WHEN  @param_name = '@absence_set' THEN CAST(@absence_set AS nvarchar(50))
							WHEN  @param_name = '@activity_set' THEN CAST(@activity_set AS nvarchar(50))
							WHEN  @param_name = '@adherence_id' THEN CAST(@adherence_id AS nvarchar(50))
							WHEN  @param_name = '@agent_code' THEN CAST(@agent_code AS nvarchar(50))
							WHEN  @param_name = '@agent_person_code' THEN CAST(@agent_person_code AS nvarchar(50))
							WHEN  @param_name = '@agent_set' THEN CAST(@agent_set AS nvarchar(50))
							WHEN  @param_name = '@business_unit_code' THEN CAST(@business_unit_code AS nvarchar(50))
							WHEN  @param_name = '@date_from' THEN CAST(@date_from AS nvarchar(50))
							WHEN  @param_name = '@date_to' THEN CAST(@date_to AS nvarchar(50))
							WHEN  @param_name = '@day_off_set' THEN CAST(@day_off_set AS nvarchar(50))
							WHEN  @param_name = '@from_matrix' THEN CAST(@from_matrix AS nvarchar(50))
							WHEN  @param_name = '@group_page_agent_code' THEN CAST(@group_page_agent_code AS nvarchar(50))
							WHEN  @param_name = '@group_page_agent_set' THEN CAST(@group_page_agent_set AS nvarchar(50))
							WHEN  @param_name = '@group_page_code' THEN CAST(@group_page_code AS nvarchar(50))
							WHEN  @param_name = '@group_page_group_id' THEN CAST(@group_page_group_id AS nvarchar(50))
							WHEN  @param_name = '@group_page_group_set' THEN CAST(@group_page_group_set AS nvarchar(50))
							WHEN  @param_name = '@interval_from' THEN CAST(@interval_from AS nvarchar(50))
							WHEN  @param_name = '@interval_to' THEN CAST(@interval_to AS nvarchar(50))
							WHEN  @param_name = '@interval_type' THEN CAST(@interval_type AS nvarchar(50))
							WHEN  @param_name = '@language_id' THEN CAST(@language_id AS nvarchar(50))
							WHEN  @param_name = '@overtime_set' THEN CAST(@overtime_set AS nvarchar(50))
							WHEN  @param_name = '@person_code' THEN CAST(@person_code AS nvarchar(50))
							WHEN  @param_name = '@queue_set' THEN CAST(@queue_set AS nvarchar(50))
							WHEN  @param_name = '@report_id' THEN CAST(@report_id AS nvarchar(50))
							WHEN  @param_name = '@scenario_id' THEN CAST(@scenario_id AS nvarchar(50))
							WHEN  @param_name = '@shift_category_set' THEN CAST(@shift_category_set AS nvarchar(50))
							WHEN  @param_name = '@site_id' THEN CAST(@site_id AS nvarchar(50))
							WHEN  @param_name = '@skill_set' THEN CAST(@skill_set AS nvarchar(50))
							WHEN  @param_name = '@sl_calc_id' THEN CAST(@sl_calc_id AS nvarchar(50))
							WHEN  @param_name = '@sort_by' THEN CAST(@sort_by AS nvarchar(50))
							WHEN  @param_name = '@team_id' THEN CAST(@team_id AS nvarchar(50))
							WHEN  @param_name = '@team_set' THEN CAST(@team_set AS nvarchar(50))
							WHEN  @param_name = '@time_zone_id' THEN CAST(@time_zone_id AS nvarchar(50))
							WHEN  @param_name = '@workload_set' THEN CAST(@workload_set AS nvarchar(50))
							WHEN  @param_name = '@bu_id' THEN CAST(@bu_id AS nvarchar(50))
							WHEN  @param_name = '@business_hierarchy_code' THEN CAST(@business_hierarchy_code AS nvarchar(50))
							WHEN  @param_name = '@date_from' THEN CAST(@date_from AS nvarchar(50))
							WHEN  @param_name = '@date_to' THEN CAST(@date_to AS nvarchar(50))
							WHEN  @param_name = '@group_page_code' THEN CAST(@group_page_code AS nvarchar(50))
							WHEN  @param_name = '@group_page_group_id' THEN CAST(@group_page_group_id AS nvarchar(50))
							WHEN  @param_name = '@language_id' THEN CAST(@language_id AS nvarchar(50))
							WHEN  @param_name = '@param' THEN CAST(@param AS nvarchar(50))
							WHEN  @param_name = '@person_category_type_id' THEN CAST(@person_category_type_id AS nvarchar(50))
							WHEN  @param_name = '@person_code' THEN CAST(@person_code AS nvarchar(50))
							WHEN  @param_name = '@site_id' THEN CAST(@site_id AS nvarchar(50))
							WHEN  @param_name = '@team_id' THEN CAST(@team_id AS nvarchar(50))
							WHEN  @param_name = '@workload_set' THEN CAST(@workload_set AS nvarchar(50))
							WHEN  @param_name = '@datasource_id' THEN CAST(@datasource_id AS nvarchar(50))
							WHEN  @param_name = '@skill' THEN CAST(@skill AS nvarchar(50))
							WHEN  @param_name = '@end_date' THEN CAST(@end_date AS nvarchar(50))
							WHEN  @param_name = '@start_date' THEN CAST(@start_date AS nvarchar(50))
							WHEN  @param_name = '@dateFrom' THEN CAST(@dateFrom AS nvarchar(50))
							WHEN  @param_name = '@dateTo' THEN CAST(@dateTo AS nvarchar(50))
							WHEN  @param_name = '@QueueList' THEN CAST(@QueueList AS nvarchar(50))
							WHEN  @param_name = '@time_zone_code' THEN CAST(@time_zone_code AS nvarchar(50))
							WHEN  @param_name = '@scenario_code' THEN CAST(@scenario_code AS nvarchar(50))
							WHEN  @param_name = '@request_type_id' THEN CAST(@request_type_id AS nvarchar(50))
							WHEN  @param_name = '@date_from_id' THEN CAST(@date_from_id AS nvarchar(50))
							WHEN  @param_name = '@date_to_id' THEN CAST(@date_to_id AS nvarchar(50))
							WHEN  @param_name = '@scheduling_type_id' THEN CAST(@scheduling_type_id AS nvarchar(50))
							WHEN  @param_name = '@active' THEN CAST(@active AS nvarchar(50))
						
							ELSE 'noDataTypeFound'
						END
						+ ''','
						FROM @dataTypeSpec WHERE [Name] = @param_name
						FETCH NEXT FROM params INTO @parameter_id, @param_name;
					END
					CLOSE params;
					DEALLOCATE params;
					
					--trim last ','
					IF LEN(@paramString) > 0
					SET @paramString = LEFT(@paramString, LEN(@paramString) - 1)
					
					--add SP name + parameter string
					SELECT @SQLString = '['+@schema+'].[' + @SPname + '] ' + @paramString;
					
					--dubug
					--PRINT @SQLString
					
					--exec
					EXEC sp_executesql @SQLString
					
					FETCH NEXT FROM ReportSPs INTO @report_id, @SPname;
				END
				CLOSE ReportSPs;
				DEALLOCATE ReportSPs;
	--SELECT distinct 'WHEN  @param_name = '''+ Name + ''' THEN CAST(' + Name +' AS nvarchar(50))' FROM @dataTypeSpec
END TRY	
BEGIN CATCH
    DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT 
        @ErrorMessage = ERROR_MESSAGE(),
        @ErrorSeverity = ERROR_SEVERITY(),
        @ErrorState = ERROR_STATE();
    
    SELECT @ErrorMessage = 'Running "AnalyticsReportsCompile.sql" to check SP interfaces and I got an error!'
		+ char(13)+char(10)+
		'The Call: "' + + @SQLString + '" failed with message: '+ @ErrorMessage + 
		', @ErrorSeverity is: '+ ISNULL(CAST(@ErrorSeverity AS nvarchar(10)),'Nothing') + '. @ErrorState is: ' + ISNULL(CAST(@ErrorState AS nvarchar(10)),'Nothing')

	CLOSE ReportSPs;
	DEALLOCATE ReportSPs;

	-- Return an error with hardcoded Severity + State to force an abort of SQLCMD and CCnet
    RAISERROR (@ErrorMessage, 16, 127);
END CATCH;

SET NOCOUNT OFF