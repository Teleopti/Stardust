----------------  
--Name: MD
--Date: 2016-06-26
--Desc: Add new application function "Add Person"
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
INSERT [dbo].[Person]([Id], [Version], [UpdatedBy],[UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [Culture], [UiCulture], [IsDeleted], [WriteProtectionUpdatedOn], [PersonWriteProtectedDate], [WriteProtectionUpdatedBy], [WorkflowControlSet], [FirstDayOfWeek])
VALUES (@SuperUserId,1, @SuperUserId, getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', NULL, NULL, 0, NULL, NULL, NULL, NULL, 1)

--get parent level
SELECT @ParentForeignId = '0004'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0135' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'AddPerson' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxAddPerson' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

-- Allow modification for all that have open people

DECLARE @OpenPeople as uniqueidentifier
DECLARE @ChangePeople as uniqueidentifier

select @OpenPeople = id from ApplicationFunction
where ForeignId = '0004'

select @ChangePeople = id from ApplicationFunction
where ForeignId = '0135'

INSERT INTO ApplicationFunctionInRole
SELECT ApplicationRole, @ChangePeople, GETDATE() FROM ApplicationFunctionInRole AFR
where ApplicationFunction = @OpenPeople
AND ApplicationRole NOT IN(SELECT ApplicationRole FROM ApplicationFunctionInRole AFR1 
WHERE AFR1.ApplicationFunction = @ChangePeople)
GO
