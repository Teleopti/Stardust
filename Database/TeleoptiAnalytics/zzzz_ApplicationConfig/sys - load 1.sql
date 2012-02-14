--=========================
--Please uncomment add make manual config here:

:SETVAR TeleoptiAnalytics_Stage STC905_TeleoptiAnalytics_Stage
:SETVAR TeleoptiAnalytics STC905_TeleoptiAnalytics
:SETVAR TeleoptiCCCAgg STC905_TeleoptiCCC_Agg
:SETVAR BOStore STC905_TeleoptiAnalytics_Stage

--=========================

SET NOCOUNT ON

--Declare
DECLARE @myTeleoptiAnalytics_Stage varchar(100)
DECLARE @myTeleoptiAnalytics varchar(100)
DECLARE @myTeleoptiCCCagg varchar(100)
DECLARE @myTeleoptiMessaging varchar(100)
DECLARE @myBOStore varchar(100)

DECLARE @TeleoptiAnalytics_Stage varchar(100)
DECLARE @TeleoptiAnalytics varchar(100)
DECLARE @TeleoptiCCCagg varchar(100)
DECLARE @BOStore varchar(100)

--Init
--This is the orgiginal names, do NOT edit this section!
SET @TeleoptiAnalytics_Stage	= 'TeleoptiAnalytics_Stage'
SET @TeleoptiAnalytics			= 'TeleoptiAnalytics'
SET @TeleoptiCCCagg				= 'TeleoptiCCCAgg'
SET @BOStore					= 'BOStore'

SET @myTeleoptiAnalytics_Stage	= '$(TeleoptiAnalytics_Stage)'
SET @myTeleoptiAnalytics		= '$(TeleoptiAnalytics)'
SET @myTeleoptiCCCagg			= '$(TeleoptiCCCAgg)'
SET @myBOStore					= '$(BOStore)'

--Load sys_datasource
EXEC $(TeleoptiAnalytics).dbo.sys_datasource_load
SELECT * FROM $(TeleoptiAnalytics).dbo.v_sys_datasource
--Load intital qeueu data from Agg-database
--Loop each log-object
DECLARE @mydatasource_id int
SET @mydatasource_id = 0

DECLARE datasource_id CURSOR FOR
	SELECT datasource_id FROM $(TeleoptiAnalytics).dbo.v_sys_datasource
	WHERE datasource_type_name = 'Teleopti CCC Agg'

	OPEN datasource_id
	FETCH NEXT FROM datasource_id INTO @mydatasource_id
	 
	--looping through each log-object 
	WHILE @@fetch_status = 0 
	BEGIN 
		
		--Load table dim_queue
		EXEC $(TeleoptiAnalytics).dbo.etl_dim_queue_load @datasource_id = @mydatasource_id

		FETCH NEXT FROM datasource_id INTO @mydatasource_id
	END
CLOSE datasource_id
DEALLOCATE datasource_id

--============================
--Config ETL app.config (interval)
PRINT '---'
PRINT 'Use this in your app.config file'
SELECT intervals_per_day FROM $(TeleoptiCCCAgg).dbo.log_object
SELECT MIN(interval), MAX(interval) FROM $(TeleoptiCCCAgg).dbo.agent_logg
--============================		

--============================		
--RUN ETL tool = Initial Load
PRINT '---'
PRINT 'Use this for the Intial Load'
SELECT min(date_from) AS date, 'min_agent' AS data FROM $(TeleoptiCCCAgg).dbo.agent_logg
UNION ALL
SELECT min(date_from) AS date, 'min_queue' AS data FROM $(TeleoptiCCCAgg).dbo.queue_logg
--============================

/*
UPDATE v_sys_datasource SET time_zone_id = 0 WHERE datasource_id NOT IN (-1)
SELECT * FROM v_sys_datasource
SELECT * FROM dim_time_zone
*/