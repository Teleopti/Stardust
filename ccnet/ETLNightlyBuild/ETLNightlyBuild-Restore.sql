USE master
-------
--First part of the script
-------
--Re-create dbs if they are missing
IF NOT EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(TeleoptiAnalytics)')
CREATE DATABASE [$(TeleoptiAnalytics)]
IF NOT EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(TeleoptiCCC7)')
CREATE DATABASE [$(TeleoptiCCC7)]
IF NOT EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(TeleoptiCCCAgg)')
CREATE DATABASE [$(TeleoptiCCCAgg)]
go

-------
--then
-------

USE master
DECLARE @SQLString VARCHAR(5000)

DECLARE @DataDir nvarchar(260)
EXEC	master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',N'Software\Microsoft\MSSQLServer\Setup',N'SQLDataRoot', @DataDir output, 'no_output'
SELECT	@DataDir = @DataDir + N'\Data'

--Restore Matrix
PRINT 'Restoring $(TeleoptiAnalytics)'
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(TeleoptiAnalytics)')
ALTER DATABASE [$(TeleoptiAnalytics)] SET SINGLE_USER WITH ROLLBACK IMMEDIATE

SELECT @SQLString = 'RESTORE DATABASE [$(TeleoptiAnalytics)]
FROM  DISK = N''$(BakDir)\TeleoptiAnalytics_Demo.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiAnalytics_Primary'' TO N''' + @DataDir +'\$(TeleoptiAnalytics)_Primary.mdf'',
MOVE N''TeleoptiAnalytics_Log'' TO N''' + @DataDir +'\$(TeleoptiAnalytics)_Log.ldf'',
MOVE N''TeleoptiAnalytics_Stage'' TO N''' + @DataDir +'\$(TeleoptiAnalytics)_Stage.ndf'',
MOVE N''TeleoptiAnalytics_Mart'' TO N''' + @DataDir +'\$(TeleoptiAnalytics)_Mart.ndf'',
MOVE N''TeleoptiAnalytics_Msg'' TO N''' + @DataDir +'\$(TeleoptiAnalytics)_Msg.ndf'',
MOVE N''TeleoptiAnalytics_Rta'' TO N''' + @DataDir +'\$(TeleoptiAnalytics)_Rta.ndf'',
NOUNLOAD,
REPLACE,
STATS = 10'

EXEC (@SQLString)

--Restore Raptor
PRINT 'Restoring $(TeleoptiCCC7)'
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(TeleoptiCCC7)')
ALTER DATABASE [$(TeleoptiCCC7)] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE

SELECT @SQLString = 'RESTORE DATABASE [$(TeleoptiCCC7)]
FROM  DISK = N''$(BakDir)\TeleoptiCCC7_Demo.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiCCC7_Data'' TO N''' + @DataDir +'\$(TeleoptiCCC7).mdf'',
MOVE N''TeleoptiCCC7_Log'' TO N''' + @DataDir +'\$(TeleoptiCCC7).ldf'',
NOUNLOAD,
REPLACE,
STATS = 10'

EXEC (@SQLString)

--Restore Agg
PRINT 'Restoring $(TeleoptiCCCAgg)'
IF EXISTS (SELECT Name FROM sys.databases WHERE NAME = '$(TeleoptiCCCAgg)')
ALTER DATABASE [$(TeleoptiCCCAgg)] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE

SELECT @SQLString = 'RESTORE DATABASE [$(TeleoptiCCCAgg)]
FROM  DISK = N''$(BakDir)\TeleoptiCCC7Agg_Demo.BAK''
WITH  FILE = 1,
MOVE N''TeleoptiCCCAgg_Data'' TO N''' + @DataDir +'\$(TeleoptiCCCAgg).mdf'',
MOVE N''TeleoptiCCCAgg_Log'' TO N''' + @DataDir +'\$(TeleoptiCCCAgg).ldf'',
NOUNLOAD,
REPLACE,
STATS = 10'

EXEC (@SQLString)

--Update name on Agg
USE [$(TeleoptiAnalytics)]
GO

EXEC mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', '$(TeleoptiCCCAgg)'
EXEC mart.sys_crossDatabaseView_load
GO

USE [$(TeleoptiCCC7)]
--Add temporary license
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
GO
