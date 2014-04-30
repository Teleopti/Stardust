--Note: This script is executed via SQLCMD using variables:
--please uncomment to execute via SSMS.
--:SETVAR TIMEZONE "GMT Standard Time"
--:SETVAR USERDOMAIN "devDomain"
--:SETVAR USERNAME "devUsername"

DECLARE @FreemiumGUID		AS uniqueidentifier
DECLARE @newid				AS uniqueidentifier
DECLARE @FreemiumAppUser	AS nvarchar(50)
DECLARE @FreemiumPwd		AS nvarchar(50)
DECLARE @FreemiumEmail		AS nvarchar(50)
DECLARE @FreemiumFirstName	AS nvarchar(25)
DECLARE @FreemiumLastName	AS nvarchar(25)
DECLARE @now				AS datetime
DECLARE @UICulture			AS int
DECLARE @Culture			AS int
DECLARE	@version			AS int

SET @now				= getdate()
SET @FreemiumGUID		= '8C4F2F48-CEE6-43A2-852B-575551CBE810'
SET	@FreemiumAppUser	= N'teleopti'
SET @FreemiumPwd		= N'1234'
SET @FreemiumEmail		= N''
SET @FreemiumFirstName	= N'Teleopti'
SET @FreemiumLastName	= N'Forecasts'
SET @UICulture			= NULL --user will get his local settings
SET @Culture			= NULL --user will get his local settings

--Check if passwords have already encrypted, if so use the encrypted value for "1234"
if exists (select 1 from dbo.databaseVersion where BuildNumber = -290)
SET @FreemiumPwd = '###BAA9A26801DA3958F2DD61A6177F8D39D0CB443E###'


----------------
--Name: DavidJ
--Date: 2011-10-24
--Desc: #16663 - Try fix Demo Queue in Freemium
----------------
update queuesource
set QueueMartId			= 7,
	LogObjectName		= 'Demo Data',
	Name				= 'Brand 1 Long Term',
	[Description]		= 'Brand 1 Long Term'	
where id = '2C49D96A-7166-4014-8B5C-9BA4010D7632'

update mart.dim_queue
set queue_name			= 'Brand 1 Long Term',
	queue_Description	= 'Brand 1 Long Term'
where queue_id = 7


SELECT @version = MAX(version)
FROM person
WHERE Id = @FreemiumGUID

----------------
--Name: DavidJ
--Date: 2013-02-06
--Desc: #22260 - Fix Person re-factor in Freemium deploy
----------------
UPDATE dbo.Person
SET	Version					= @version+1,
	UpdatedBy				= @FreemiumGUID,
	UpdatedOn				= @now,
	Email					= @FreemiumEmail,
	FirstName				= @FreemiumFirstName,
	LastName				= @FreemiumLastName,
	DefaultTimeZone			= '$(TIMEZONE)',
	Culture					= @Culture,
	UiCulture				= @UICulture
WHERE Id = @FreemiumGUID
AND Version = @version

UPDATE dbo.ApplicationAuthenticationInfo
SET ApplicationLogOnName	= @FreemiumAppUser,
	Password				= @FreemiumPwd
FROM dbo.Person p
WHERE p.Id = dbo.ApplicationAuthenticationInfo.person
AND p.Id = @FreemiumGUID

--add if not already exist
INSERT INTO dbo.AuthenticationInfo (person,[Identity])
SELECT p.Id,'$(USERDOMAIN)\$(USERNAME)'
FROM dbo.person p
WHERE
   NOT EXISTS (SELECT * FROM dbo.AuthenticationInfo win
              WHERE win.person = p.Id)
AND p.Id = @FreemiumGUID

--if exist, update to current Windows user
UPDATE dbo.AuthenticationInfo
SET 	[Identity] = '$(USERDOMAIN)\$(USERNAME)'
FROM dbo.Person p
WHERE p.Id = dbo.AuthenticationInfo.person
AND p.Id = @FreemiumGUID

--Add temporary license
DELETE FROM License

--re-init
SET @version = 1
SET @newid	= newid()

exec sp_executesql N'INSERT INTO dbo.License (Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, XmlString, Id) VALUES (@p0, @p1, @p1, @p2, @p2, @p3, @p4)',N'@p0 int,@p1 uniqueidentifier,@p2 datetime,@p3 nvarchar(4000),@p4 uniqueidentifier',
@p0=1,
@p1=@FreemiumGUID,
@p2=@now,
@p3=N'<?xml version="1.0" encoding="utf-8"?>
<License>
  <CustomerName>Freemium license</CustomerName>
  <ExpirationDate>2014-01-22T08:44:06</ExpirationDate>
  <ExpirationGracePeriod>P30D</ExpirationGracePeriod>
  <MaxActiveAgents>1</MaxActiveAgents>
  <MaxActiveAgentsGrace>10</MaxActiveAgentsGrace>
  <TeleoptiCCCFreemium>
    <Forecasts>true</Forecasts>
  </TeleoptiCCCFreemium>
  <Agreement>Teleopti.Ccc.Infrastructure.Licensing.Agreements.TeleoptiLic_En_Sw_Forecasts.txt</Agreement>
  <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
    <SignedInfo>
      <CanonicalizationMethod Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315" />
      <SignatureMethod Algorithm="http://www.w3.org/2000/09/xmldsig#rsa-sha1" />
      <Reference URI="">
        <Transforms>
          <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature" />
        </Transforms>
        <DigestMethod Algorithm="http://www.w3.org/2000/09/xmldsig#sha1" />
        <DigestValue>kVWutqFRt0NBi/SDu6G5d+YcITw=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>v/j1xu/Dl7vTHF4Q+8xkd3acJgz15u5CcFV/NXKGSYTTwXzjPDetHYXKEYJd3T47kguvRWvJQs13fb/gBK4yMtLSY+B4XX0F+s9QaoxFGbex/mcpG4Db/t+jdaXv2kpqeYbWeO21P3e4SOaSKa5eV//HJhU5af7labDos+feG9A=</SignatureValue>
  </Signature>
</License>',@p4=@newid

