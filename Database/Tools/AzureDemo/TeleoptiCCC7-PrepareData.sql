update dbo.person
set DefaultTimeZone = 'W. Europe Standard Time'
where ApplicationLogOnName NOT IN ('datamart','_Super User')

DELETE FROM License

update dbo.person
set WindowsLogOnName='',DomainName=''

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