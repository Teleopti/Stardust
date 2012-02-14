----------------  
--Name: Mattias Engblom
--Date: 2012-01-11  
--Desc: Move MyTimeWeb/TeamSchedule from UnderConstruction
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
SELECT @ForeignId = '0072' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'TeamSchedule' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxTeamSchedule' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', '_Super User', 0, 1, 1) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO
----------------  
--Name: David J
--Date: 2012-01-10
--Desc: Adding index to support FindPerson  
----------------  
--shipped as special function (NO MERGE)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ReadModel].[FindPerson]') AND name = N'IX_FindPerson_PersonId')
CREATE NONCLUSTERED INDEX [IX_FindPerson_PersonId]
ON [ReadModel].[FindPerson] ([PersonId])
INCLUDE ([FirstName],[LastName],[EmploymentNumber],[Note],[TerminalDate],[TeamId],[SiteId],[BusinessUnitId])
GO

----------------  
--Name: Robin K
--Date: 2012-01-18
--Desc: Adding contract time to read model for layers 
----------------  
--shipped as previous trunk (NO MERGE)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[ReadModel].[ScheduleProjectionReadOnly]') AND name=N'ContractTime')
BEGIN
TRUNCATE TABLE ReadModel.ScheduleProjectionReadOnly 
ALTER TABLE ReadModel.ScheduleProjectionReadOnly ADD
	ContractTime bigint NOT NULL CONSTRAINT DF_ScheduleProjectionReadOnly_ContractTime DEFAULT 0
END
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (350,'7.1.350') 
