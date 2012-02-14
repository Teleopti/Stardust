/* 
Trunk initiated: 
2009-12-16 
20:35
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Robin+David
--Date: 2010-01-13  
--Desc: Adds a new application function
----------------  
SET NOCOUNT ON
	--insert to super user if not exist
	DECLARE @SuperUserId as uniqueidentifier
	SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'
	
	IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) -- check for the existence of super user role
		INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [ApplicationLogOnName], [IsDeleted], [BuiltIn])
			VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', '_Super User', 0, 1) 

	--insert or modify application functions
	DECLARE @FunctionId as uniqueidentifier
	DECLARE @ForeignId as varchar(255)
	DECLARE @FunctionCode as varchar(255)
	DECLARE @FunctionDescription as varchar(255)
	DECLARE @ParentId as uniqueidentifier

	--Get parent level
	DECLARE @AgentPortalId as uniqueidentifier
	SELECT @ForeignId = '0019'	--AgentPortal hardcoded
	SELECT @AgentPortalId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	
	--insert/modify application function
	SELECT @ForeignId = '0051'	
	SELECT @FunctionCode = 'ExtendedPreferences'
	SELECT @FunctionDescription = 'xxExtendedPreferences'
	SELECT @ParentId = @AgentPortalId

	IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
		INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
			VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
	SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
	UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO


----------------  
--Name: Roger
--Date: 2010-01-26 
--Desc: Script for clr type shiftcategorylimitation and table SchedulePeriodShiftCategoryLimitation
create table dbo.SchedulePeriodShiftCategoryLimitation 
			(SchedulePeriod UNIQUEIDENTIFIER not null, 
			Weekly BIT not null, 
			MaxNumberOf INT not null, 
			ShiftCategory UNIQUEIDENTIFIER not null,unique (SchedulePeriod, ShiftCategory))

GO

alter table dbo.SchedulePeriodShiftCategoryLimitation add constraint FK_Limitation_ShiftCategory foreign key (ShiftCategory) references ShiftCategory
GO
alter table dbo.SchedulePeriodShiftCategoryLimitation add constraint FK_Limitation_SchedulePeriod foreign key (SchedulePeriod) references SchedulePeriod
GO

PRINT 'Adding build number to database' 
INSERT INTO dbo.DatabaseVersion(BuildNumber, SystemVersion) VALUES (196,'7.1.196') 
