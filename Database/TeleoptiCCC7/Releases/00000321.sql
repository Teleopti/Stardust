/* 
Trunk initiated: 
2011-03-24 
08:26
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Xianwei Shen
--Date: 2011-03-23  
--Desc: Add student availability to workflow control set settings
----------------  
ALTER TABLE dbo.WorkflowControlSet ADD
	StudentAvailabilityPeriodFromDate datetime NOT NULL DEFAULT ('1901-01-01T00:00:00'),
	StudentAvailabilityPeriodToDate datetime NOT NULL DEFAULT ('2029-01-01T00:00:00'),
	StudentAvailabilityInputFromDate datetime NOT NULL DEFAULT ('1901-01-01T00:00:00'),
	StudentAvailabilityInputToDate datetime NOT NULL DEFAULT ('2029-01-01T00:00:00')
----------------  
--Name: Mattiae Engblom  
--Date: 2011-03-30
--Desc: Add report Agent Queue Metrics
----------------  
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
SELECT @ParentForeignId = '0006'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE FunctionCode='Reports' AND ForeignSource = 'Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '24' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ResReportAgentQueueMetrics' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxResReportAgentQueueMetrics' --Description of the function > hardcoded

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Matrix' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Matrix', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Matrix' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction]
SET [ForeignId]=@ForeignId, [Parent]=@ParentId
WHERE ForeignSource='Matrix' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')





 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (321,'7.1.321') 
