----------------  
--Name: Mattias Engblom
--Date: 2012-05-15  
--Desc: Add the following new application function> AbsenceRequestsWeb
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
SELECT @ParentForeignId = '0065'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0077' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'AbsenceRequestsWeb' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxAbsenceRequestsWeb' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Mattias E
--Date: 2012-05-30  
--Desc: Move Anywhere fron UnderCOnstruction
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
SELECT @ParentForeignId = '0001'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0074' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'Anywhere' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxAnywhere' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Robin Karlsson
--Date: 2012-06-01
--Desc: Removing column IsDeleted as it isn't used anymore. (also in trunk script in 7.2.362)
----------------  
IF EXISTS(SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('dbo.StateGroupActivityAlarm','U') AND name='IsDeleted')
	BEGIN
		DELETE FROM dbo.StateGroupActivityAlarm	WHERE IsDeleted=1
	END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('dbo.StateGroupActivityAlarm','U') AND name='IsDeleted')
	BEGIN
		ALTER TABLE dbo.StateGroupActivityAlarm	DROP COLUMN IsDeleted
	END
	
GO
