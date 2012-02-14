/* 
Trunk initiated: 
2010-06-07 
14:26
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: tamasb  
--Date: 2010-06-07  
--Desc: Remove two application functions: ModifyMainShift, ModifyPersonalShift  
----------------  
update dbo.ApplicationFunction
set  IsDeleted = 1
where  (ForeignId = '0024' and ForeignSource = 'Raptor')
	or (ForeignId = '0011' and ForeignSource = 'Raptor')
	
----------------  
--Name: Zoë  
--Date: 2010-06-07  
--Desc: Adding new appfunction for Shift trade request in options  
----------------  
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

--get parent level
SELECT @ParentForeignId = '0017'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0056' -- Foreign id of the function that is hardcoded	
SELECT @FunctionCode = 'ShiftTradeRequest' -- Name of the function that is hardcoded
SELECT @FunctionDescription = 'xxShiftTradeRequest' -- Description of the function that is hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (278,'7.1.278') 
