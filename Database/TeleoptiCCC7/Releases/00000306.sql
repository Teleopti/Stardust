/* 
Trunk initiated: 
2010-11-03 
14:41
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Jonas Nordh
--Date: 2010-10-21  
--Desc: Add the following new application function> Schedule Audit Trail online Report
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
--Not needed. DBManager should catch and report this error if we run in to it!
/*
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', '_Super User', 0, 1) 
*/

--get parent level
SELECT @ParentForeignId = '0054'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0059' -- Foreign id of the function > hardcoded
SELECT @FunctionCode = 'ScheduleAuditTrailReport' -- Name of the function > hardcoded
SELECT @FunctionDescription = 'xxScheduleAuditTrailReport' -- Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: tamasb  
--Date: 2010-10-27  
--Desc: Add new columns to SchedulePeriod   
----------------  
ALTER TABLE dbo.SchedulePeriod ADD
	BalanceIn bigint NOT NULL CONSTRAINT DF_SchedulePeriod_BalanceIn DEFAULT 0,
	BalanceOut bigint NOT NULL CONSTRAINT DF_SchedulePeriod_BalanceOut DEFAULT 0,
	Extra bigint NOT NULL CONSTRAINT DF_SchedulePeriod_Extra DEFAULT 0
GO

----------------  
--Name: Ola Håkansson, moved to release 306 by David
--Date: 2010-11-11  
--Desc: Because duplicates could be saved to KpiTarget  
----------------  
-- find duplicates
select max(CreatedOn) Created ,[KeyPerformanceIndicator] ,[Team]
into #leave
FROM KpiTarget
Group by [KeyPerformanceIndicator]
			,[Team]
			having count(Id)>1
	           
	-- remove all except latest
	DELETE from KpiTarget
	from KpiTarget
	INNER JOIN #leave
	On KpiTarget.KeyPerformanceIndicator = #leave.KeyPerformanceIndicator
	AND KpiTarget.[Team] = #leave.[Team]
	WHERE CreatedOn <> Created
	 
-- Add a Index so we can't have duplicates
CREATE UNIQUE INDEX IX_KpiTarget ON dbo.KpiTarget
	(
	KeyPerformanceIndicator,
	Team
	) ON [PRIMARY]

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (306,'7.1.306') 
