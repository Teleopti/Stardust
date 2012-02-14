----------------  
--Name: Mattias E
--Date: 2011-10-04  
--Desc: Add the following new application function> MyTimeWeb/StandardPreferences
----------------  
SET NOCOUNT ON
	
--declarations
DECLARE @SuperUserId as uniqueidentifier
DECLARE @FunctionId as uniqueidentifier
DECLARE @ParentFunctionId as uniqueidentifier
DECLARE @ForeignId as varchar(255)
DECLARE @ParentForeignId as varchar(255)
DECLARE @FunctionCode as varchar(255)
DECLARE @FunctionDescription as varchar(255)
DECLARE @ParentId as uniqueidentifier

--insert to super user if not exist
SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

-- check for the existence of super user role
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', '_Super User', 0, 1) 

--get parent level
SELECT @ParentForeignId = '0065'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0068' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'StandardPreferences' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxModifyShiftCategoryPreferences' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

---------------
--Name: David J
--Date: 2011-10-06
--Desc: Create a special schema for customer specific tables. e.g. PayRoll stuff
---------------
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'CustomTables')
EXEC sp_executesql N'CREATE SCHEMA [CustomTables] AUTHORIZATION [dbo]'
GO
----------------  
--Name: DavidJ
----------------  
--Name: Xianwei Shen
--Date: 2011-09-26  
--Desc: Add a new Application Function
--ViewCustomTeamSchedule

--declarations
DECLARE @SuperUserId as uniqueidentifier
DECLARE @FunctionId as uniqueidentifier
DECLARE @ParentFunctionId as uniqueidentifier
DECLARE @ForeignId as varchar(255)
DECLARE @ParentForeignId as varchar(255)
DECLARE @FunctionCode as varchar(255)
DECLARE @FunctionDescription as varchar(255)
DECLARE @ParentId as uniqueidentifier

--insert to super user if not exist
SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

-- check for the existence of super user role
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', '_Super User', 0, 1) 

--get parent level
SELECT @ParentForeignId = '0019'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0069' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ViewCustomTeamSchedule' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxViewCustomTeamSchedule' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
GO

----------------  
--Name: RobinK
--Date: 2011-10-06
--Desc: Adding this procedure here as we need to run it as part of the release script for the initial update of the read model.
----------------  
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateGroupingReadModel]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateGroupingReadModel]
GO

-- =============================================
-- Author:		RobinK
-- Create date: 2011-09-26
-- Description:	Updates the read model for groupings
-- Change:		
-- =============================================
CREATE TABLE [dbo].[GroupingReadOnly](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [uniqueidentifier] NOT NULL,
	[StartDate] [smalldatetime] NOT NULL,
	[TeamId] [uniqueidentifier] NULL,
	[SiteId] [uniqueidentifier] NULL,
	[BusinessUnitId] [uniqueidentifier] NULL,
	[GroupId] [uniqueidentifier] NULL,
	[GroupName] [nvarchar](50) NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[PageId] [uniqueidentifier] NULL,
	[PageName] [nvarchar](50) NULL,
	[EmploymentNumber] [nvarchar](50) NULL,
	[EndDate] [smalldatetime] NULL,
	[LeavingDate] [smalldatetime] NULL)
ALTER TABLE [dbo].[GroupingReadOnly] ADD CONSTRAINT [PK_GroupingReadOnly]
PRIMARY KEY CLUSTERED 
(
			  [Id] ASC
)
GO

----------------  
--Name: Anders F
--Date: 2011-10-27  
--Desc: Column need to be longer as we have concatenated sites and teams in it, and recursive groups within groups...
----------------
alter table dbo.GroupingReadOnly alter column GroupName nvarchar(200)
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateGroupingReadModel]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateGroupingReadModel]
GO

CREATE PROCEDURE dbo.UpdateGroupingReadModel
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION
    
    declare @mainId uniqueidentifier
	set @mainId='6CE00B41-0722-4B36-91DD-0A3B63C545CF'
	
	declare @contractId uniqueidentifier
	set @contractId='0CE00B41-0722-4B36-91DD-0A3B63C545CF'

	declare @contractScheduleId uniqueidentifier
	set @contractScheduleId='1CE00B41-0722-4B36-91DD-0A3B63C545CF'

	declare @partTimePercentageId uniqueidentifier
	set @partTimePercentageId='2CE00B41-0722-4B36-91DD-0A3B63C545CF'

	declare @ruleSetBagId uniqueidentifier
	set @ruleSetBagId='3CE00B41-0722-4B36-91DD-0A3B63C545CF'

	declare @skillId uniqueidentifier
	set @skillId='4CE00B41-0722-4B36-91DD-0A3B63C545CF'

	--declare @noteId uniqueidentifier
	--set @noteId='5CE00B41-0722-4B36-91DD-0A3B63C545CF'

	DELETE FROM groupingreadonly
	
	--Insert people from business hierarchy
	INSERT INTO groupingreadonly (personid,startdate,teamid,siteid,businessunitid,groupid,groupname,firstname,lastname,pageid,pagename,employmentnumber,enddate,leavingdate) select p.id,isnull(pp.startdate,'1900-01-01') as startdate,t.id teamid,s.id siteid,s.businessunit as businessunitid,t.id as groupid,s.name +'/'+t.name as groupname,p.firstname,p.lastname,@mainId as pageid,'xxMain' as pagename,p.employmentnumber,(select top 1 dateadd(d,-1,startdate) from personperiod where parent=p.id and startdate>pp.startdate order by startdate asc) as enddate,p.terminaldate as leavingdate from site s inner join team t on t.site=s.id inner join personperiod pp on pp.team=t.id inner join person p on p.id=pp.parent where p.isdeleted=0 and t.isdeleted=0 and s.isdeleted=0
	
	--Insert people from contract
	INSERT INTO groupingreadonly (personid,startdate,teamid,siteid,businessunitid,groupid,groupname,firstname,lastname,pageid,pagename,employmentnumber,enddate,leavingdate) select p.id,isnull(pp.startdate,'1900-01-01') as startdate,t.id teamid,t.site siteid,c.businessunit as businessunitid,c.id as groupid,c.name as groupname,p.firstname,p.lastname,@contractId as pageid,'xxContract' as pagename,p.employmentnumber,(select top 1 dateadd(d,-1,startdate) from personperiod where parent=p.id and startdate>pp.startdate order by startdate asc) as enddate,p.terminaldate as leavingdate from team t inner join personperiod pp on pp.team=t.id inner join contract c on pp.contract=c.id inner join person p on p.id=pp.parent where p.isdeleted=0 and t.isdeleted=0 and c.isdeleted=0
	
	--Insert people from part time percentage
	INSERT INTO groupingreadonly (personid,startdate,teamid,siteid,businessunitid,groupid,groupname,firstname,lastname,pageid,pagename,employmentnumber,enddate,leavingdate) select p.id,isnull(pp.startdate,'1900-01-01') as startdate,t.id teamid,t.site siteid,c.businessunit as businessunitid,c.id as groupid,c.name as groupname,p.firstname,p.lastname,@partTimePercentageId as pageid,'xxPartTimePercentage' as pagename,p.employmentnumber,(select top 1 dateadd(d,-1,startdate) from personperiod where parent=p.id and startdate>pp.startdate order by startdate asc) as enddate,p.terminaldate as leavingdate from team t inner join personperiod pp on pp.team=t.id inner join parttimepercentage c on pp.parttimepercentage=c.id inner join person p on p.id=pp.parent where p.isdeleted=0 and t.isdeleted=0 and c.isdeleted=0
	
	--Insert people from contract schedule
	INSERT INTO groupingreadonly (personid,startdate,teamid,siteid,businessunitid,groupid,groupname,firstname,lastname,pageid,pagename,employmentnumber,enddate,leavingdate) select p.id,isnull(pp.startdate,'1900-01-01') as startdate,t.id teamid,t.site siteid,c.businessunit as businessunitid,c.id as groupid,c.name as groupname,p.firstname,p.lastname,@contractScheduleId as pageid,'xxContractSchedule' as pagename,p.employmentnumber,(select top 1 dateadd(d,-1,startdate) from personperiod where parent=p.id and startdate>pp.startdate order by startdate asc) as enddate,p.terminaldate as leavingdate from team t inner join personperiod pp on pp.team=t.id inner join contractschedule c on pp.contractschedule=c.id inner join person p on p.id=pp.parent where p.isdeleted=0 and t.isdeleted=0 and c.isdeleted=0
	
	--Insert people from rule set bag
	INSERT INTO groupingreadonly (personid,startdate,teamid,siteid,businessunitid,groupid,groupname,firstname,lastname,pageid,pagename,employmentnumber,enddate,leavingdate) select p.id,isnull(pp.startdate,'1900-01-01') as startdate,t.id teamid,t.site siteid,c.businessunit as businessunitid,c.id as groupid,c.name as groupname,p.firstname,p.lastname,@ruleSetBagId as pageid,'xxRuleSetBag' as pagename,p.employmentnumber,(select top 1 dateadd(d,-1,startdate) from personperiod where parent=p.id and startdate>pp.startdate order by startdate asc) as enddate,p.terminaldate as leavingdate from team t inner join personperiod pp on pp.team=t.id inner join rulesetbag c on pp.rulesetbag=c.id inner join person p on p.id=pp.parent where p.isdeleted=0 and t.isdeleted=0 and c.isdeleted=0
	
	--Insert people from skill
	INSERT INTO groupingreadonly (personid,startdate,teamid,siteid,businessunitid,groupid,groupname,firstname,lastname,pageid,pagename,employmentnumber,enddate,leavingdate) select p.id,isnull(pp.startdate,'1900-01-01') as startdate,t.id teamid,t.site siteid,skill.businessunit as businessunitid,skill.id as groupid,skill.name as groupname,p.firstname,p.lastname,@skillId as pageid,'xxSkill' as pagename,p.employmentnumber,(select top 1 dateadd(d,-1,startdate) from personperiod where parent=p.id and startdate>pp.startdate order by startdate asc) as enddate,p.terminaldate as leavingdate from team t inner join personperiod pp on pp.team=t.id inner join personskill c on pp.id=c.parent and c.active=1 inner join skill on skill.id=c.skill inner join person p on p.id=pp.parent where p.isdeleted=0 and t.isdeleted=0 and skill.isdeleted=0
	
	CREATE TABLE #tempresult
	(
		groupid uniqueidentifier not null,
		queryid uniqueidentifier not null,
		name nvarchar(200) null,
		pagename nvarchar(50) null,
		pageid uniqueidentifier not null,
		businessunit uniqueidentifier not null,
		parent uniqueidentifier null
	);

	--Flatten out the hierarchy of custom groups
	WITH grouping1 (GroupId,queryid,name,pagename,pageid,businessunit,parent)
	AS
	(
	SELECT pgb.id,pgb.id,CAST(pgb.name AS NVARCHAR(200)),gp.name pagename,gp.id pageid,gp.businessunit,CAST(NULL AS UNIQUEIDENTIFIER) FROM persongroupbase pgb INNER JOIN rootpersongroup rpg ON rpg.persongroupbase=pgb.id INNER JOIN grouppage gp ON gp.id=rpg.parent where gp.isdeleted=0
	UNION ALL
	SELECT pgb.id,pgb.id,CAST(g1.name+'\'+pgb.name AS NVARCHAR(200)),g1.pagename,g1.pageid,g1.businessunit,cpg.parent FROM persongroupbase pgb INNER JOIN childpersongroup cpg ON cpg.persongroupbase=pgb.id INNER JOIN grouping1 g1 ON g1.groupid=cpg.parent
	)

	INSERT INTO #tempresult SELECT * FROM grouping1;

	--Enable people to pop up both in MainGroup\SubGroup1 and MainGroup
	WITH grouping2 (groupId,queryid,name,pagename,pageid,businessunit,parent)
	AS
	(
	SELECT t2.groupid,t1.groupid,t2.name,t1.pagename,t1.pageid,t1.businessunit,t1.parent FROM #tempresult t1 INNER JOIN #tempresult t2 ON t2.queryid=t1.parent WHERE t1.parent IS NOT NULL
	UNION ALL
	SELECT p.groupid,c.groupid,p.name,c.pagename,c.pageid,c.businessunit,c.parent FROM #tempresult c INNER JOIN grouping2 p ON p.queryid=c.parent
	)

	INSERT INTO #tempresult SELECT * FROM grouping2
	
	INSERT INTO groupingreadonly (pageid,pagename,businessunitid,groupid,groupname,firstname,lastname,employmentnumber,personid,teamid,siteid,startdate,enddate,leavingdate) SELECT pageid,pagename,businessunit,groupid,tr.name groupname,p.firstname,p.lastname,p.employmentnumber,p.id personid,pp.team teamid,t.site siteid,isnull(pp.startdate,'1900-01-01') startdate,(SELECT TOP 1 dateadd(d,-1,startdate) FROM personperiod WHERE parent=p.id AND startdate>pp.startdate ORDER BY startdate ASC) as enddate,p.terminaldate leavingdate FROM #tempresult tr INNER JOIN persongroup pg ON pg.persongroup=tr.queryid INNER JOIN person p ON p.id=pg.person LEFT JOIN personperiod pp ON pp.parent=p.id LEFT JOIN team t ON t.id=pp.team WHERE p.isdeleted=0

	COMMIT TRANSACTION
END
GO

EXEC dbo.UpdateGroupingReadModel
GO

----------------  
--Name: AndersF
--Date: 2011-10-19
--Desc: Blanks blocks RTA updates
----------------  
update ExternalLogOn set AcdLogOnOriginalId = LTRIM(RTRIM(AcdLogOnOriginalId))
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (337,'7.1.337') 
