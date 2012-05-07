--Empty the table, must be filled using the security tool!
DELETE FROM [dbo].[LicenseStatus]
----------------  
--Name: RobinK
--Date: 2012-04-11
--Desc: #18919 - Require unique items in table to avoid future problems!
----------------  
BEGIN
CREATE TABLE #sgaa
(
	Activity uniqueidentifier null,
	StateGroup uniqueidentifier null,
	BusinessUnit uniqueidentifier null
)

INSERT INTO #sgaa (activity,stategroup,businessunit)
SELECT sgaa1.[Activity]
      ,sgaa1.[StateGroup]
      ,sgaa1.[BusinessUnit]
  FROM [dbo].[StateGroupActivityAlarm] sgaa1
  group by sgaa1.[Activity]
      ,sgaa1.[StateGroup]
      ,sgaa1.[BusinessUnit]
      having count(sgaa1.businessunit) > 1

DELETE sgaa1 
FROM [dbo].[StateGroupActivityAlarm] sgaa1 
inner join #sgaa sgaa2	on (sgaa2.activity=sgaa1.activity
							or (sgaa2.activity is null and sgaa1.activity is null)) 
						and (sgaa2.stategroup  = sgaa1.stategroup 
							or (sgaa2.stategroup is null and sgaa1.stategroup is null))
						and sgaa2.businessunit=sgaa1.businessunit

DROP TABLE #sgaa

END
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[StateGroupActivityAlarm]') AND name = N'UQ_StateGroupActivityAlarm')
ALTER TABLE dbo.StateGroupActivityAlarm ADD CONSTRAINT
	UQ_StateGroupActivityAlarm UNIQUE NONCLUSTERED 
	(
	StateGroup,
	Activity,
	BusinessUnit
	)
GO


----------------  
--Name: Xianwei Shen
--Date: 2012-04-16 
--Desc: Add the following new application function> Forecaster/ImportForecastFromFile
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
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1) 

--get parent level
SELECT @ParentForeignId = '0003'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0076' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ImportForecastFromFile' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxImportForecastFromFile' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Xianwei Shen
--Date: 2012-04-25  
--Desc: Add the following new application function> Budgets/RequestAllowances
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
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1) 

--get parent level
SELECT @ParentForeignId = '0050'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0075' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'RequestAllowances' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxRequestAllowances' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (360,'7.1.360') 
