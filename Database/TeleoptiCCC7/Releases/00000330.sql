/* 
Trunk initiated: 
2011-06-09 
15:11
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Jonas
--Date: 2011-06-10  
--Desc: Remove timezone from site since it is not used anywhere.  
----------------  
ALTER TABLE dbo.Site
	DROP COLUMN TimeZone
GO

----------------  
--Name: Jonas
--Date: 2011-06-13  
--Desc: Add the following new application function MyTimeWeb
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
SELECT @ParentForeignId = '0001'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0065' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'MyTimeWeb' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxMyTimeWeb' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: David
--Date: 2011-06-18
--Desc: _SuperUser: if windowslogonname is null it bugged the code (Something on Robin found)
----------------  
update person set windowslogonname=''
where id='3F0886AB-7B25-4E95-856A-0D726EDC2A67'
and windowslogonname is null

----------------  
--Name: Ola
--Date: 2011-06-21
--Desc: Add column
----------------  
--DJ-Fix: No transactions in trunk-file, remove non-Azure code
--BEGIN TRANSACTION
--GO
ALTER TABLE dbo.PersonSkill ADD
	Active bit NOT NULL CONSTRAINT DF_PersonSkill_Active DEFAULT 1
--GO
--ALTER TABLE dbo.PersonSkill SET (LOCK_ESCALATION = TABLE)
GO
--COMMIT
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (330,'7.1.330') 
