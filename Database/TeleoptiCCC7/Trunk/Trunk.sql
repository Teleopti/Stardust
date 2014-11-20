----------------  
--Name: CS
--Date: 2014-11-20
--Desc: Add new application function "Publish Schedule"
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
INSERT [dbo].[Person]([Id], [Version], [UpdatedBy],[UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1, @SuperUserId, getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1)

--get parent level
SELECT @ParentForeignId = '0023'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0105' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'PublishSchedule' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxPublishSchedule' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Xianwei Shen
--Date: 2014-11-20
--Desc: Add new table for adherence details
---------------- 
DROP TABLE [ReadModel].[AdherenceDetails];
GO
CREATE TABLE [ReadModel].[AdherenceDetails](
	[PersonId] [uniqueidentifier] NOT NULL,
	[BelongsToDate] [smalldatetime] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[StartTime] [datetime] NULL,
	[ActualStartTime] [datetime] NULL,
	[LastStateChangedTime] [datetime] NULL,
	[IsInAdherence] [bit] NULL,
	[TimeInAdherence] [bigint] NULL,
	[TimeOutOfAdherence] [bigint] NULL,
)
GO
CREATE UNIQUE CLUSTERED INDEX [UCI_AdherenceDetails] ON [ReadModel].[AdherenceDetails]
(
	[PersonId] ASC,
	[BelongsToDate] ASC,
	[StartTime] ASC
)
GO