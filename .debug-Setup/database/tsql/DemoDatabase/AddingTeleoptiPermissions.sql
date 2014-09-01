/*
:SETVAR TELEOPTICCC TeleoptiCCC7_Demo
:SETVAR TELEOPTIANALYTICS TeleoptiAnalytics_Demo
:SETVAR TELEOPTIAGG TeleoptiCCC7Agg_Demo
*/

SET NOCOUNT ON
----------------
--Adding a cool user
----------------
DECLARE @WinUser varchar(50)
DECLARE @WinDomain varchar(50)
DECLARE @delim varchar(1)
DECLARE @commaindex int
DECLARE @csv varchar(100)
DECLARE @userid uniqueidentifier

SET @userid = '10957ad5-5489-48e0-959a-9b5e015b2b5c' --Id in SalesDemo

--if not demo, find any other cool user
IF NOT EXISTS (SELECT 1 FROM $(TELEOPTICCC).dbo.Person WHERE Id=@userid)
BEGIN
	select TOP 1 @userid = p.Id
	from $(TELEOPTICCC).dbo.ApplicationFunction af
	inner join $(TELEOPTICCC).dbo.ApplicationFunctionInRole afir
		on af.Id = afir.ApplicationFunction
	inner join $(TELEOPTICCC).dbo.ApplicationRole ar
		on ar.Id = afir.ApplicationRole
	inner join $(TELEOPTICCC).dbo.PersonInApplicationRole pnar
		on pnar.ApplicationRole = ar.id
	inner join $(TELEOPTICCC).dbo.Person p
		on p.Id = pnar.Person
	left outer join $(TELEOPTICCC).dbo.PersonPeriod pp
		on p.Id = pp.Parent
	where af.FunctionDescription = 'xxAll'
	and ar.DescriptionText = 'xxSuperRole'
	and p.IsDeleted = 0
	and p.BuiltIn = 0
	ORDER BY pp.Parent,p.UpdatedOn desc --perferable:oldest, "non"-agent
END


SELECT @WinDomain = '$(USERDOMAIN)'
SELECT @WinUser = '$(USERNAME)'

--delete all Windows domains as they stall IIS -> AD-lookup in TeleoptiPM
DELETE FROM $(TELEOPTICCC).dbo.AuthenticationInfo

--insert current user and connect to @userid
declare @identity nvarchar(100)
select @identity=@WinDomain + N'\' + @WinUser
INSERT INTO $(TELEOPTICCC).dbo.AuthenticationInfo
SELECT @userid,@identity

--Add currect user to IIS-users: update aspnet_users
UPDATE $(TELEOPTIANALYTICS).dbo.aspnet_Users
SET UserName=system_user,LoweredUserName=system_user
WHERE userid=@userid

--Add permissions on all reports
