/* 
Trunk initiated: 
2009-08-10 
14:11
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
/* 
Trunk initiated: 
2009-07-02 
By: TOPTINET\tamasb
On TELEOPTI494
*/ 
----------------  
--Name: tamasb  
--Date: 2009-07-01  
--Desc: Adds or modifies the default application functions. 
----------------  
SET NOCOUNT ON
BEGIN
	--insert to super user if not exist
	DECLARE @SuperUserId as uniqueidentifier
	SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'
	
	IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) -- check for the existence of super user role
		INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn])
			VALUES (@SuperUserId,1,@SuperUserId, NULL, getdate(), NULL, '', '', '', NULL, '_Super User', '_Super User', 'UTC', '_Super User', 0, 1) 

	--insert or modify application functions
	DECLARE @FunctionId as uniqueidentifier
	DECLARE @ForeignId as varchar(255)
	DECLARE @FunctionCode as varchar(255)
	DECLARE @FunctionDescription as varchar(255)
	DECLARE @ParentId as uniqueidentifier

	DECLARE @RaptorRootId as uniqueidentifier
	
	--insert/modify application function
	SELECT @ForeignId = '0000'	
	SELECT @FunctionCode = 'All'
	SELECT @FunctionDescription = 'xxAll'
	SELECT @ParentId = NULL

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function 
	SELECT @ForeignId = '0001'	
	SELECT @FunctionCode = 'Raptor'
	SELECT @FunctionDescription = 'xxOpenRaptorApplication'
	SELECT @ParentId = NULL

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	SELECT @RaptorRootId = @FunctionId

	--insert/modify application function
	SELECT @ForeignId = '0023'	
	SELECT @FunctionCode = 'Global'
	SELECT @FunctionDescription = 'xxGlobalFunctions'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	DECLARE @GlobalId as uniqueidentifier
	SELECT @GlobalId = @FunctionId

	--insert/modify application function
	SELECT @ForeignId = '0002'	
	SELECT @FunctionCode = 'Scheduler'
	SELECT @FunctionDescription = 'xxOpenSchedulePage'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	DECLARE @SchedulerId as uniqueidentifier
	SELECT @SchedulerId = @FunctionId

	--insert/modify application function
	SELECT @ForeignId = '0003'	
	SELECT @FunctionCode = 'Forecaster'
	SELECT @FunctionDescription = 'xxForecasts'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0004'	
	SELECT @FunctionCode = 'PersonAdmin'
	SELECT @FunctionDescription = 'xxOpenPersonAdminPage'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	DECLARE @PersonAdminId as uniqueidentifier
	SELECT @PersonAdminId = @FunctionId

	--insert/modify application function
	SELECT @ForeignId = '0006'	
	SELECT @FunctionCode = 'Reports'
	SELECT @FunctionDescription = 'xxReports'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0008'	
	SELECT @FunctionCode = 'Permission'
	SELECT @FunctionDescription = 'xxOpenPermissionPage'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	DECLARE @PermissionId as uniqueidentifier
	SELECT @PermissionId = @FunctionId

	--insert/modify application function
	SELECT @ForeignId = '0016'	
	SELECT @FunctionCode = 'Shifts'
	SELECT @FunctionDescription = 'xxShifts'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0017'	
	SELECT @FunctionCode = 'Options'
	SELECT @FunctionDescription = 'xxOptions'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	DECLARE @OptionsId as uniqueidentifier
	SELECT @OptionsId = @FunctionId

	--insert/modify application function
	SELECT @ForeignId = '0018'	
	SELECT @FunctionCode = 'Intraday'
	SELECT @FunctionDescription = 'xxIntraday'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	DECLARE @IntradayId as uniqueidentifier
	SELECT @IntradayId = @FunctionId

	--insert/modify application function
	-- note that the sql is different here to eliminate foreign id multiplication
	SELECT @ForeignId = '0050'	
	SELECT @FunctionCode = 'Budgets'
	SELECT @FunctionDescription = 'xxBudgets'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND FunctionCode = @FunctionCode))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND FunctionCode = @FunctionCode
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND FunctionCode = @FunctionCode

	--insert/modify application function
	SELECT @ForeignId = '0040'	
	SELECT @FunctionCode = 'PerformanceManager'
	SELECT @FunctionDescription = 'xxPerformanceManager'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	DECLARE @PerformanceManagerId as uniqueidentifier
	SELECT @PerformanceManagerId = @FunctionId

	--insert/modify application function
	SELECT @ForeignId = '0044'	
	SELECT @FunctionCode = 'PayrollIntegration'
	SELECT @FunctionDescription = 'xxPayrollIntegration'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0048'	
	SELECT @FunctionCode = 'UnderConstruction'
	SELECT @FunctionDescription = 'xxUnderConstruction'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0019'	
	SELECT @FunctionCode = 'AgentPortal'
	SELECT @FunctionDescription = 'xxAgentPortal'
	SELECT @ParentId = @RaptorRootId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	DECLARE @AgentPortalId as uniqueidentifier
	SELECT @AgentPortalId = @FunctionId

	--insert/modify application function
	SELECT @ForeignId = '0014'	
	SELECT @FunctionCode = 'ModifyAssignment'
	SELECT @FunctionDescription = 'xxModifyAssignment'
	SELECT @ParentId = @GlobalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	DECLARE @ModifyAssignmentId as uniqueidentifier
	SELECT @ModifyAssignmentId = @FunctionId

	--insert/modify application function
	SELECT @ForeignId = '0024'	
	SELECT @FunctionCode = 'ModifyMainShift'
	SELECT @FunctionDescription = 'xxModifyMainShift'
	SELECT @ParentId = @ModifyAssignmentId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0011'	
	SELECT @FunctionCode = 'ModifyPersonalShift'
	SELECT @FunctionDescription = 'xxModifyPersonalShift'
	SELECT @ParentId = @ModifyAssignmentId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0012'	
	SELECT @FunctionCode = 'ModifyAbsence'
	SELECT @FunctionDescription = 'xxModifyAbsence'
	SELECT @ParentId = @ModifyAssignmentId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0013'	
	SELECT @FunctionCode = 'ModifyDayOff'
	SELECT @FunctionDescription = 'xxModifyDayOff'
	SELECT @ParentId = @ModifyAssignmentId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0039'	
	SELECT @FunctionCode = 'ModifyPersonRestriction'
	SELECT @FunctionDescription = 'xxModifyPersonRestriction'
	SELECT @ParentId = @ModifyAssignmentId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0049'	
	SELECT @FunctionCode = 'ViewSchedules'
	SELECT @FunctionDescription = 'xxViewSchedules'
	SELECT @ParentId = @GlobalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0025'	
	SELECT @FunctionCode = 'ViewUnpublishedSchedules'
	SELECT @FunctionDescription = 'xxViewUnpublishedSchedules'
	SELECT @ParentId = @GlobalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0043'	
	SELECT @FunctionCode = 'ModifyMeetings'
	SELECT @FunctionDescription = 'xxModifyMeetings'
	SELECT @ParentId = @GlobalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0045'	
	SELECT @FunctionCode = 'ModifyWriteProtectedSchedule'
	SELECT @FunctionDescription = 'xxModifyWriteProtectedSchedule'
	SELECT @ParentId = @GlobalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0046'	
	SELECT @FunctionCode = 'SetWriteProtection'
	SELECT @FunctionDescription = 'xxSetWriteProtection'
	SELECT @ParentId = @GlobalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0007'	
	SELECT @FunctionCode = 'ModifyPersonNameAndPassword'
	SELECT @FunctionDescription = 'xxModifyPersonNameAndPassword'
	SELECT @ParentId = @PersonAdminId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0037'	
	SELECT @FunctionCode = 'ModifyGroupPage'
	SELECT @FunctionDescription = 'xxModifyGroupPage'
	SELECT @ParentId = @PersonAdminId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0038'	
	SELECT @FunctionCode = 'ModifyPeopleWithinGroupPage'
	SELECT @FunctionDescription = 'xxModifyPeopleWithinGroupPage'
	SELECT @ParentId = @PersonAdminId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0032'	
	SELECT @FunctionCode = 'RTA'
	SELECT @FunctionDescription = 'xxManageRTA'
	SELECT @ParentId = @OptionsId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0033'	
	SELECT @FunctionCode = 'Scorecards'
	SELECT @FunctionDescription = 'xxManageScorecards'
	SELECT @ParentId = @OptionsId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0009'	
	SELECT @FunctionCode = 'AutomaticScheduling'
	SELECT @FunctionDescription = 'xxAutomaticScheduling'
	SELECT @ParentId = @SchedulerId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	
	--insert/modify application function
	SELECT @ForeignId = '0021'	
	SELECT @FunctionCode = 'Request'
	SELECT @FunctionDescription = 'xxRequests'
	SELECT @ParentId = @SchedulerId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	DECLARE @RequestId as uniqueidentifier
	SELECT @RequestId = @FunctionId

	--insert/modify application function
	SELECT @ForeignId = '0022'	
	SELECT @FunctionCode = 'AutomaticUpdate'
	SELECT @FunctionDescription = 'xxAutomaticUpdate'
	SELECT @ParentId = @RequestId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0020'	
	SELECT @FunctionCode = 'ASM'
	SELECT @FunctionDescription = 'xxASM'
	SELECT @ParentId = @AgentPortalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0026'	
	SELECT @FunctionCode = 'ShiftCategoryPreferences'
	SELECT @FunctionDescription = 'xxModifyShiftCategoryPreferences'
	SELECT @ParentId = @AgentPortalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0027'	
	SELECT @FunctionCode = 'MyReport'
	SELECT @FunctionDescription = 'xxOpenMyReport'
	SELECT @ParentId = @AgentPortalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0028'	
	SELECT @FunctionCode = 'TextRequests'
	SELECT @FunctionDescription = 'xxCreateTextRequest'
	SELECT @ParentId = @AgentPortalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0029'	
	SELECT @FunctionCode = 'ShiftTradeRequests'
	SELECT @FunctionDescription = 'xxCreateShiftTradeRequest'
	SELECT @ParentId = @AgentPortalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0030'	
	SELECT @FunctionCode = 'AbsenceRequests'
	SELECT @FunctionDescription = 'xxCreateAbsenceRequest'
	SELECT @ParentId = @AgentPortalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0031'	
	SELECT @FunctionCode = 'Scorecard'
	SELECT @FunctionDescription = 'xxOpenScorecard'
	SELECT @ParentId = @AgentPortalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0036'	
	SELECT @FunctionCode = 'StudentAvailability'
	SELECT @FunctionDescription = 'xxCreateStudentAvailability'
	SELECT @ParentId = @AgentPortalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0034'	
	SELECT @FunctionCode = 'RTA'
	SELECT @FunctionDescription = 'xxRealTimeAdherence'
	SELECT @ParentId = @IntradayId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	-- note that the SQL is different here to eliminate multiple foreign id
	SELECT @ForeignId = '0035'	
	SELECT @FunctionCode = 'EW'
	SELECT @FunctionDescription = 'xxEarlyWarning'
	SELECT @ParentId = @IntradayId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND FunctionCode = @FunctionCode))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND FunctionCode = @FunctionCode
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND FunctionCode = @FunctionCode

	--insert/modify application function
	SELECT @ForeignId = '0041'	
	SELECT @FunctionCode = 'CreatePerformanceManagerReport'
	SELECT @FunctionDescription = 'xxCreatePerformanceManagerReport'
	SELECT @ParentId = @PerformanceManagerId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

	--insert/modify application function
	SELECT @ForeignId = '0042'	
	SELECT @FunctionCode = 'ViewPerformanceManagerReport'
	SELECT @FunctionDescription = 'xxViewPerformanceManagerReport'
	SELECT @ParentId = @PerformanceManagerId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, NULL, getdate(), NULL, @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
END
GO
 
 
GO 
 
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_stg_queue]'))
DROP VIEW [mart].[v_stg_queue]
GO
CREATE VIEW [mart].[v_stg_queue]
AS
SELECT * FROM stage.stg_queue

GO


  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplitStringString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[SplitStringString]
GO

CREATE   FUNCTION [dbo].[SplitStringString]
-- Takes an input string with strings separated by commas and
-- inserts the result into a field called id in a given table 
-- with name @table_name
--
-- Created: 990322 by viktor.edlund@advisorconsulting.se
-- Last changed: 990513 by viktor.edlund@advisorconsulting.se
-- Last changed: 990819 by Micke
-- Omgjord till en funktion Ola 2004-11-09
-- returnerar en tabell istället
(@string_string varchar(8000))
RETURNS @strings TABLE (string varchar(100) NOT NULL)
As
BEGIN 

 DECLARE @pos int
 DECLARE @string varchar(50)
 DECLARE @insert_text varchar(100)
 -- Exit if an empty string is given 
 IF @string_string = '' BEGIN
  RETURN 
 END 
 -- For simplicty concatenate , at the end of the string
 SELECT @string_string = @string_string + ','
 -- Ensure that @pos <> 0  
 SELECT @pos = CHARINDEX(',', @string_string )
 WHILE @pos <> 0 BEGIN
  -- Get the position of the first ,
  SELECT @pos = CHARINDEX(',', @string_string )
  
  -- Exit?
  IF @pos = 0 OR @pos = 1 OR @string_string = ','
   return
  -- Extract the substring
  SELECT @string = SUBSTRING(@string_string,1,@pos-1)
  -- Skip leading blanks
  SELECT @string = LTRIM(@string)
  -- Extract everything except the string
  SELECT @string_string = STUFF (@string_string,1,@pos,'')
  -- Insert the string into the return table
	INSERT INTO @strings
	SELECT @string
  
 END

RETURN

END


GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplitStringInt]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[SplitStringInt]
GO



-- SELECT * FROM SplitStringInt('1,2,3,4,5,6,7,8,9')

CREATE   FUNCTION [dbo].[SplitStringInt]
-- Takes an input string with strings separated by commas and
-- inserts the result into a field called id in a given table 
-- with name @table_name
--
-- Created: 990322 by viktor.edlund@advisorconsulting.se
-- Last changed: 990513 by viktor.edlund@advisorconsulting.se
-- Last changed: 990819 by Micke
-- Omgjord till en funktion Ola 2004-11-09
-- returnerar en tabell istället
(@string_string varchar(8000))
RETURNS @strings TABLE (id int NOT NULL)
As
BEGIN 

 DECLARE @pos int
 DECLARE @string varchar(50)
 DECLARE @insert_text varchar(100)
 -- Exit if an empty string is given 
 IF @string_string = '' BEGIN
  RETURN 
 END 
 -- For simplicty concatenate , at the end of the string
 SELECT @string_string = @string_string + ','
 -- Ensure that @pos <> 0  
 SELECT @pos = CHARINDEX(',', @string_string )
 WHILE @pos <> 0 BEGIN
  -- Get the position of the first ,
  SELECT @pos = CHARINDEX(',', @string_string )
  
  -- Exit?
  IF @pos = 0 OR @pos = 1 OR @string_string = ','
   return
  -- Extract the substring
  SELECT @string = SUBSTRING(@string_string,1,@pos-1)
  -- Skip leading blanks
  SELECT @string = LTRIM(@string)
  -- Extract everything except the string
  SELECT @string_string = STUFF (@string_string,1,@pos,'')
  -- Insert the string into the return table
	INSERT INTO @strings
	SELECT @string
  
 END

RETURN

END




GO

  
GO  
 
/****** Object:  StoredProcedure [mart].[raptor_v_stg_queue_delete]    Script Date: 02/02/2009 15:13:42 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_v_stg_queue_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_v_stg_queue_delete]
GO
/****** Object:  StoredProcedure [mart].[raptor_v_stg_queue_delete]    Script Date: 02/02/2009 15:13:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<KJ>
-- Create date: <2009-02-02>
-- Description:	<Delete data from v_stg_queue>
-- =============================================
CREATE PROCEDURE [mart].[raptor_v_stg_queue_delete]

AS
BEGIN
	DELETE FROM mart.v_stg_queue
END
  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_statistics_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_statistics_load]
GO
-- =============================================
-- Author:		Unknown
-- Create date: 2008-xx-xx
-- Description:	Return the queue workload used in "prepare workload"
-- Change date:	2008-12-02
--				DJ: Use existing functions to split input strings
-- =============================================
CREATE PROCEDURE [mart].[raptor_statistics_load] 
(@QueueList		varchar(1024),		
@DateFromList	varchar(1024),
@DateToList		varchar(1024)
)
AS
BEGIN
	SET NOCOUNT ON;
	--Declares
	DECLARE @TempList table
	(
	QueueID int
	)

	DECLARE	@TempDateFromList table
	(
	ID_num int IDENTITY(1,1),
	DateFrom smallDateTime
	)

	DECLARE	@TempDateToList table
	(
	ID_num int IDENTITY(1,1),
	DateTo smallDateTime
	)

	DECLARE @TempFromToDates table
	(
	ID_num int,
	DateFrom smalldatetime,
	DateTo smalldatetime
	)

	--Init
	INSERT INTO @TempList
	SELECT * FROM mart.SplitStringInt(@QueueList)

	INSERT INTO @TempDateFromList
	SELECT * FROM mart.SplitStringString(@DateFromList)

	INSERT INTO @TempDateToList
	SELECT * FROM mart.SplitStringString(@DateToList)

	INSERT INTO @TempFromToDates
	SELECT fromDates.ID_num, fromDates.DateFrom, toDates.DateTo
	FROM @TempDateFromList as fromDates
	INNER JOIN @TempDateToList as toDates ON fromDates.ID_num = toDates.ID_num

	--Return result set to client
	SELECT	
		DATEADD(mi, DATEDIFF(mi,'1900-01-01',i.interval_start), d.date_date) as Interval, 
		ql.offered_calls as StatCalculatedTasks,
		ql.abandoned_calls as StatAbandonedTasks, 
		ql.abandoned_short_calls as StatAbandonedShortTasks, 
		ql.abandoned_calls_within_SL as StatAbandonedTasksWithinSL, 
		ql.answered_calls as StatAnsweredTasks,
		ql.answered_calls_within_SL as StatAnsweredTasksWithinSL,
		ql.overflow_out_calls as StatOverflowOutTasks,
		ql.overflow_in_calls as StatOverflowInTasks,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.talk_time_s, 0)
			ELSE ISNULL(ql.talk_time_s/ql.answered_calls, 0)
		END AS StatAverageTaskTimeSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.after_call_work_s, 0)
			ELSE ISNULL(ql.after_call_work_s/ql.answered_calls, 0)
		END AS StatAverageAfterTaskTimeSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.speed_of_answer_s, 0)
			ELSE ISNULL(ql.speed_of_answer_s/ql.answered_calls, 0)
		END AS StatAverageQueueTimeSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.handle_time_s, 0)
			ELSE ISNULL(ql.handle_time_s/ql.answered_calls, 0)
		END AS StatAverageHandleTimeSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.time_to_abandon_s, 0)
			ELSE ISNULL(ql.time_to_abandon_s/ql.answered_calls, 0)
		END AS StatAverageTimeToAbandonSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.longest_delay_in_queue_answered_s, 0)
			ELSE ISNULL(ql.longest_delay_in_queue_answered_s/ql.answered_calls, 0)
		END AS StatAverageTimeLongestInQueueAnsweredSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.longest_delay_in_queue_abandoned_s, 0)
			ELSE ISNULL(ql.longest_delay_in_queue_abandoned_s/ql.answered_calls, 0)
		END AS StatAverageTimeLongestInQueueAbandonedSeconds
	FROM		mart.fact_queue ql 
	INNER JOIN	mart.dim_date d
		ON ql.date_id = d.date_id 
	INNER JOIN	mart.dim_interval i
		ON ql.interval_id = i.interval_id 
	INNER JOIN	mart.dim_queue q
		ON ql.queue_id = q.queue_id 
	WHERE q.queue_original_id IN (SELECT QueueID FROM @TempList)
	AND EXISTS
			(SELECT * FROM @TempFromToDates 
			WHERE DATEADD(mi, DATEDIFF(mi,'1900-01-01',i.interval_start), d.date_date) BETWEEN DateFrom and DateTo)
END
GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_reports_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_reports_load]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2009-01-20
-- Description:	Used by Freemimum to get an empty list fo reports. E.g fake the Analytics reports
-- Change:		
-- =============================================

CREATE PROCEDURE [mart].[raptor_reports_load]
AS

CREATE TABLE #report(
	[report_id] [int] NOT NULL,
	[url] [nvarchar](500) NULL,
	[report_name_resource_key] [nvarchar](50) NOT NULL
)

SELECT	report_id						as ReportId,
		'xx' + report_name_resource_key as ReportName, 
                                    url as ReportUrl
FROM #report  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_load_queues]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_load_queues]
GO

CREATE PROCEDURE mart.[raptor_load_queues] 
                            AS
                            BEGIN
	                            SET NOCOUNT ON;
                                SELECT	
										queue_original_id	QueueOriginalId, 
										queue_agg_id		QueueAggId, 
		                                queue_id			QueueMartId,
		                                datasource_id		DataSourceId,
		                                log_object_name		LogObjectName,
                                        queue_name			Name,
                                        queue_Description	[Description]                                        
                                FROM mart.dim_queue WHERE queue_id > -1
                            END
GO

  
GO  
 
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_load_acd_logins]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_load_acd_logins]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2009-01-20
-- Description:	Used by Freemimum to get an empty result set E.g fake the Analytics ACD-logins syncing
-- Change:		
-- =============================================

CREATE PROCEDURE mart.[raptor_load_acd_logins] 
AS

--Create teporaty table
CREATE TABLE #dim_acd_login(
	[acd_login_id] [int] IDENTITY(1,1) NOT NULL,
	[acd_login_agg_id] [int] NULL,
	[acd_login_original_id] [nvarchar](50) NULL,
	[acd_login_name] [nvarchar](100) NULL,
	[log_object_name] [nvarchar](100) NULL,
	[is_active] [bit] NULL,
	[datasource_id] [smallint] NULL
)

--Select empty result set for Freemimum
SELECT	acd_login_id			AcdLogOnMartId,
		acd_login_agg_id		AcdLogOnAggId, 
		acd_login_original_id	AcdLogOnOriginalId, 
		acd_login_name			AcdLogOnName,
		is_active				Active,
		datasource_id			DataSourceId
FROM #dim_acd_login
GO

  
GO  
 
/****** Object:  StoredProcedure [mart].[raptor_fact_queue_load]    Script Date: 02/02/2009 14:00:53 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_fact_queue_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_fact_queue_load]
GO
/****** Object:  StoredProcedure [mart].[raptor_fact_queue_load]    Script Date: 02/02/2009 14:00:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<KJ
-- Create date: <2009-02-02>
-- Update date: <2009-02-04>
-- Description:	<File Import - Loads data to fact_queue from stg_queue>
--				This procedure is for TeleoptiCCC database for Freeemium case, NOT same procedure as in TeleoptiAnalytics database(even though same name). Does not handle timezones.
-- =============================================
CREATE PROCEDURE [mart].[raptor_fact_queue_load] 
AS
BEGIN
--DECLARE
DECLARE @datasource_id smallint
DECLARE @log_object_name nvarchar(100)

--INIT
SET @datasource_id = 1
SET @log_object_name  = 'Teleopti CCC - File import'

--tidszoner, hur g�ra?
--DECLARE @time_zone_id smallint
--SELECT 
--	@time_zone_id = ds.time_zone_id
--FROM
--	v_sys_datasource ds
--WHERE 
--	ds.datasource_id= @datasource_id

--CREATE TABLE #bridge_time_zone(date_id int,interval_id int,time_zone_id int,local_date_id int,local_interval_id int)
--
----H��mta datum och intervall grupperade sǾ vi slipper dubletter vid sommar-vintertid
--INSERT #bridge_time_zone(date_id,time_zone_id,local_date_id,local_interval_id)
--SELECT	min(date_id),	time_zone_id, 	local_date_id,	local_interval_id
--FROM bridge_time_zone 
--WHERE time_zone_id	= @time_zone_id	
--AND local_date_id BETWEEN @start_date_id AND @end_date_id
--GROUP BY time_zone_id, local_date_id,local_interval_id
--
--UPDATE #bridge_time_zone
--SET interval_id= bt.interval_id
--FROM 
--(SELECT date_id,local_date_id,local_interval_id,interval_id= MIN(interval_id)
--FROM bridge_time_zone
--WHERE time_zone_id=@time_zone_id
--GROUP BY date_id,local_date_id,local_interval_id)bt
--INNER JOIN #bridge_time_zone temp ON temp.local_interval_id=bt.local_interval_id
--AND temp.date_id=bt.date_id
--AND temp.local_date_id=bt.local_date_id


DECLARE @start_date_id	INT
DECLARE @end_date_id	INT

DECLARE @max_date smalldatetime
DECLARE @min_date smalldatetime


SELECT  
	@min_date= min(date),
	@max_date= max(date)
FROM
	mart.v_stg_queue
 

SET	@min_date = convert(smalldatetime,floor(convert(decimal(18,4),@min_date )))
SET @max_date	= convert(smalldatetime,floor(convert(decimal(18,4),@max_date )))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @min_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @max_date = date_date)

--ANALYZE AND UPDATE DATA IN TEMPORARY TABLE
SELECT *
INTO #stg_queue
FROM mart.v_stg_queue

UPDATE #stg_queue
SET queue_code = d.queue_original_id
FROM mart.dim_queue d
INNER JOIN #stg_queue stg ON stg.queue_name=d.queue_name
AND d.datasource_id = @datasource_id  
WHERE (stg.queue_code is null OR stg.queue_code='')

ALTER TABLE  #stg_queue ADD interval_id smallint

UPDATE #stg_queue
SET interval_id= i.interval_id
FROM mart.dim_interval i
INNER JOIN #stg_queue stg ON stg.interval=LEFT(i.interval_name,5)

-- Delete rows for the queues imported from file
DELETE FROM mart.fact_queue  
WHERE local_date_id BETWEEN @start_date_id 
AND @end_date_id AND datasource_id = @datasource_id
AND queue_id IN (SELECT queue_id from mart.dim_queue dq INNER JOIN #stg_queue stg ON dq.queue_original_id=stg.queue_code WHERE dq.datasource_id = @datasource_id )


INSERT INTO mart.fact_queue
	(
	date_id, 
	interval_id, 
	queue_id, 
	local_date_id,
	local_interval_id, 
	offered_calls, 
	answered_calls, 
	answered_calls_within_SL, 
	abandoned_calls, 
	abandoned_calls_within_SL, 
	abandoned_short_calls, 
	overflow_out_calls,
	overflow_in_calls,
	talk_time_s, 
	after_call_work_s, 
	handle_time_s, 
	speed_of_answer_s, 
	time_to_abandon_s, 
	longest_delay_in_queue_answered_s,
	longest_delay_in_queue_abandoned_s,
	datasource_id, 
	insert_date, 
	update_date, 
	datasource_update_date
	)
SELECT
	date_id						= d.date_id,--bridge.date_id, 
	interval_id					= stg.interval_id,--bridge.interval_id, 
	queue_id					= q.queue_id, 
	local_date_id				= d.date_id,
	local_interval_id			= stg.interval_id, 
	offered_calls				= ISNULL(offd_direct_call_cnt,0), 
	answered_calls				= ISNULL(answ_call_cnt,0), 
	answered_calls_within_SL	= ISNULL(ans_servicelevel_cnt,0), 
	abandoned_calls				= ISNULL(aband_call_cnt,0), 
	abandoned_calls_within_SL	= ISNULL(aband_within_sl_cnt,0), 
	abandoned_short_calls		= ISNULL(aband_short_call_cnt,0), 
	overflow_out_calls			= ISNULL(overflow_out_call_cnt,0),
	overflow_in_calls			= ISNULL(overflow_in_call_cnt,0), 
	talk_time_s					= ISNULL(talking_call_dur,0), 
	after_call_work_s			= ISNULL(wrap_up_dur,0), 
	handle_time_s				= ISNULL(talking_call_dur,0)+ISNULL(wrap_up_dur,0), 
	speed_of_answer_s			= ISNULL(queued_and_answ_call_dur,0), 
	time_to_abandon_s			= ISNULL(queued_and_aband_call_dur,0), 
	longest_delay_in_queue_answered_s = ISNULL(queued_answ_longest_que_dur,0),
	longest_delay_in_queue_abandoned_s = ISNULL(queued_aband_longest_que_dur,0),
	datasource_id				= q.datasource_id, 
	insert_date					= getdate(), 
	update_date					= getdate(), 
	datasource_update_date		= '1900-01-01'

FROM
	(SELECT * FROM #stg_queue WHERE date between @min_date and @max_date) stg
JOIN
	mart.dim_date		d
ON
	stg.date	= d.date_date
JOIN
	mart.dim_interval i
ON
	stg.interval = substring(i.interval_name,1,5)
JOIN
	mart.dim_queue		q
ON
	q.queue_original_id= stg.queue_code 
	AND q.datasource_id = @datasource_id

END  
GO  
 
/****** Object:  StoredProcedure [mart].[raptor_dim_queue_load]    Script Date: 02/02/2009 14:00:15 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_dim_queue_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_dim_queue_load]
GO
/****** Object:  StoredProcedure [mart].[raptor_dim_queue_load]    Script Date: 02/02/2009 14:00:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<KJ>
-- Create date: 2009-02-02
-- Description:	File Import - Loads data to dim_queue from stg_queue
-- Update date: 2009-07-07 Update queue_description if NULL
-- =============================================
CREATE PROCEDURE [mart].[raptor_dim_queue_load] 
	
AS
BEGIN
--DECLARE
DECLARE @datasource_id smallint
DECLARE @log_object_name nvarchar(100)

--INIT
SET @datasource_id = 1
SET @log_object_name  = 'Teleopti CCC - File import'

--------------------------------------------------------------
-- Insert Not Defined queue
SET IDENTITY_INSERT mart.dim_queue ON
INSERT INTO mart.dim_queue
	(
	queue_id,
	queue_name,
	datasource_id	
	)
SELECT 
	queue_id			=-1,
	queue_name			='Not Defined',
	datasource_id		=-1
WHERE NOT EXISTS (SELECT * FROM mart.dim_queue where queue_id = -1)
SET IDENTITY_INSERT mart.dim_queue OFF

--Update
--Existing queues with a queue_original_id in the importfile
UPDATE mart.dim_queue
SET
	queue_original_id		=stage.queue_code, 
	queue_name		=stage.queue_name,
	log_object_name =@log_object_name, 
	update_date		= getdate()
FROM
	mart.v_stg_queue stage
JOIN
	mart.dim_queue
ON
	mart.dim_queue.queue_original_id		= stage.queue_code 			AND
	mart.dim_queue.datasource_id	= @datasource_id
WHERE stage.queue_code IS NOT NULL

--------------------------------------------------------------
--Update
--Existing queues without a queue_code (fallback on queue_name)
UPDATE mart.dim_queue
SET 
	queue_original_id		=q.queue_id, 
	queue_name		=stage.queue_name,
	log_object_name =@log_object_name, 
	update_date		= getdate()
FROM
	mart.v_stg_queue stage	
JOIN
	mart.dim_queue q
ON
	q.queue_name		= stage.queue_name 			AND
	q.datasource_id		= @datasource_id
WHERE stage.queue_code IS NULL

---------------------------------------------------------------
-- Reset identity seed.
DECLARE @max_id INT
SET @max_id= (SELECT max(queue_id) FROM mart.dim_queue)

DBCC CHECKIDENT ('mart.dim_queue',reseed,@max_id);
---------------------------------------------------------------------------
-- Insert new queues with a queue_code
INSERT INTO mart.dim_queue
	( 
	queue_agg_id,
	queue_original_id, 
	queue_name, 
	log_object_name,
	datasource_id
	)
SELECT 
	queue_agg_id			= NULL, 
	queue_original_id		= stage.queue_code, 
	queue_name				= max(stage.queue_name),
	log_object_name			= @log_object_name ,
	datasource_id			= @datasource_id
FROM
	mart.v_stg_queue stage
WHERE 
	NOT EXISTS (SELECT queue_id FROM mart.dim_queue d 
					WHERE	d.queue_original_id= stage.queue_code 	AND
							d.datasource_id =@datasource_id
				)
AND stage.queue_code IS NOT NULL
GROUP BY stage.queue_code

----------------------------------------------------------------------------------------
-- Insert new queues without a queue_code (fallback on queue_name)
INSERT INTO mart.dim_queue
	( 
	queue_agg_id,
	queue_original_id, 
	queue_name, 
	log_object_name,
	datasource_id
	)
SELECT 
	queue_agg_id			= NULL, 
	queue_original_id				= NULL, 
	queue_name				= stage.queue_name,
	log_object_name			= @log_object_name ,
	datasource_id			= @datasource_id
FROM
	mart.v_stg_queue stage
WHERE 
	NOT EXISTS (SELECT queue_id FROM mart.dim_queue d 
					WHERE	d.queue_name= stage.queue_name 	AND
							d.datasource_id =@datasource_id
				)
AND stage.queue_code IS NULL
GROUP BY stage.queue_name

--SET queue_agg_id AND queue_code TO SAME VALUES AS queue_id IF NO queue_code OR queue_agg_id
UPDATE mart.dim_queue
SET queue_agg_id=queue_id, queue_original_id= queue_id
WHERE queue_agg_id IS NULL 
AND queue_original_id IS NULL
AND datasource_id=@datasource_id

UPDATE mart.dim_queue
SET queue_agg_id=queue_original_id
WHERE queue_agg_id IS NULL 
AND queue_original_id IS NOT NULL
AND datasource_id=@datasource_id

--Update queue_description if IS NULL
UPDATE mart.dim_queue
SET queue_description = queue_name
WHERE queue_description IS NULL
AND datasource_id=@datasource_id --only for File Imports!

END
GO
  
GO  
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (136,'7.0.136') 
