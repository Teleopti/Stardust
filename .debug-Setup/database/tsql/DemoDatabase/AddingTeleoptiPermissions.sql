/*
:SETVAR TELEOPTICCC TeleoptiCCC7_Demo
:SETVAR TELEOPTIANALYTICS TeleoptiAnalytics_Demo
:SETVAR TELEOPTIAGG TeleoptiCCC7Agg_Demo
*/

SET NOCOUNT ON
----------------
--Adding a agent persmissions
----------------
DECLARE @AgentRole uniqueidentifier
SET @AgentRole = 'E7F360D3-C4B6-41FC-9B2D-9B5E015AAE64' --Id in DemoSales

DECLARE @FunctionCode TABLE (FunctionCode nvarchar(50))
INSERT INTO @FunctionCode
--Add new FunctionCode's here
SELECT 'ViewBadge'
UNION ALL
SELECT 'QueueMetrics'
UNION ALL
SELECT 'ViewPersonalAccount'

;WITH ClassHierarchy_CTE (ClassID, ClassID_Join, Level)
AS
(
SELECT
	Id,
	Parent,
	0
FROM $(TELEOPTICCC).dbo.ApplicationFunction af
INNER JOIN @FunctionCode fc
	ON fc.FunctionCode = af.FunctionCode collate database_default
UNION ALL
SELECT
    cte.ClassID,
    h.Parent,
    Level + 1
FROM $(TELEOPTICCC).dbo.ApplicationFunction AS h
INNER JOIN ClassHierarchy_CTE as cte
	ON h.Id = cte.ClassID_Join
)
--Insert parents needed by added function
INSERT INTO $(TELEOPTICCC).dbo.ApplicationFunctionInRole
SELECT
    @AgentRole, af.Id, getdate()
FROM ClassHierarchy_CTE a
INNER JOIN $(TELEOPTICCC).dbo.ApplicationFunction af
	ON a.ClassID_Join = af.Id
WHERE a.ClassID_Join IS NOT NULL
AND NOT EXISTS (SELECT * FROM $(TELEOPTICCC).dbo.ApplicationFunctionInRole a 
						WHERE a.ApplicationRole = @AgentRole
						AND a.ApplicationFunction = af.Id)
ORDER BY
    a.ClassID,
    a.Level

--add add the acutal function it self
INSERT INTO $(TELEOPTICCC).dbo.ApplicationFunctionInRole
select @AgentRole, af.Id, getdate()
FROM $(TELEOPTICCC).dbo.ApplicationFunction af
INNER JOIN @FunctionCode fc
	ON fc.FunctionCode = af.FunctionCode collate database_default
WHERE NOT EXISTS (SELECT * FROM $(TELEOPTICCC).dbo.ApplicationFunctionInRole a 
						WHERE a.ApplicationRole = @AgentRole
						AND a.ApplicationFunction = af.Id)

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
SET
	UserName=@WinDomain+'\'+@WinUser,
	LoweredUserName=@WinDomain+'\'+@WinUser
WHERE userid=@userid

update $(TELEOPTIANALYTICS).dbo.aspnet_Users
set LoweredUserName = lower(LoweredUserName)

--Add permissions on all reports
truncate table $(TELEOPTIANALYTICS).mart.permission_report

insert into $(TELEOPTIANALYTICS).mart.permission_report
select @userid,t.team_id,0,bu.business_unit_id,1,getdate(),Id
from $(TELEOPTIANALYTICS).mart.report
inner join $(TELEOPTIANALYTICS).mart.dim_team t
            on 1=1
            and t.team_id > -1
inner join $(TELEOPTIANALYTICS).mart.dim_business_unit bu
            on 1=1
            and bu.business_unit_id > -1
GO