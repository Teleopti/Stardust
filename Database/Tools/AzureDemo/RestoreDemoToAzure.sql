SET NOCOUNT ON
EXEC sp_configure 'show advanced options', 1
RECONFIGURE
EXEC sp_configure 'xp_cmdshell', 1
RECONFIGURE
GO
use master
GO

DECLARE @XpCommand nvarchar(4000)
SELECT @XpCommand='"C:\Program Files (x86)\Teleopti\DatabaseInstaller\DemoDatabase\Helpers\RestoreDemo.bat"' 
--restore Demo databases
exec xp_cmdshell @XpCommand
--stop service
exec xp_cmdshell 'net stop TeleoptiETLService'
exec xp_cmdshell 'net stop TeleoptiBrokerService'

--upgrade local databases
SELECT @XpCommand='"C:\Data\RaptorScrum\Root-Azure\Database\DBManager.exe" -S. -DTeleoptiWFMAnalytics_Demo -OTeleoptiAnalytics -E -T'
exec xp_cmdshell @XpCommand

SELECT @XpCommand='"C:\Data\RaptorScrum\Root-Azure\Database\DBManager.exe" -S. -DTeleoptiWFM_Demo -OTeleoptiCCC7 -E -T'
exec xp_cmdshell @XpCommand

--encrypt local pwd
SELECT @XpCommand='"C:\Data\RaptorScrum\Root-Azure\Database\Tools\Encryption\Teleopti.Support.Security.exe" -DS. -DDTeleoptiWFM_Demo -EE'
exec xp_cmdshell @XpCommand

--clean out unwanted data in source db
USE TeleoptiWFMAnalytics_Demo
	-- Delete data from bridge and other tables
	DELETE FROM mart.bridge_queue_workload
	DELETE FROM mart.bridge_skillset_skill
	DELETE FROM mart.permission_report
	DELETE FROM mart.scorecard_kpi
	DELETE FROM dbo.aspnet_Membership
	DELETE FROM dbo.aspnet_Users
	DELETE FROM mart.bridge_acd_login_person
	DELETE FROM mart.bridge_group_page_person
	TRUNCATE TABLE mart.bridge_time_zone
	
    -- Delete data from fact tables
	TRUNCATE TABLE mart.fact_schedule
	TRUNCATE TABLE mart.fact_queue
	TRUNCATE TABLE mart.fact_forecast_workload
	TRUNCATE TABLE mart.fact_schedule_forecast_skill
	TRUNCATE TABLE mart.fact_agent
	TRUNCATE TABLE mart.fact_agent_queue
	TRUNCATE TABLE mart.fact_kpi_targets_team
	TRUNCATE TABLE mart.fact_schedule_deviation
	TRUNCATE TABLE mart.fact_schedule_day_count
	TRUNCATE TABLE mart.fact_schedule_preference

	--this unused time zone makes ETL to load bridge_time_zone over and over again
	delete from mart.dim_time_zone where time_zone_code = 'GMT Standard Time'

--update MsgBroker
update [msg].configuration
set configurationValue = 'teleopticcc-demo.cloudapp.net'
where configurationId = 2

update [msg].address
set Address='teleopticcc-demo.cloudapp.net'
where AddressId= 1

update msg.configuration
set ConfigurationValue='9080'
where ConfigurationId = 1

update msg.Address
set Port='9090'
where AddressId= 1

--Get Agg Data into Analytics
INSERT [TeleoptiAnalytics_Demo].[dbo].[ccc_system_info]
SELECT [id]
      ,[desc]
      ,[int_value]
      ,[varchar_value]
  FROM [TeleoptiCCC7Agg_Demo].[dbo].[ccc_system_info]

INSERT [TeleoptiAnalytics_Demo].[dbo].[acd_type]
SELECT [acd_type_id]
      ,[acd_type_desc]
  FROM [TeleoptiCCC7Agg_Demo].[dbo].[acd_type]

INSERT [TeleoptiAnalytics_Demo].[dbo].[log_object]
SELECT [log_object_id]
      ,[acd_type_id]
      ,[log_object_desc]
      ,[logDB_name]
      ,[intervals_per_day]
      ,[default_service_level_sec]
      ,[default_short_call_treshold]
  FROM [TeleoptiCCC7Agg_Demo].[dbo].[log_object]

INSERT [TeleoptiAnalytics_Demo].[dbo].[agent_info]
SELECT [Agent_id]
      ,[Agent_name]
      ,[is_active]
      ,[log_object_id]
      ,[orig_agent_id]
  FROM [TeleoptiCCC7Agg_Demo].[dbo].[agent_info]

INSERT [TeleoptiAnalytics_Demo].[dbo].[queues]
SELECT [queue]
      ,[orig_desc]
      ,[log_object_id]
      ,[orig_queue_id]
      ,[display_desc]
  FROM [TeleoptiCCC7Agg_Demo].[dbo].[queues]

INSERT [TeleoptiAnalytics_Demo].[dbo].[queue_logg]
SELECT [queue]
      ,[date_from]
      ,[interval]
      ,[offd_direct_call_cnt]
      ,[overflow_in_call_cnt]
      ,[aband_call_cnt]
      ,[overflow_out_call_cnt]
      ,[answ_call_cnt]
      ,[queued_and_answ_call_dur]
      ,[queued_and_aband_call_dur]
      ,[talking_call_dur]
      ,[wrap_up_dur]
      ,[queued_answ_longest_que_dur]
      ,[queued_aband_longest_que_dur]
      ,[avg_avail_member_cnt]
      ,[ans_servicelevel_cnt]
      ,[wait_dur]
      ,[aband_short_call_cnt]
      ,[aband_within_sl_cnt]
  FROM [TeleoptiCCC7Agg_Demo].[dbo].[queue_logg]


INSERT [TeleoptiAnalytics_Demo].[dbo].[agent_logg]
SELECT [queue]
      ,[date_from]
      ,[interval]
      ,[agent_id]
      ,[agent_name]
      ,[avail_dur]
      ,[tot_work_dur]
      ,[talking_call_dur]
      ,[pause_dur]
      ,[wait_dur]
      ,[wrap_up_dur]
      ,[answ_call_cnt]
      ,[direct_out_call_cnt]
      ,[direct_out_call_dur]
      ,[direct_in_call_cnt]
      ,[direct_in_call_dur]
      ,[transfer_out_call_cnt]
      ,[admin_dur]
  FROM [TeleoptiCCC7Agg_Demo].[dbo].[agent_logg]


USE TeleoptiCCC7_Demo
--minimize the number of time zones used
update dbo.person
set DefaultTimeZone = 'W. Europe Standard Time'
where ApplicationLogOnName NOT IN ('datamart','_Super User')

DELETE FROM License

--re-init
DECLARE	@version	AS int
DECLARE @newid		AS uniqueidentifier
DECLARE @PersonGUID AS uniqueidentifier
DECLARE @now				AS datetime

SET @version = 1
SET @newid	= newid()
SELECT top 1 @PersonGUID = Id FROM dbo.person
SET @now				= getdate()

exec sp_executesql N'INSERT INTO dbo.License (Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, XmlString, Id) VALUES (@p0, @p1, @p1, @p2, @p2, @p3, @p4)',N'@p0 int,@p1 uniqueidentifier,@p2 datetime,@p3 nvarchar(4000),@p4 uniqueidentifier',
@p0=1,
@p1=@PersonGUID,
@p2=@now,
@p3=N'<?xml version="1.0" encoding="utf-8"?>
<License>
  <CustomerName>Teleopti RD NOT for production use!</CustomerName>
  <ExpirationDate>2012-01-31T12:00:00</ExpirationDate>
  <ExpirationGracePeriod>P30D</ExpirationGracePeriod>
  <MaxActiveAgents>10000</MaxActiveAgents>
  <MaxActiveAgentsGrace>10</MaxActiveAgentsGrace>
  <TeleoptiCCC>
    <Base>true</Base>
    <AgentSelfService>true</AgentSelfService>
    <Developer>true</Developer>
    <MyTimeWeb>true</MyTimeWeb>
    <ShiftTrades>true</ShiftTrades>
    <AgentScheduleMessenger>true</AgentScheduleMessenger>
    <HolidayPlanner>true</HolidayPlanner>
    <RealtimeAdherence>true</RealtimeAdherence>
    <PerformanceManager>true</PerformanceManager>
    <PayrollIntegration>true</PayrollIntegration>
  </TeleoptiCCC>
  <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
    <SignedInfo>
      <CanonicalizationMethod Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315" />
      <SignatureMethod Algorithm="http://www.w3.org/2000/09/xmldsig#rsa-sha1" />
      <Reference URI="">
        <Transforms>
          <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature" />
        </Transforms>
        <DigestMethod Algorithm="http://www.w3.org/2000/09/xmldsig#sha1" />
        <DigestValue>A/BzeOcJUGI9I3aBiKDmSCCq5cI=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>Y18eBweYSSflfolDYGmBnieKLi4O3lbNjknJ4K8DpbFuRtl9RoQtQkfpkyNgWwN/M8mW3OptcP59OHrsVOq3RcBrOQf1QaORm79LggcL1e7+YKhJFBOMsuRcB7XzVTiiQxD7QcJ65P//ZgkpI3oxzRyIibA2Uss2MF1yYDBtT9o=</SignatureValue>
  </Signature>
</License>',@p4=@newid

/*
--Drop Azure databases
SELECT @XpCommand='SQLCMD -Stcp:s8v4m110k9.database.windows.net -UTeleoptiDemoUser@s8v4m110k9 -PTeleoptiDemoPwd2 -Q"drop database Demo_TeleoptiAnalytics"'
exec xp_cmdshell @XpCommand

SELECT @XpCommand='SQLCMD -Stcp:s8v4m110k9.database.windows.net -UTeleoptiDemoUser@s8v4m110k9 -PTeleoptiDemoPwd2 -Q"drop database Demo_TeleoptiCCC7"'
exec xp_cmdshell @XpCommand

--Create Azure databases
SELECT @XpCommand='"C:\Data\RaptorScrum\Root-Azure\Database\DBManager.exe" -Ss8v4m110k9.database.windows.net -DDemo_TeleoptiAnalytics -OTeleoptiAnalytics -Uteleopti -PT3l30pt1 -C -T -LTeleoptiDemoUser:TeleoptiDemoPwd2'
exec xp_cmdshell @XpCommand

SELECT @XpCommand='"C:\Data\RaptorScrum\Root-Azure\Database\DBManager.exe" -Ss8v4m110k9.database.windows.net -DDemo_TeleoptiCCC7 -OTeleoptiCCC7 -Uteleopti -PT3l30pt1 -C -T -LTeleoptiDemoUser:TeleoptiDemoPwd2'
exec xp_cmdshell @XpCommand
*/
--generate bcp out + in
SELECT @XpCommand='SQLCMD -S. -E -dTeleoptiCCC7_Demo -i"C:\Data\RaptorScrum\Root-Azure\Database\Tools\AzureDemo\DropCircularFKs.sql"'
exec xp_cmdshell @XpCommand

SELECT @XpCommand='SQLCMD -S. -E -dTeleoptiCCC7_Demo -i"C:\Data\RaptorScrum\Root-Azure\Database\Tools\AzureDemo\GenerateBCPStatements.sql" -v DESTDB = "Demo_TeleoptiCCC7"'
exec xp_cmdshell @XpCommand

SELECT @XpCommand='SQLCMD -S. -E -dTeleoptiCCC7_Demo -i"C:\Data\RaptorScrum\Root-Azure\Database\Tools\AzureDemo\CreateCircularFKs.sql"'
exec xp_cmdshell @XpCommand

SELECT @XpCommand='SQLCMD -S. -E -dTeleoptiAnalytics_Demo -i"C:\Data\RaptorScrum\Root-Azure\Database\Tools\AzureDemo\GenerateBCPStatements.sql" -v DESTDB = "Demo_TeleoptiAnalytics"'
exec xp_cmdshell @XpCommand

--execute bcp out
SELECT @XpCommand='C:\bcp\TeleoptiAnalytics_Demo\Out.bat'
exec xp_cmdshell @XpCommand

SELECT @XpCommand='C:\bcp\TeleoptiCCC7_Demo\Out.bat'
exec xp_cmdshell @XpCommand

--Clear out all data from Azure
SELECT @XpCommand='SQLCMD -Stcp:s8v4m110k9.database.windows.net -UTeleoptiDemoUser@s8v4m110k9 -PTeleoptiDemoPwd2 -dDemo_TeleoptiCCC7 -i"C:\Data\RaptorScrum\Root-Azure\Database\Tools\AzureDemo\DropCircularFKs.sql"'
exec xp_cmdshell @XpCommand

SELECT @XpCommand='SQLCMD -Stcp:s8v4m110k9.database.windows.net -UTeleoptiDemoUser@s8v4m110k9 -PTeleoptiDemoPwd2 -dDemo_TeleoptiCCC7 -i"C:\Data\RaptorScrum\Root-Azure\Database\Tools\AzureDemo\DeleteAllData.sql"'
exec xp_cmdshell @XpCommand

SELECT @XpCommand='SQLCMD -Stcp:s8v4m110k9.database.windows.net -UTeleoptiDemoUser@s8v4m110k9 -PTeleoptiDemoPwd2 -dDemo_TeleoptiCCC7 -i"C:\Data\RaptorScrum\Root-Azure\Database\Tools\AzureDemo\CreateCircularFKs.sql"'
exec xp_cmdshell @XpCommand

SELECT @XpCommand='SQLCMD -Stcp:s8v4m110k9.database.windows.net -UTeleoptiDemoUser@s8v4m110k9 -PTeleoptiDemoPwd2 -dDemo_TeleoptiAnalytics -i"C:\Data\RaptorScrum\Root-Azure\Database\Tools\AzureDemo\DeleteAllData.sql"'
exec xp_cmdshell @XpCommand

--execute bcp in
SELECT @XpCommand='C:\bcp\TeleoptiAnalytics_Demo\In.bat'
exec xp_cmdshell @XpCommand

SELECT @XpCommand='C:\bcp\TeleoptiCCC7_Demo\In.bat'
exec xp_cmdshell @XpCommand

--Fix views
SELECT @XpCommand='SQLCMD -Stcp:s8v4m110k9.database.windows.net -UTeleoptiDemoUser@s8v4m110k9 -PTeleoptiDemoPwd2 -dDemo_TeleoptiAnalytics -Q"update mart.sys_crossdatabaseview_target set confirmed = 1"'
SELECT @XpCommand='SQLCMD -Stcp:s8v4m110k9.database.windows.net -UTeleoptiDemoUser@s8v4m110k9 -PTeleoptiDemoPwd2 -dDemo_TeleoptiAnalytics -Q"exec mart.sys_crossDatabaseView_load"'

--Run ETL
--b) intraday today
--b) nightly today
--b) nightly 2009-01-01 => 2009-04-01