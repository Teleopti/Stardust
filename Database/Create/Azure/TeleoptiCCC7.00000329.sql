SET NOCOUNT ON
GO
CREATE SCHEMA [AuditTrail] AUTHORIZATION [dbo]
GO
CREATE SCHEMA [mart] AUTHORIZATION [dbo]
GO
CREATE SCHEMA [stage] AUTHORIZATION [dbo]
GO
CREATE SCHEMA [stg] AUTHORIZATION [dbo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[XmlResult](
	[Id] [uniqueidentifier] NOT NULL,
	[XPathNavigable] [xml] NULL,
 CONSTRAINT [PK_XmlResult] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BusinessUnit](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_BusinessUnit] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Person](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Email] [nvarchar](50) NOT NULL,
	[Note] [nvarchar](1024) NOT NULL,
	[EmploymentNumber] [nvarchar](50) NOT NULL,
	[TerminalDate] [datetime] NULL,
	[FirstName] [nvarchar](25) NOT NULL,
	[LastName] [nvarchar](25) NOT NULL,
	[PartOfUnique] [uniqueidentifier] NULL,
	[DefaultTimeZone] [nvarchar](50) NOT NULL,
	[Culture] [int] NULL,
	[UiCulture] [int] NULL,
	[WindowsLogOnName] [nvarchar](50) NULL,
	[DomainName] [nvarchar](50) NULL,
	[ApplicationLogOnName] [nvarchar](50) NULL,
	[Password] [nvarchar](50) NULL,
	[IsDeleted] [bit] NOT NULL,
	[WriteProtectionUpdatedOn] [datetime] NULL,
	[PersonWriteProtectedDate] [datetime] NULL,
	[WriteProtectionUpdatedBy] [uniqueidentifier] NULL,
	[BuiltIn] [bit] NOT NULL,
	[WorkflowControlSet] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
),
 CONSTRAINT [UQ_Person_PartOfUnique] UNIQUE NONCLUSTERED 
(
	[PartOfUnique] ASC,
	[WindowsLogOnName] ASC,
	[DomainName] ASC,
	[ApplicationLogOnName] ASC
)
)
GO
INSERT [dbo].[Person] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [PartOfUnique], [DefaultTimeZone], [Culture], [UiCulture], [WindowsLogOnName], [DomainName], [ApplicationLogOnName], [Password], [IsDeleted], [WriteProtectionUpdatedOn], [PersonWriteProtectedDate], [WriteProtectionUpdatedBy], [BuiltIn], [WorkflowControlSet]) VALUES (N'3f0886ab-7b25-4e95-856a-0d726edc2a67', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0288 AS DateTime), CAST(0x00009EFD00AB0288 AS DateTime), N'', N'', N'', NULL, N'System', N'System', NULL, N'UTC', NULL, NULL, NULL, NULL, N'_Super User', NULL, 0, NULL, NULL, NULL, 1, NULL)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PayrollExport](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[FileFormat] [int] NOT NULL,
	[PayrollFormatName] [nvarchar](50) NOT NULL,
	[PayrollFormatId] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_PayrollExport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PayrollResult](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[PayrollFormatName] [nvarchar](50) NOT NULL,
	[PayrollFormatId] [uniqueidentifier] NOT NULL,
	[Owner] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[PayrollExport] [uniqueidentifier] NOT NULL,
	[FinishedOk] [bit] NOT NULL,
	[XmlResult] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PayrollResultDetail](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Message] [nvarchar](1000) NOT NULL,
	[ExceptionStackTrace] [nvarchar](max) NULL,
	[ExceptionMessage] [nvarchar](2000) NULL,
	[InnerExceptionStackTrace] [nvarchar](max) NULL,
	[InnerExceptionMessage] [nvarchar](2000) NULL,
	[Timestamp] [datetime] NOT NULL,
	[DetailLevel] [int] NOT NULL,
 CONSTRAINT [PK_PayrollResultDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SkillType](
	[Id] [uniqueidentifier] NOT NULL,
	[ForecastType] [int] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[ForecastSource] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_SkillType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GroupingActivity](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_GroupingActivity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Activity](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[DisplayColor] [int] NOT NULL,
	[InContractTime] [bit] NOT NULL,
	[InReadyTime] [bit] NOT NULL,
	[RequiresSkill] [bit] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[GroupingActivity] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[InPaidTime] [bit] NOT NULL,
	[InWorkTime] [bit] NOT NULL,
	[ReportLevelDetail] [int] NOT NULL,
	[IsMaster] [nvarchar](1) NOT NULL,
	[PayrollCode] [nvarchar](20) NULL,
	[RequiresSeat] [bit] NOT NULL,
 CONSTRAINT [PK_Activity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Skill](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[DisplayColor] [int] NOT NULL,
	[Description] [nvarchar](1024) NOT NULL,
	[DefaultResolution] [int] NOT NULL,
	[SkillType] [uniqueidentifier] NOT NULL,
	[Activity] [uniqueidentifier] NOT NULL,
	[TimeZone] [nvarchar](50) NOT NULL,
	[MidnightBreakOffset] [bigint] NOT NULL,
	[SeriousUnderstaffing] [float] NOT NULL,
	[Understaffing] [float] NOT NULL,
	[Overstaffing] [float] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[Priority] [int] NOT NULL,
	[OverstaffingFactor] [float] NOT NULL,
	[UnderstaffingFor] [float] NOT NULL,
 CONSTRAINT [PK_Skill] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Workload](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](1024) NOT NULL,
	[Skill] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[OfferedTasks] [float] NOT NULL,
	[OverflowIn] [float] NOT NULL,
	[OverflowOut] [float] NOT NULL,
	[AbandonedShort] [float] NOT NULL,
	[AbandonedWithinServiceLevel] [float] NOT NULL,
	[AbandonedAfterServiceLevel] [float] NOT NULL,
	[Abandoned] [float] NOT NULL,
 CONSTRAINT [PK_Workload] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Outlier](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[Workload] [uniqueidentifier] NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Outlier] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OutlierDates](
	[Parent] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NULL
)
GO
CREATE CLUSTERED INDEX [CIX_OutlierDates_Parent] ON [dbo].[OutlierDates] 
(
	[Parent] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OutlierDateProviderBase](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NULL,
 CONSTRAINT [PK_OutlierDateProviderBase] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OptionalColumn](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[TableName] [nvarchar](255) NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_OptionalColumn] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OptionalColumnValue](
	[Id] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[ReferenceId] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_OptionalColumnValue] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Scenario](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[DefaultScenario] [bit] NOT NULL,
	[Audittrail] [bit] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[EnableReporting] [bit] NOT NULL,
	[Restricted] [bit] NOT NULL,
 CONSTRAINT [PK_Scenario] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MultisiteSkill](
	[Skill] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_MultisiteSkill] PRIMARY KEY CLUSTERED 
(
	[Skill] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MultisiteDay](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[TemplateName] [nvarchar](50) NULL,
	[TemplateId] [uniqueidentifier] NOT NULL,
	[DayOfWeek] [int] NULL,
	[VersionNumber] [int] NOT NULL,
	[TemplateReferenceSkill] [uniqueidentifier] NULL,
	[MultisiteDayDate] [datetime] NOT NULL,
	[Skill] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_MultisiteDay] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
),
 CONSTRAINT [UQ_MultisiteDay_MultisiteDayDate] UNIQUE NONCLUSTERED 
(
	[MultisiteDayDate] ASC,
	[Skill] ASC,
	[Scenario] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MultisitePeriod](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_MultisitePeriod] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Multiplicator](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[DisplayColor] [int] NOT NULL,
	[MultiplicatorType] [int] NOT NULL,
	[MultiplicatorValue] [float] NOT NULL,
	[ExportCode] [nvarchar](255) NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MultiplicatorDefinitionSet](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[MultiplicatorType] [int] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MultiplicatorDefinition](
	[Id] [uniqueidentifier] NOT NULL,
	[DefinitionType] [nvarchar](255) NOT NULL,
	[OrderIndex] [int] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[DayOfWeek] [int] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[StartTime] [bigint] NULL,
	[EndTime] [bigint] NULL,
	[Multiplicator] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApplicationFunction](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Parent] [uniqueidentifier] NULL,
	[FunctionCode] [nvarchar](50) NOT NULL,
	[FunctionDescription] [nvarchar](255) NULL,
	[ForeignId] [nvarchar](255) NULL,
	[ForeignSource] [nvarchar](255) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_ApplicationFunction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'bbec8cbe-8764-457d-8557-02fe3af70b2f', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0A45 AS DateTime), CAST(0x00009EFD00AB0A45 AS DateTime), N'cf4a9dba-62b9-4976-a653-a0bccf3b18cf', N'ScheduleAuditTrailReport', N'xxScheduleAuditTrailReport', N'0059', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'2186d3cd-a369-4fd9-9bf7-09c26fe2ce3f', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB058D AS DateTime), CAST(0x00009EFD00AB058D AS DateTime), N'7730c397-f7d0-4bbb-93a3-d91d103ef91c', N'TextRequests', N'xxCreateTextRequest', N'0028', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'd1841cd7-b5be-4e1a-8652-0cf9ec1461d6', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0AE1 AS DateTime), CAST(0x00009EFD00AB0AE1 AS DateTime), N'4398f0f7-9177-4fec-80e6-b92334afdbc4', N'ModifyRestrictedScenario', N'xxModifyRestrictedScenario', N'0062', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'813eaad8-53e2-4608-967e-1466f1d78ef5', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0594 AS DateTime), CAST(0x00009EFD00AB0594 AS DateTime), N'7730c397-f7d0-4bbb-93a3-d91d103ef91c', N'StudentAvailability', N'xxCreateStudentAvailability', N'0036', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'20c8af31-71b3-4cd5-9089-173dfab29af1', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0576 AS DateTime), CAST(0x00009EFD00AB0576 AS DateTime), N'fc718794-807d-4304-b73e-4478afc37836', N'ModifyDayOff', N'xxModifyDayOff', N'0013', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'9a295707-cabe-4b9d-9ac6-1b87c58e1908', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056E AS DateTime), CAST(0x00009EFD00AB056E AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'PayrollIntegration', N'xxPayrollIntegration', N'0044', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'fa343f9f-7157-4de8-8ea1-2c6e6e9512ce', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0575 AS DateTime), CAST(0x00009EFD00AB0575 AS DateTime), N'809343a9-57db-4907-a658-9640d8a2eefb', N'ModifyPersonalShift', N'xxModifyPersonalShift', N'0011', N'Raptor', 1)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'7643d961-f3e6-4ca3-aa2c-2d2fb1b57b1a', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0AFB AS DateTime), CAST(0x00009EFD00AB0AFB AS DateTime), N'cf4a9dba-62b9-4976-a653-a0bccf3b18cf', N'ScheduleTimeVersusTargetTimeReport', N'xxScheduleTimeVersusTargetTimeReport', N'0064', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'bae8aad0-f872-4a2f-925c-2ff6ded9b8eb', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056D AS DateTime), CAST(0x00009EFD00AB056D AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'Forecaster', N'xxForecasts', N'0003', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'0ee42584-83b1-49f8-ade0-36b0e61b4673', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056D AS DateTime), CAST(0x00009EFD00AB056D AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'PersonAdmin', N'xxOpenPersonAdminPage', N'0004', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'8e9422be-8e5e-4de7-9407-3a21c329ee56', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB058D AS DateTime), CAST(0x00009EFD00AB058D AS DateTime), N'7730c397-f7d0-4bbb-93a3-d91d103ef91c', N'ShiftTradeRequests', N'xxCreateShiftTradeRequest', N'0029', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'776aa032-4e2e-4534-9b76-3c1a55e38ca0', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0594 AS DateTime), CAST(0x00009EFD00AB0594 AS DateTime), N'ae085df8-1987-404a-9ffb-4a7ff6108ae8', N'RTA', N'xxRealTimeAdherence', N'0034', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'3780395d-0902-45d9-8fd9-413eec02520d', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0584 AS DateTime), CAST(0x00009EFD00AB0584 AS DateTime), N'4398f0f7-9177-4fec-80e6-b92334afdbc4', N'SetWriteProtection', N'xxSetWriteProtection', N'0046', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'fc718794-807d-4304-b73e-4478afc37836', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0982 AS DateTime), CAST(0x00009EFD00AB0982 AS DateTime), N'4398f0f7-9177-4fec-80e6-b92334afdbc4', N'ModifySchedule', N'xxModifySchedule', N'0057', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'c74b298a-e2ee-42c1-9044-4536262de564', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0AE7 AS DateTime), CAST(0x00009EFD00AB0AE7 AS DateTime), N'1f8abe90-9793-4257-b41a-559146829089', N'ResReportScheduledOvertimePerAgent', N'xxResReportScheduledOvertimePerAgent', N'23', N'Matrix', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'ae085df8-1987-404a-9ffb-4a7ff6108ae8', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056E AS DateTime), CAST(0x00009EFD00AB056E AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'Intraday', N'xxIntraday', N'0018', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'0d95b120-d6d1-4148-aeeb-545dfd780325', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB058C AS DateTime), CAST(0x00009EFD00AB058C AS DateTime), N'7730c397-f7d0-4bbb-93a3-d91d103ef91c', N'MyReport', N'xxOpenMyReport', N'0027', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'158b4223-b491-4592-81c8-54dce1f8451a', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0575 AS DateTime), CAST(0x00009EFD00AB0575 AS DateTime), N'809343a9-57db-4907-a658-9640d8a2eefb', N'ModifyMainShift', N'xxModifyMainShift', N'0024', N'Raptor', 1)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'1f8abe90-9793-4257-b41a-559146829089', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056D AS DateTime), CAST(0x00009EFD00AB056D AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'Reports', N'xxReports', N'0006', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'965aefdf-23a3-4efe-8a89-59cbdcc4a168', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0594 AS DateTime), CAST(0x00009EFD00AB0594 AS DateTime), N'814665c8-c2a3-4422-a0d3-cc399be2fb09', N'CreatePerformanceManagerReport', N'xxCreatePerformanceManagerReport', N'0041', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'01094fa7-01a8-47e5-9be1-5a909eac697b', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056F AS DateTime), CAST(0x00009EFD00AB056F AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'UnderConstruction', N'xxUnderConstruction', N'0048', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'522ca38c-3f9d-477e-9128-5e1e80821d8e', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0576 AS DateTime), CAST(0x00009EFD00AB0576 AS DateTime), N'fc718794-807d-4304-b73e-4478afc37836', N'ModifyPersonRestriction', N'xxModifyPersonRestriction', N'0039', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'6be7078b-63c8-42a0-852f-6b9e90c0ec2e', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0ADD AS DateTime), CAST(0x00009EFD00AB0ADD AS DateTime), N'4398f0f7-9177-4fec-80e6-b92334afdbc4', N'ViewRestrictedScenario', N'xxViewRestrictedScenario', N'0061', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'd2c22fdb-4f05-4177-8c0e-6f6346aca880', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056E AS DateTime), CAST(0x00009EFD00AB056E AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'Budgets', N'xxBudgets', N'0050', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'9558d709-42e7-4eae-aa77-7043ebae9f54', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0585 AS DateTime), CAST(0x00009EFD00AB0585 AS DateTime), N'58493d6f-935a-4187-9598-b182c778f846', N'Scorecards', N'xxManageScorecards', N'0033', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'de4a718f-ed81-43f9-80ac-74dd2fbf3343', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB09DE AS DateTime), CAST(0x00009EFD00AB09DE AS DateTime), N'58493d6f-935a-4187-9598-b182c778f846', N'AuditTrailSettings', N'xxAuditTrailSettings', N'0058', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'c4993972-922d-4dfa-a67e-7bb8feaf57d1', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0583 AS DateTime), CAST(0x00009EFD00AB0583 AS DateTime), N'4398f0f7-9177-4fec-80e6-b92334afdbc4', N'ViewUnpublishedSchedules', N'xxViewUnpublishedSchedules', N'0025', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'ade78695-8a6a-466b-a1b5-7e73f31714ac', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB058C AS DateTime), CAST(0x00009EFD00AB058C AS DateTime), N'e33ef9a4-8081-4433-b69f-93b15c8b68d5', N'Request', N'xxRequests', N'0021', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'6aec118b-978f-488c-b343-811a0167a7ce', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0585 AS DateTime), CAST(0x00009EFD00AB0585 AS DateTime), N'58493d6f-935a-4187-9598-b182c778f846', N'RTA', N'xxManageRTA', N'0032', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'62a5fbb9-066b-48e3-9764-832bfc80bce6', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0594 AS DateTime), CAST(0x00009EFD00AB0594 AS DateTime), N'7730c397-f7d0-4bbb-93a3-d91d103ef91c', N'AbsenceRequests', N'xxCreateAbsenceRequest', N'0030', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'8711d6cc-cdd7-481b-95ad-8491415c3632', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0585 AS DateTime), CAST(0x00009EFD00AB0585 AS DateTime), N'e33ef9a4-8081-4433-b69f-93b15c8b68d5', N'AutomaticScheduling', N'xxAutomaticScheduling', N'0009', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'619638a1-8f31-4021-a396-8c0b36cf065b', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB058C AS DateTime), CAST(0x00009EFD00AB058C AS DateTime), N'ade78695-8a6a-466b-a1b5-7e73f31714ac', N'xxApprove', N'Approve', N'0022', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'fe5c2010-5802-4d04-ab44-8f3456d4005f', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0594 AS DateTime), CAST(0x00009EFD00AB0594 AS DateTime), N'ae085df8-1987-404a-9ffb-4a7ff6108ae8', N'EW', N'xxEarlyWarning', N'0035', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'e33ef9a4-8081-4433-b69f-93b15c8b68d5', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056D AS DateTime), CAST(0x00009EFD00AB056D AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'Scheduler', N'xxOpenSchedulePage', N'0002', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'809343a9-57db-4907-a658-9640d8a2eefb', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0575 AS DateTime), CAST(0x00009EFD00AB0575 AS DateTime), N'fc718794-807d-4304-b73e-4478afc37836', N'ModifyAssignment', N'xxModifyAssignment', N'0014', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'0beb39d9-1eb0-43cd-ba41-9c1e597b5e0a', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB058C AS DateTime), CAST(0x00009EFD00AB058C AS DateTime), N'7730c397-f7d0-4bbb-93a3-d91d103ef91c', N'ASM', N'xxASM', N'0020', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'cf4a9dba-62b9-4976-a653-a0bccf3b18cf', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0955 AS DateTime), CAST(0x00009EFD00AB0955 AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'OnlineReports', N'xxOnlineReports', N'0054', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'4d13d40a-6566-4615-840e-aa28d1215021', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0584 AS DateTime), CAST(0x00009EFD00AB0584 AS DateTime), N'0ee42584-83b1-49f8-ade0-36b0e61b4673', N'ModifyPeopleWithinGroupPage', N'xxModifyPeopleWithinGroupPage', N'0038', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'58493d6f-935a-4187-9598-b182c778f846', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056E AS DateTime), CAST(0x00009EFD00AB056E AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'Options', N'xxOptions', N'0017', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'833cdfef-33a8-40f4-a61f-b2a4d9f0e9df', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0288 AS DateTime), CAST(0x00009EFD00AB0288 AS DateTime), NULL, N'All', N'xxAll', N'0000', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'4398f0f7-9177-4fec-80e6-b92334afdbc4', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056D AS DateTime), CAST(0x00009EFD00AB056D AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'Global', N'xxGlobalFunctions', N'0023', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'9e25158d-7c34-44f7-873d-baa1a9f032df', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0958 AS DateTime), CAST(0x00009EFD00AB0958 AS DateTime), N'cf4a9dba-62b9-4976-a653-a0bccf3b18cf', N'ScheduledTimePerActivityReport', N'xxScheduledTimePerActivityReport', N'0055', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'fbd70982-9cc8-4478-9813-c2433e0227de', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB08D3 AS DateTime), CAST(0x00009EFD00AB08D3 AS DateTime), N'4398f0f7-9177-4fec-80e6-b92334afdbc4', N'ViewConfidential', N'xxViewConfidental', N'0052', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'9d24c44b-4dd4-40e6-8369-c3adc57d9db5', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0584 AS DateTime), CAST(0x00009EFD00AB0584 AS DateTime), N'0ee42584-83b1-49f8-ade0-36b0e61b4673', N'ModifyGroupPage', N'xxModifyGroupPage', N'0037', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'980c61d6-21fe-41ab-95bc-c40337a1040a', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0584 AS DateTime), CAST(0x00009EFD00AB0584 AS DateTime), N'4398f0f7-9177-4fec-80e6-b92334afdbc4', N'ModifyWriteProtectedSchedule', N'xxModifyWriteProtectedSchedule', N'0045', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'814665c8-c2a3-4422-a0d3-cc399be2fb09', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056E AS DateTime), CAST(0x00009EFD00AB056E AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'PerformanceManager', N'xxPerformanceManager', N'0040', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'a9d46b33-47a8-4739-93be-cce75b656876', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB08B8 AS DateTime), CAST(0x00009EFD00AB08B8 AS DateTime), N'7730c397-f7d0-4bbb-93a3-d91d103ef91c', N'ExtendedPreferences', N'xxExtendedPreferences', N'0051', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'67e832fb-6142-44bc-a2d2-cdab2cf90bef', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056D AS DateTime), CAST(0x00009EFD00AB056D AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'Permission', N'xxOpenPermissionPage', N'0008', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'22ce6d90-7ef1-4e57-a37c-cf551ed06417', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB058C AS DateTime), CAST(0x00009EFD00AB058C AS DateTime), N'7730c397-f7d0-4bbb-93a3-d91d103ef91c', N'ShiftCategoryPreferences', N'xxModifyShiftCategoryPreferences', N'0026', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'1c89371d-46a3-4c3a-b5c2-d2a3a20ef60d', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0595 AS DateTime), CAST(0x00009EFD00AB0595 AS DateTime), N'814665c8-c2a3-4422-a0d3-cc399be2fb09', N'ViewPerformanceManagerReport', N'xxViewPerformanceManagerReport', N'0042', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'f08af50a-c92d-4a82-a45e-d396e8657773', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0978 AS DateTime), CAST(0x00009EFD00AB0978 AS DateTime), N'58493d6f-935a-4187-9598-b182c778f846', N'ShiftTradeRequest', N'xxShiftTradeRequest', N'0056', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'cd1ea7d3-4178-4dd3-a1dc-d7a197bac6a1', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0AD7 AS DateTime), CAST(0x00009EFD00AB0AD7 AS DateTime), N'7730c397-f7d0-4bbb-93a3-d91d103ef91c', N'SetPlanningTimeBank', N'xxSetPlanningTimeBank', N'0063', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'51b7a658-75bd-44de-81ff-d7bc6315cd59', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056C AS DateTime), CAST(0x00009EFD00AB056C AS DateTime), NULL, N'Raptor', N'xxOpenRaptorApplication', N'0001', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'7f817b4b-a319-4932-94ce-d81fb7868582', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056E AS DateTime), CAST(0x00009EFD00AB056E AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'Shifts', N'xxShifts', N'0016', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'b86417c8-c239-4227-9ace-d82ed9ec9320', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB057D AS DateTime), CAST(0x00009EFD00AB057D AS DateTime), N'4398f0f7-9177-4fec-80e6-b92334afdbc4', N'ViewSchedules', N'xxViewSchedules', N'0049', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'7730c397-f7d0-4bbb-93a3-d91d103ef91c', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB056F AS DateTime), CAST(0x00009EFD00AB056F AS DateTime), N'51b7a658-75bd-44de-81ff-d7bc6315cd59', N'AgentPortal', N'xxAgentPortal', N'0019', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'681428e2-a51a-47b2-9345-d960d901ddbe', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0594 AS DateTime), CAST(0x00009EFD00AB0594 AS DateTime), N'7730c397-f7d0-4bbb-93a3-d91d103ef91c', N'Scorecard', N'xxOpenScorecard', N'0031', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'8db1ec31-079e-4fee-8869-dee270e16413', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0ABA AS DateTime), CAST(0x00009EFD00AB0ABA AS DateTime), N'7730c397-f7d0-4bbb-93a3-d91d103ef91c', N'ViewSchedulePeriodCalculation', N'xxViewSchedulePeriodCalculation', N'0060', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'e30c909d-e5da-439b-aca4-e98802169a49', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0584 AS DateTime), CAST(0x00009EFD00AB0584 AS DateTime), N'4398f0f7-9177-4fec-80e6-b92334afdbc4', N'ModifyMeetings', N'xxModifyMeetings', N'0043', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'03e363f4-3d0e-4aba-8e37-ebe5278cefb8', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0930 AS DateTime), CAST(0x00009EFD00AB0930 AS DateTime), N'58493d6f-935a-4187-9598-b182c778f846', N'AbsenceRequests', N'xxAbsenceRequest', N'0053', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'e84961fb-63dd-48ee-bc45-f4f37abbca97', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0576 AS DateTime), CAST(0x00009EFD00AB0576 AS DateTime), N'fc718794-807d-4304-b73e-4478afc37836', N'ModifyAbsence', N'xxModifyAbsence', N'0012', N'Raptor', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'c4d87642-1488-4c66-bec9-f514f3c7cebd', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0B01 AS DateTime), CAST(0x00009EFD00AB0B01 AS DateTime), N'1f8abe90-9793-4257-b41a-559146829089', N'ResReportAgentQueueMetrics', N'xxResReportAgentQueueMetrics', N'24', N'Matrix', 0)
INSERT [dbo].[ApplicationFunction] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted]) VALUES (N'a7e19173-c209-4b90-8de2-f7b6df308938', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0584 AS DateTime), CAST(0x00009EFD00AB0584 AS DateTime), N'0ee42584-83b1-49f8-ade0-36b0e61b4673', N'ModifyPersonNameAndPassword', N'xxModifyPersonNameAndPassword', N'0007', N'Raptor', 0)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApplicationRole](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[DescriptionText] [nvarchar](255) NULL,
	[BuiltIn] [bit] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_ApplicationRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
INSERT [dbo].[ApplicationRole] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Name], [DescriptionText], [BuiltIn], [BusinessUnit], [IsDeleted]) VALUES (N'193ad35c-7735-44d7-ac0c-b8eda0011e5f', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0288 AS DateTime), CAST(0x00009EFD00AB0288 AS DateTime), N'_Super Role', N'xxSuperRole', 1, NULL, 0)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApplicationFunctionInRole](
	[ApplicationRole] [uniqueidentifier] NOT NULL,
	[ApplicationFunction] [uniqueidentifier] NOT NULL,
UNIQUE CLUSTERED 
(
	[ApplicationRole] ASC,
	[ApplicationFunction] ASC
)
)
GO
INSERT [dbo].[ApplicationFunctionInRole] ([ApplicationRole], [ApplicationFunction]) VALUES (N'193ad35c-7735-44d7-ac0c-b8eda0011e5f', N'833cdfef-33a8-40f4-a61f-b2a4d9f0e9df')
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MasterActivityCollection](
	[MasterActivity] [uniqueidentifier] NOT NULL,
	[Activity] [uniqueidentifier] NOT NULL
)
GO
CREATE CLUSTERED INDEX [CIX_MasterActivityCollection_MasterActivity] ON [dbo].[MasterActivityCollection] 
(
	[MasterActivity] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GroupingAbsence](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_GroupingAbsence] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Absence](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[DisplayColor] [int] NOT NULL,
	[Tracker] [tinyint] NULL,
	[Priority] [tinyint] NOT NULL,
	[InContractTime] [bit] NOT NULL,
	[Requestable] [bit] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[GroupingAbsence] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[InWorkTime] [bit] NOT NULL,
	[InPaidTime] [bit] NOT NULL,
	[PayrollCode] [nvarchar](20) NULL,
	[Confidential] [int] NOT NULL,
 CONSTRAINT [PK_Absence] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonAbsenceAccount](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Absence] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
),
UNIQUE NONCLUSTERED 
(
	[Person] ASC,
	[Absence] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Account](
	[Id] [uniqueidentifier] NOT NULL,
	[AccountType] [nvarchar](255) NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Extra] [bigint] NOT NULL,
	[Accrued] [bigint] NOT NULL,
	[BalanceIn] [bigint] NOT NULL,
	[LatestCalculatedBalance] [bigint] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[BalanceOut] [bigint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContractSchedule](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_ContractSchedule] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContractScheduleWeek](
	[Id] [uniqueidentifier] NOT NULL,
	[WeekOrder] [int] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_ContractScheduleWeek] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContractScheduleWeekWorkDays](
	[ContractScheduleWeek] [uniqueidentifier] NOT NULL,
	[WorkDay] [bit] NULL,
	[DayOfWeek] [int] NOT NULL,
 CONSTRAINT [PK_ContractScheduleWeekWorkDays] PRIMARY KEY CLUSTERED 
(
	[ContractScheduleWeek] ASC,
	[DayOfWeek] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Contract](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[EmploymentType] [int] NOT NULL,
	[AvgWorkTimePerDay] [bigint] NOT NULL,
	[MaxTimePerWeek] [bigint] NOT NULL,
	[NightlyRest] [bigint] NOT NULL,
	[WeeklyRest] [bigint] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[PositivePeriodWorkTimeTolerance] [bigint] NOT NULL,
	[NegativePeriodWorkTimeTolerance] [bigint] NOT NULL,
	[MinTimeSchedulePeriod] [bigint] NOT NULL,
	[PlanningTimeBankMax] [bigint] NOT NULL,
	[PlanningTimeBankMin] [bigint] NOT NULL,
 CONSTRAINT [PK_Contract] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MultiplicatorDefinitionSetCollection](
	[Contract] [uniqueidentifier] NOT NULL,
	[MultiplicatorDefinitionSet] [uniqueidentifier] NOT NULL,
 CONSTRAINT [UQ_MultiplicatorDefinitionSetCollection] UNIQUE CLUSTERED 
(
	[Contract] ASC,
	[MultiplicatorDefinitionSet] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChildSkill](
	[Skill] [uniqueidentifier] NOT NULL,
	[ParentSkill] [uniqueidentifier] NULL,
 CONSTRAINT [PK_ChildSkill] PRIMARY KEY CLUSTERED 
(
	[Skill] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MultisitePeriodDistribution](
	[Parent] [uniqueidentifier] NOT NULL,
	[Value] [float] NOT NULL,
	[ChildSkill] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_MultisitePeriodDistribution] PRIMARY KEY CLUSTERED 
(
	[Parent] ASC,
	[ChildSkill] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[License](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[XmlString] [nvarchar](4000) NOT NULL,
 CONSTRAINT [PK_License] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KeyPerformanceIndicator](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ResourceKey] [nvarchar](50) NOT NULL,
	[TargetValueType] [int] NOT NULL,
	[DefaultTargetValue] [float] NOT NULL,
	[DefaultMinValue] [float] NOT NULL,
	[DefaultMaxValue] [float] NOT NULL,
	[DefaultBetweenColor] [int] NOT NULL,
	[DefaultLowerThanMinColor] [int] NOT NULL,
	[DefaultHigherThanMaxColor] [int] NOT NULL,
 CONSTRAINT [PK_KeyPerformanceIndicator] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExternalLogOn](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[AcdLogOnMartId] [int] NOT NULL,
	[AcdLogOnAggId] [int] NOT NULL,
	[AcdLogOnOriginalId] [nvarchar](50) NULL,
	[AcdLogOnName] [nvarchar](100) NULL,
	[Active] [bit] NOT NULL,
	[DataSourceId] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_ExternalLogOn] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExtendedPreferenceTemplate](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[DisplayColor] [int] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PartTimePercentage](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[Value] [float] NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_PartTimePercentage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GroupPage](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_GroupPage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[GlobalSettingData](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Key] [nvarchar](255) NOT NULL,
	[SerializedValue] [varbinary](8000) NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_GlobalSettingData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
),
 CONSTRAINT [UQ_Key_BusinessUnit] UNIQUE NONCLUSTERED 
(
	[Key] ASC,
	[BusinessUnit] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DayOffTemplate](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[Flexibility] [bigint] NULL,
	[Anchor] [bigint] NULL,
	[TargetLength] [bigint] NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[DisplayColor] [int] NOT NULL,
	[PayrollCode] [nvarchar](20) NULL,
 CONSTRAINT [PK_DayOffTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AlarmType](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[DisplayColor] [int] NOT NULL,
	[Mode] [int] NOT NULL,
	[ThresholdTime] [bigint] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[StaffingEffect] [float] NOT NULL,
 CONSTRAINT [PK_AlarmType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BudgetGroup](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[DaysPerYear] [int] NOT NULL,
	[TimeZone] [nvarchar](50) NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomShrinkage](
	[Id] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[ShrinkageName] [nvarchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomEfficiencyShrinkage](
	[Id] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[ShrinkageName] [nvarchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BudgetDay](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[BudgetGroup] [uniqueidentifier] NOT NULL,
	[Contractors] [float] NOT NULL,
	[DaysOffPerWeek] [float] NOT NULL,
	[ForecastedHours] [float] NOT NULL,
	[FulltimeEquivalentHours] [float] NOT NULL,
	[OvertimeHours] [float] NOT NULL,
	[Recruitment] [float] NOT NULL,
	[StaffEmployed] [float] NULL,
	[StudentHours] [float] NOT NULL,
	[Day] [datetime] NOT NULL,
	[AttritionRate] [float] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomShrinkageBudget](
	[Parent] [uniqueidentifier] NOT NULL,
	[Value] [float] NOT NULL,
	[CustomShrinkage] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Parent] ASC,
	[CustomShrinkage] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomEfficiencyShrinkageBudget](
	[Parent] [uniqueidentifier] NOT NULL,
	[Value] [float] NOT NULL,
	[CustomEfficiencyShrinkage] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Parent] ASC,
	[CustomEfficiencyShrinkage] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvailableData](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[ApplicationRole] [uniqueidentifier] NOT NULL,
	[AvailableDataRange] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_AvailableData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
INSERT [dbo].[AvailableData] ([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [ApplicationRole], [AvailableDataRange], [IsDeleted]) VALUES (N'1a23761b-40fa-467d-b3df-9e764b038ea8', 1, N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0288 AS DateTime), CAST(0x00009EFD00AB0288 AS DateTime), N'193ad35c-7735-44d7-ac0c-b8eda0011e5f', 5, 0)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvailableUnitsInApplicationRole](
	[AvailableData] [uniqueidentifier] NOT NULL,
	[AvailableBusinessUnit] [uniqueidentifier] NULL
)
GO
CREATE CLUSTERED INDEX [CIX_AvailableUnitsInApplicationRole_AvailableData] ON [dbo].[AvailableUnitsInApplicationRole] 
(
	[AvailableData] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvailablePersonsInApplicationRole](
	[AvailableData] [uniqueidentifier] NOT NULL,
	[AvailablePerson] [uniqueidentifier] NULL
)
GO
CREATE CLUSTERED INDEX [CIX_AvailablePersonsInApplicationRole_AvailableData] ON [dbo].[AvailablePersonsInApplicationRole] 
(
	[AvailableData] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvailabilityRotation](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_AvailabilityRotation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonAvailability](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[StartDay] [int] NOT NULL,
	[Availability] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_PersonAvailability] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvailabilityDay](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Day] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_AvailabilityDay_Parent] ON [dbo].[AvailabilityDay] 
(
	[Parent] ASC
)
INCLUDE ( [Id],
[Day]) 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvailabilityRestriction](
	[Id] [uniqueidentifier] NOT NULL,
	[NotAvailable] [bit] NOT NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[EndTimeMinimum] [bigint] NULL,
	[EndTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditTrailSetting](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[DaysToKeep] [int] NOT NULL,
	[IsRunning] [bit] NOT NULL,
 CONSTRAINT [PK_AuditTrailSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
INSERT [dbo].[AuditTrailSetting] ([Id], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [DaysToKeep], [IsRunning]) VALUES (N'675cdde8-a30a-4bea-b798-36d2db463ef2', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB09CA AS DateTime), CAST(0x00009EFD00AB09CA AS DateTime), 14, 0)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SkillDay](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[TemplateName] [nvarchar](50) NULL,
	[TemplateId] [uniqueidentifier] NOT NULL,
	[DayOfWeek] [int] NULL,
	[VersionNumber] [int] NOT NULL,
	[TemplateReferenceSkill] [uniqueidentifier] NULL,
	[SkillDayDate] [datetime] NOT NULL,
	[Skill] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SkillDay] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
),
 CONSTRAINT [UQ_SkillDay_SkillDayDate] UNIQUE NONCLUSTERED 
(
	[SkillDayDate] ASC,
	[Skill] ASC,
	[Scenario] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SkillDataPeriod](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Value] [float] NOT NULL,
	[Seconds] [float] NOT NULL,
	[MaxOccupancy] [float] NOT NULL,
	[MinOccupancy] [float] NOT NULL,
	[PersonBoundary_Maximum] [int] NOT NULL,
	[PersonBoundary_Minimum] [int] NOT NULL,
	[Shrinkage] [float] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
	[Efficiency] [float] NOT NULL,
	[ManualAgents] [float] NULL,
 CONSTRAINT [PK_SkillDataPeriod] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE CLUSTERED INDEX [CIX_SkillDataPeriod_Parent] ON [dbo].[SkillDataPeriod] 
(
	[Parent] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SkillDayTemplate](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[WeekdayIndex] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[VersionNumber] [int] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SkillDayTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SkillCollection](
	[BudgetGroup] [uniqueidentifier] NOT NULL,
	[Skill] [uniqueidentifier] NOT NULL
)
GO
CREATE CLUSTERED INDEX [CIX_SkillCollection_BudgetGroup] ON [dbo].[SkillCollection] 
(
	[BudgetGroup] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MultisiteDayTemplate](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[WeekdayIndex] [int] NOT NULL,
	[VersionNumber] [int] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_MultisiteDayTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Site](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[TimeZone] [nvarchar](50) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[MaxSeats] [int] NULL,
 CONSTRAINT [PK_Site] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvailableSitesInApplicationRole](
	[AvailableData] [uniqueidentifier] NOT NULL,
	[AvailableSite] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_AvailableSitesInApplicationRole] PRIMARY KEY CLUSTERED 
(
	[AvailableData] ASC,
	[AvailableSite] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonRequest](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[RequestStatus] [int] NOT NULL,
	[Subject] [nvarchar](80) NULL,
	[Message] [nvarchar](2000) NULL,
	[IsDeleted] [bit] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[DenyReason] [nvarchar](300) NOT NULL,
 CONSTRAINT [PK_PersonRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Request](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Request] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShiftTradeRequest](
	[Request] [uniqueidentifier] NOT NULL,
	[ShiftTradeStatus] [int] NOT NULL,
 CONSTRAINT [PK_ShiftTradeRequest] PRIMARY KEY CLUSTERED 
(
	[Request] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShiftTradeSwapDetail](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[PersonFrom] [uniqueidentifier] NOT NULL,
	[PersonTo] [uniqueidentifier] NOT NULL,
	[DateFrom] [datetime] NOT NULL,
	[DateTo] [datetime] NOT NULL,
	[ChecksumFrom] [bigint] NOT NULL,
	[ChecksumTo] [bigint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShiftCategory](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[DisplayColor] [int] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_ShiftCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShiftCategoryJusticeValues](
	[ShiftCategory] [uniqueidentifier] NOT NULL,
	[Value] [int] NULL,
	[DayOfWeek] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ShiftCategory] ASC,
	[DayOfWeek] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Scorecard](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Period] [int] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Scorecard] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KeyPerformanceIndicatorCollection](
	[Scorecard] [uniqueidentifier] NOT NULL,
	[KeyPerformanceIndicator] [uniqueidentifier] NOT NULL,
UNIQUE CLUSTERED 
(
	[Scorecard] ASC,
	[KeyPerformanceIndicator] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchedulePeriod](
	[Id] [uniqueidentifier] NOT NULL,
	[DateFrom] [datetime] NOT NULL,
	[PeriodType] [int] NOT NULL,
	[Number] [int] NOT NULL,
	[AverageWorkTimePerDay] [bigint] NULL,
	[DaysOff] [int] NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[MustHavePreference] [int] NOT NULL,
	[BalanceIn] [bigint] NOT NULL,
	[BalanceOut] [bigint] NOT NULL,
	[Extra] [bigint] NOT NULL,
 CONSTRAINT [PK_SchedulePeriod] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchedulePeriodShiftCategoryLimitation](
	[SchedulePeriod] [uniqueidentifier] NOT NULL,
	[Weekly] [bit] NOT NULL,
	[MaxNumberOf] [int] NOT NULL,
	[ShiftCategory] [uniqueidentifier] NOT NULL,
 CONSTRAINT [UQ_SchedulePeriodShiftCategoryLimitation] UNIQUE CLUSTERED 
(
	[SchedulePeriod] ASC,
	[ShiftCategory] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Note](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[NoteDate] [datetime] NOT NULL,
	[ScheduleNote] [nvarchar](255) NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RuleSetBag](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_RuleSetBag] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RtaStateGroup](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[DefaultStateGroup] [bit] NOT NULL,
	[Available] [bit] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsLogOutState] [bit] NOT NULL,
 CONSTRAINT [PK_RtaStateGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StateGroupActivityAlarm](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Activity] [uniqueidentifier] NULL,
	[StateGroup] [uniqueidentifier] NULL,
	[AlarmType] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_StateGroupActivityAlarm] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RtaState](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[StateCode] [nvarchar](25) NULL,
	[PlatformTypeId] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_RtaState] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rotation](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Rotation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RotationDay](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Day] [int] NOT NULL,
 CONSTRAINT [PK_RotationDay] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_RotationDay_Parent] ON [dbo].[RotationDay] 
(
	[Parent] ASC
)
INCLUDE ( [Id],
[Day]) 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RotationRestriction](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NULL,
	[ShiftCategory] [uniqueidentifier] NULL,
	[DayOffTemplate] [uniqueidentifier] NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[EndTimeMinimum] [bigint] NULL,
	[EndTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AbsenceRequest](
	[Request] [uniqueidentifier] NOT NULL,
	[Absence] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_AbsenceRequest] PRIMARY KEY CLUSTERED 
(
	[Request] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [AuditTrail].[Meeting](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[AuditType] [char](1) NOT NULL,
	[AuditChangeSet] [varchar](255) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NULL,
	[Organizer] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[Subject] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](4000) NOT NULL,
	[Location] [nvarchar](200) NOT NULL,
	[Activity] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[StartTime] [bigint] NOT NULL,
	[EndTime] [bigint] NOT NULL,
	[TimeZone] [nvarchar](200) NOT NULL,
	[OriginalMeetingId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Meeting] PRIMARY KEY NONCLUSTERED 
(
	[AuditId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
CREATE CLUSTERED INDEX [CIX_Meeting] ON [AuditTrail].[Meeting] 
(
	[AuditChangeSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Meeting](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NULL,
	[Organizer] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[Subject] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](2000) NOT NULL,
	[Location] [nvarchar](100) NOT NULL,
	[Activity] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[StartTime] [bigint] NOT NULL,
	[EndTime] [bigint] NOT NULL,
	[TimeZone] [nvarchar](100) NOT NULL,
	[OriginalMeetingId] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecurrentMeetingOption](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[IncrementCount] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecurrentWeeklyMeeting](
	[RecurrentMeetingOption] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RecurrentMeetingOption] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecurrentWeeklyMeetingWeekDays](
	[RecurrentWeeklyMeeting] [uniqueidentifier] NOT NULL,
	[DayOfWeek] [int] NULL
)
GO
CREATE CLUSTERED INDEX [CIX_RecurrentWeeklyMeetingWeekDays_RecurrentWeeklyMeeting] ON [dbo].[RecurrentWeeklyMeetingWeekDays] 
(
	[RecurrentWeeklyMeeting] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecurrentMonthlyByWeekMeeting](
	[RecurrentMeetingOption] [uniqueidentifier] NOT NULL,
	[DayOfWeek] [int] NOT NULL,
	[WeekOfMonth] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RecurrentMeetingOption] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecurrentMonthlyByDayMeeting](
	[RecurrentMeetingOption] [uniqueidentifier] NOT NULL,
	[DayInMonth] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RecurrentMeetingOption] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecurrentDailyMeeting](
	[RecurrentMeetingOption] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RecurrentMeetingOption] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QueueSource](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[QueueMartId] [int] NOT NULL,
	[QueueAggId] [int] NOT NULL,
	[QueueOriginalId] [int] NOT NULL,
	[DataSourceId] [int] NULL,
	[LogObjectName] [nvarchar](50) NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](50) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_QueueSource] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PushMessage](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Message] [nvarchar](255) NOT NULL,
	[Sender] [uniqueidentifier] NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[AllowDialogueReply] [bit] NOT NULL,
	[TranslateMessage] [bit] NULL,
 CONSTRAINT [PK_PushMessage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PushMessageDialogue](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Receiver] [uniqueidentifier] NOT NULL,
	[PushMessage] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsReplied] [bit] NULL,
	[Reply] [nvarchar](255) NULL,
 CONSTRAINT [PK_PushMessageDialogue] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DialogueMessage](
	[Id] [uniqueidentifier] NOT NULL,
	[Created] [datetime] NOT NULL,
	[Text] [nvarchar](255) NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Sender] [uniqueidentifier] NULL,
 CONSTRAINT [PK_DialogueMessage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReplyOptions](
	[id] [uniqueidentifier] NOT NULL,
	[elt] [nvarchar](255) NULL
)
GO
CREATE CLUSTERED INDEX [CIX_ReplyOptions_Id] ON [dbo].[ReplyOptions] 
(
	[id] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PublicNote](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[NoteDate] [datetime] NOT NULL,
	[ScheduleNote] [nvarchar](255) NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_PublicNote] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PreferenceRestrictionTemplate](
	[Id] [uniqueidentifier] NOT NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[EndTimeMinimum] [bigint] NULL,
	[EndTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
	[MustHave] [bit] NOT NULL,
	[ShiftCategory] [uniqueidentifier] NULL,
	[DayOffTemplate] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActivityRestrictionTemplate](
	[Id] [uniqueidentifier] NOT NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[EndTimeMinimum] [bigint] NULL,
	[EndTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
	[Activity] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PreferenceDay](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[RestrictionDate] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[TemplateName] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PreferenceRestriction](
	[Id] [uniqueidentifier] NOT NULL,
	[ShiftCategory] [uniqueidentifier] NULL,
	[DayOffTemplate] [uniqueidentifier] NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[EndTimeMinimum] [bigint] NULL,
	[EndTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
	[MustHave] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActivityRestriction](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NULL,
	[Activity] [uniqueidentifier] NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[EndTimeMinimum] [bigint] NULL,
	[EndTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonWriteProtectionInfo](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[PersonWriteProtectedDate] [datetime] NULL,
 CONSTRAINT [PK_PersonWriteProtectionInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
INSERT [dbo].[PersonWriteProtectionInfo] ([Id], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [PersonWriteProtectedDate]) VALUES (N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'3f0886ab-7b25-4e95-856a-0d726edc2a67', CAST(0x00009EFD00AB0288 AS DateTime), CAST(0x00009EFD00AB0288 AS DateTime), NULL)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonsInPayrollExport](
	[PersonId] [uniqueidentifier] NOT NULL,
	[Person] [uniqueidentifier] NULL,
	[collection_id] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[collection_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonRotation](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[StartDay] [int] NOT NULL,
	[Rotation] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_PersonRotation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Team](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Site] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[IsDeleted] [bit] NOT NULL,
	[Scorecard] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Team] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonPeriod](
	[Id] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[Team] [uniqueidentifier] NOT NULL,
	[Note] [nvarchar](1024) NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[RuleSetBag] [uniqueidentifier] NULL,
	[PartTimePercentage] [uniqueidentifier] NOT NULL,
	[Contract] [uniqueidentifier] NOT NULL,
	[ContractSchedule] [uniqueidentifier] NOT NULL,
	[BudgetGroup] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PersonPeriod] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_PersonPeriod_Parent] ON [dbo].[PersonPeriod] 
(
	[Parent] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonSkill](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Skill] [uniqueidentifier] NOT NULL,
	[Value] [float] NOT NULL,
 CONSTRAINT [PK_PersonSkill] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExternalLogOnCollection](
	[PersonPeriod] [uniqueidentifier] NOT NULL,
	[ExternalLogOn] [uniqueidentifier] NOT NULL,
UNIQUE CLUSTERED 
(
	[PersonPeriod] ASC,
	[ExternalLogOn] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonInApplicationRole](
	[Person] [uniqueidentifier] NOT NULL,
	[ApplicationRole] [uniqueidentifier] NOT NULL,
UNIQUE CLUSTERED 
(
	[Person] ASC,
	[ApplicationRole] ASC
)
)
GO
INSERT [dbo].[PersonInApplicationRole] ([Person], [ApplicationRole]) VALUES (N'3f0886ab-7b25-4e95-856a-0d726edc2a67', N'193ad35c-7735-44d7-ac0c-b8eda0011e5f')
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkShiftRuleSet](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[BaseActivity] [uniqueidentifier] NOT NULL,
	[Category] [uniqueidentifier] NOT NULL,
	[EarlyStart] [bigint] NOT NULL,
	[LateStart] [bigint] NOT NULL,
	[StartSegment] [bigint] NOT NULL,
	[EarlyEnd] [bigint] NOT NULL,
	[LateEnd] [bigint] NOT NULL,
	[EndSegment] [bigint] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[DefaultAccessibility] [int] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[OnlyForRestrictions] [bit] NOT NULL,
 CONSTRAINT [PK_WorkShiftRuleSet] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RuleSetRuleSetBag](
	[RuleSet] [uniqueidentifier] NOT NULL,
	[RuleSetBag] [uniqueidentifier] NOT NULL
)
GO
CREATE CLUSTERED INDEX [CIX_RuleSetRuleSetBag_RuleSet] ON [dbo].[RuleSetRuleSetBag] 
(
	[RuleSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContractTimeLimiter](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[MinimumLength] [bigint] NOT NULL,
	[MaximumLength] [bigint] NOT NULL,
	[LengthSegment] [bigint] NOT NULL,
 CONSTRAINT [PK_ContractTimeLimiter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActivityTimeLimiter](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[TimeLimit] [bigint] NOT NULL,
	[Activity] [uniqueidentifier] NOT NULL,
	[TimeLimitOperator] [int] NOT NULL,
 CONSTRAINT [PK_ActivityTimeLimiter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActivityExtender](
	[Id] [uniqueidentifier] NOT NULL,
	[ExtenderType] [nvarchar](255) NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[ExtendWithActivity] [uniqueidentifier] NOT NULL,
	[MinimumLength] [bigint] NOT NULL,
	[MaximumLength] [bigint] NOT NULL,
	[LengthSegment] [bigint] NOT NULL,
	[OrderIndex] [int] NOT NULL,
	[NumberOfLayers] [tinyint] NULL,
	[AutoPosStartSegment] [bigint] NULL,
	[EarlyStart] [bigint] NULL,
	[LateStart] [bigint] NULL,
	[StartSegment] [bigint] NULL,
	[AutoPosIntervalSegment] [bigint] NULL,
 CONSTRAINT [PK_ActivityExtender] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccessibilityDaysOfWeek](
	[RuleSet] [uniqueidentifier] NOT NULL,
	[DayOfWeek] [int] NULL
)
GO
CREATE CLUSTERED INDEX [CIX_AccessibilityDaysOfWeek_RuleSet] ON [dbo].[AccessibilityDaysOfWeek] 
(
	[RuleSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccessibilityDates](
	[RuleSet] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NULL
)
GO
CREATE CLUSTERED INDEX [CIX_AccessibilityDates_RuleSet] ON [dbo].[AccessibilityDates] 
(
	[RuleSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkloadDayBase](
	[Id] [uniqueidentifier] NOT NULL,
	[WorkloadDate] [datetime] NULL,
	[Workload] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_WorkloadDayBase] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_WorkloadDayBase_Workload] ON [dbo].[WorkloadDayBase] 
(
	[Workload] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OpenHourList](
	[Parent] [uniqueidentifier] NOT NULL,
	[Minimum] [bigint] NOT NULL,
	[Maximum] [bigint] NOT NULL
)
GO
CREATE CLUSTERED INDEX [CIX_OpenHourList_Parent] ON [dbo].[OpenHourList] 
(
	[Parent] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkloadDayTemplate](
	[WorkloadDayBase] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[Parent] [uniqueidentifier] NULL,
	[WeekdayIndex] [int] NOT NULL,
	[VersionNumber] [int] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_WorkloadDayTemplate] PRIMARY KEY CLUSTERED 
(
	[WorkloadDayBase] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkloadDay](
	[WorkloadDayBase] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[TemplateName] [nvarchar](50) NULL,
	[TemplateId] [uniqueidentifier] NOT NULL,
	[DayOfWeek] [int] NULL,
	[VersionNumber] [int] NOT NULL,
	[Workload] [uniqueidentifier] NULL,
	[Annotation] [nvarchar](1000) NULL,
	[UpdatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_WorkloadDay] PRIMARY KEY CLUSTERED 
(
	[WorkloadDayBase] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_WorkloadDay_Parent] ON [dbo].[WorkloadDay] 
(
	[Parent] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QueueSourceCollection](
	[Workload] [uniqueidentifier] NOT NULL,
	[QueueSource] [uniqueidentifier] NOT NULL,
UNIQUE CLUSTERED 
(
	[Workload] ASC,
	[QueueSource] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkflowControlSet](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[AllowedPreferenceActivity] [uniqueidentifier] NULL,
	[SchedulePublishedToDate] [datetime] NULL,
	[WriteProtection] [int] NULL,
	[ShiftTradeTargetTimeFlexibility] [bigint] NULL,
	[ShiftTradeOpenPeriodDaysForwardMinimum] [int] NOT NULL,
	[ShiftTradeOpenPeriodDaysForwardMaximum] [int] NOT NULL,
	[PreferencePeriodFromDate] [datetime] NOT NULL,
	[PreferencePeriodToDate] [datetime] NOT NULL,
	[PreferenceInputFromDate] [datetime] NOT NULL,
	[PreferenceInputToDate] [datetime] NOT NULL,
	[AutoGrantShiftTradeRequest] [bit] NOT NULL,
	[UseShiftCategoryFairness] [bit] NOT NULL,
	[StudentAvailabilityPeriodFromDate] [datetime] NOT NULL,
	[StudentAvailabilityPeriodToDate] [datetime] NOT NULL,
	[StudentAvailabilityInputFromDate] [datetime] NOT NULL,
	[StudentAvailabilityInputToDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkflowControlSetSkills](
	[WorkflowControlSet] [uniqueidentifier] NOT NULL,
	[Skill] [uniqueidentifier] NOT NULL
)
GO
CREATE CLUSTERED INDEX [CIX_WorkflowControlSetSkills_WorkflowControlSet] ON [dbo].[WorkflowControlSetSkills] 
(
	[WorkflowControlSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkflowControlSetAllowedShiftCategories](
	[WorkflowControlSet] [uniqueidentifier] NOT NULL,
	[ShiftCategory] [uniqueidentifier] NOT NULL
)
GO
CREATE CLUSTERED INDEX [CIX_WorkflowControlSetAllowedShiftCategories_WorkflowControlSet] ON [dbo].[WorkflowControlSetAllowedShiftCategories] 
(
	[WorkflowControlSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkflowControlSetAllowedDayOffs](
	[WorkflowControlSet] [uniqueidentifier] NOT NULL,
	[DayOffTemplate] [uniqueidentifier] NOT NULL
)
GO
CREATE CLUSTERED INDEX [CIX_WorkflowControlSetAllowedDayOffs_WorkflowControlSet] ON [dbo].[WorkflowControlSetAllowedDayOffs] 
(
	[WorkflowControlSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AbsenceRequestOpenPeriod](
	[Id] [uniqueidentifier] NOT NULL,
	[PeriodType] [nvarchar](255) NOT NULL,
	[Absence] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OpenMinimum] [datetime] NOT NULL,
	[OpenMaximum] [datetime] NOT NULL,
	[PersonAccountValidator] [int] NOT NULL,
	[StaffingThresholdValidator] [int] NOT NULL,
	[DaysMinimum] [int] NULL,
	[DaysMaximum] [int] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[AbsenceRequestProcess] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ValidatedVolumeDay](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[VolumeDayDate] [datetime] NOT NULL,
	[Workload] [uniqueidentifier] NOT NULL,
	[ValidatedTasks] [float] NULL,
	[ValidatedTaskTime] [bigint] NULL,
	[ValidatedAfterTaskTime] [bigint] NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_ValidatedVolumeDay] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserDetail](
	[Id] [uniqueidentifier] NOT NULL,
	[LastPasswordChange] [datetime] NOT NULL,
	[InvalidAttemptsSequenceStart] [datetime] NOT NULL,
	[IsLocked] [bit] NOT NULL,
	[InvalidAttempts] [int] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
),
UNIQUE NONCLUSTERED 
(
	[Person] ASC
)
)
GO
INSERT [dbo].[UserDetail] ([Id], [LastPasswordChange], [InvalidAttemptsSequenceStart], [IsLocked], [InvalidAttempts], [Person]) VALUES (N'd1a5c71c-b51d-4012-95b4-01028859c14e', CAST(0x00009EFC00AB09E4 AS DateTime), CAST(0x00009EFC00AB09E4 AS DateTime), 0, 0, N'3f0886ab-7b25-4e95-856a-0d726edc2a67')
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TextRequest](
	[Request] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_TextRequest] PRIMARY KEY CLUSTERED 
(
	[Request] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TemplateTaskPeriod](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Tasks] [float] NOT NULL,
	[AverageTaskTime] [bigint] NOT NULL,
	[AverageAfterTaskTime] [bigint] NOT NULL,
	[CampaignTasks] [float] NOT NULL,
	[CampaignTaskTime] [float] NOT NULL,
	[CampaignAfterTaskTime] [float] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
 CONSTRAINT [PK_TemplateTaskPeriod] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE CLUSTERED INDEX [CIX_TemplateTaskPeriod] ON [dbo].[TemplateTaskPeriod] 
(
	[Parent] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TemplateSkillDataPeriod](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Value] [float] NOT NULL,
	[Seconds] [float] NOT NULL,
	[MaxOccupancy] [float] NOT NULL,
	[MinOccupancy] [float] NOT NULL,
	[PersonBoundary_Maximum] [int] NOT NULL,
	[PersonBoundary_Minimum] [int] NOT NULL,
	[Shrinkage] [float] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
	[Efficiency] [float] NOT NULL,
	[ManualAgents] [float] NULL,
 CONSTRAINT [PK_TemplateSkillDataPeriod] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TemplateMultisitePeriod](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_TemplateMultisitePeriod] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TemplateMultisitePeriodDistribution](
	[Parent] [uniqueidentifier] NOT NULL,
	[Value] [float] NOT NULL,
	[ChildSkill] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_TemplateMultisitePeriodDistribution] PRIMARY KEY CLUSTERED 
(
	[Parent] ASC,
	[ChildSkill] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KpiTarget](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[KeyPerformanceIndicator] [uniqueidentifier] NOT NULL,
	[Team] [uniqueidentifier] NOT NULL,
	[TargetValue] [float] NOT NULL,
	[MinValue] [float] NOT NULL,
	[MaxValue] [float] NOT NULL,
	[BetweenColor] [int] NOT NULL,
	[LowerThanMinColor] [int] NOT NULL,
	[HigherThanMaxColor] [int] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_KpiTarget] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_KpiTarget] ON [dbo].[KpiTarget] 
(
	[KeyPerformanceIndicator] ASC,
	[Team] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvailableTeamsInApplicationRole](
	[AvailableData] [uniqueidentifier] NOT NULL,
	[AvailableTeam] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_AvailableTeamsInApplicationRole] PRIMARY KEY CLUSTERED 
(
	[AvailableData] ASC,
	[AvailableTeam] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SystemRoleApplicationRoleMapper](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[SystemRoleLongName] [nvarchar](255) NOT NULL,
	[SystemName] [nvarchar](255) NULL,
	[ApplicationRole] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_SystemRoleApplicationRoleMapper] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StudentAvailabilityDay](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[NotAvailable] [bit] NOT NULL,
	[RestrictionDate] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StudentAvailabilityRestriction](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NULL,
	[RestrictionIndex] [int] NOT NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[EndTimeMinimum] [bigint] NULL,
	[EndTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [AuditTrail].[PersonDayOff](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[AuditType] [char](1) NOT NULL,
	[AuditChangeSet] [varchar](255) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Anchor] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[TargetLength] [bigint] NOT NULL,
	[Flexibility] [bigint] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[ShortName] [nvarchar](50) NULL,
	[DisplayColor] [int] NOT NULL,
	[PayrollCode] [nvarchar](20) NULL,
 CONSTRAINT [PK_PersonDayOff] PRIMARY KEY NONCLUSTERED 
(
	[AuditId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
CREATE CLUSTERED INDEX [CIX_PersonDayOff] ON [AuditTrail].[PersonDayOff] 
(
	[AuditChangeSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonDayOff](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Anchor] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[TargetLength] [bigint] NOT NULL,
	[Flexibility] [bigint] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
	[DisplayColor] [int] NOT NULL,
	[PayrollCode] [nvarchar](20) NULL,
 CONSTRAINT [PK_PersonDayOff] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_queue](
	[date] [datetime] NOT NULL,
	[interval] [nvarchar](50) NOT NULL,
	[queue_code] [int] NULL,
	[queue_name] [nvarchar](50) NOT NULL,
	[offd_direct_call_cnt] [int] NULL,
	[overflow_in_call_cnt] [int] NULL,
	[aband_call_cnt] [int] NULL,
	[overflow_out_call_cnt] [int] NULL,
	[answ_call_cnt] [int] NULL,
	[queued_and_answ_call_dur] [int] NULL,
	[queued_and_aband_call_dur] [int] NULL,
	[talking_call_dur] [int] NULL,
	[wrap_up_dur] [int] NULL,
	[queued_answ_longest_que_dur] [int] NULL,
	[queued_aband_longest_que_dur] [int] NULL,
	[avg_avail_member_cnt] [int] NULL,
	[ans_servicelevel_cnt] [int] NULL,
	[wait_dur] [int] NULL,
	[aband_short_call_cnt] [int] NULL,
	[aband_within_sl_cnt] [int] NULL
)
GO
CREATE CLUSTERED INDEX [CIX_stg_queue_date] ON [stage].[stg_queue] 
(
	[date] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonGroupBase](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShortName] [nvarchar](25) NULL,
 CONSTRAINT [PK_PersonGroupBase] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RootPersonGroup](
	[PersonGroupBase] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_RootPersonGroup] PRIMARY KEY CLUSTERED 
(
	[PersonGroupBase] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonGroup](
	[PersonGroup] [uniqueidentifier] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL
)
GO
CREATE CLUSTERED INDEX [CIX_PersonGroup_PersonGroup] ON [dbo].[PersonGroup] 
(
	[PersonGroup] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[REVINFO](
	[REV] [int] IDENTITY(1,1) NOT NULL,
	[REVTSTMP] [datetime] NULL,
 CONSTRAINT [PK_REVINFO] PRIMARY KEY CLUSTERED 
(
	[REV] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [AuditTrail].[AuditChangeSet](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AuditChangeSet] [varchar](255) NOT NULL,
	[AuditDatetime] [datetime] NOT NULL,
	[AuditOwner] [uniqueidentifier] NOT NULL,
	[AuditStatus] [char](1) NOT NULL,
 CONSTRAINT [PK_AuditChangeSet] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
CREATE CLUSTERED INDEX [CIX_AuditChangeSet] ON [AuditTrail].[AuditChangeSet] 
(
	[AuditChangeSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_queue](
	[queue_id] [int] IDENTITY(1,1) NOT NULL,
	[queue_agg_id] [int] NULL,
	[queue_original_id] [nvarchar](50) NULL,
	[queue_name] [nvarchar](100) NOT NULL,
	[queue_description] [nvarchar](100) NULL,
	[log_object_name] [nvarchar](100) NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_queue] PRIMARY KEY CLUSTERED 
(
	[queue_id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_dim_queue] ON [mart].[dim_queue] 
(
	[queue_original_id] ASC,
	[queue_id] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_interval](
	[interval_id] [smallint] NOT NULL,
	[interval_name] [nvarchar](20) NULL,
	[halfhour_name] [nvarchar](50) NULL,
	[hour_name] [nvarchar](50) NULL,
	[interval_start] [smalldatetime] NULL,
	[interval_end] [smalldatetime] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_interval] PRIMARY KEY CLUSTERED 
(
	[interval_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_date](
	[date_id] [int] IDENTITY(1,1) NOT NULL,
	[date_date] [smalldatetime] NOT NULL,
	[year] [int] NOT NULL,
	[year_month] [int] NOT NULL,
	[month] [int] NOT NULL,
	[month_name] [nvarchar](20) NOT NULL,
	[month_resource_key] [nvarchar](100) NULL,
	[day_in_month] [int] NOT NULL,
	[weekday_number] [int] NOT NULL,
	[weekday_name] [nvarchar](20) NOT NULL,
	[weekday_resource_key] [nvarchar](100) NULL,
	[week_number] [int] NOT NULL,
	[year_week] [nvarchar](6) NOT NULL,
	[quarter] [nvarchar](6) NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_dim_date] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [AuditTrail].[MeetingPerson](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[AuditType] [char](1) NOT NULL,
	[AuditChangeSet] [varchar](255) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Optional] [bit] NOT NULL,
 CONSTRAINT [PK_MeetingPerson] PRIMARY KEY NONCLUSTERED 
(
	[AuditId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
CREATE CLUSTERED INDEX [CIX_MeetingPerson] ON [AuditTrail].[MeetingPerson] 
(
	[AuditChangeSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MeetingPerson](
	[Id] [uniqueidentifier] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[Optional] [bit] NOT NULL,
 CONSTRAINT [PK_MeetingPerson] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [AuditTrail].[MainShift](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[AuditType] [char](1) NOT NULL,
	[AuditChangeSet] [varchar](255) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[ShiftCategory] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_MainShift] PRIMARY KEY NONCLUSTERED 
(
	[AuditId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
CREATE CLUSTERED INDEX [CIX_MainShift] ON [AuditTrail].[MainShift] 
(
	[AuditChangeSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [AuditTrail].[PersonAssignment](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[AuditType] [char](1) NOT NULL,
	[AuditChangeSet] [varchar](255) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
 CONSTRAINT [PK_PersonAssignment] PRIMARY KEY NONCLUSTERED 
(
	[AuditId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
CREATE CLUSTERED INDEX [CIX_PersonAssignment] ON [AuditTrail].[PersonAssignment] 
(
	[AuditChangeSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonAssignment](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
 CONSTRAINT [PK_PersonAssignment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_Person_Scenario] ON [dbo].[PersonAssignment] 
(
	[Person] ASC
)
INCLUDE ( [Scenario])
GO
CREATE NONCLUSTERED INDEX [IX_Scenario_Minimum_Maximum] ON [dbo].[PersonAssignment] 
(
	[Scenario] ASC,
	[Minimum] ASC,
	[Maximum] ASC
)
INCLUDE ( [Id]) 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MainShift](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ShiftCategory] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_MainShift] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_MainShift_ShiftCategory] ON [dbo].[MainShift] 
(
	[ShiftCategory] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [AuditTrail].[MainShiftActivityLayer](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[AuditType] [char](1) NOT NULL,
	[AuditChangeSet] [varchar](255) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[payLoad] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
 CONSTRAINT [PK_MainShiftActivityLayer] PRIMARY KEY NONCLUSTERED 
(
	[AuditId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
CREATE CLUSTERED INDEX [CIX_MainShiftActivityLayer] ON [AuditTrail].[MainShiftActivityLayer] 
(
	[AuditChangeSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MainShiftActivityLayer](
	[Id] [uniqueidentifier] NOT NULL,
	[payLoad] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
 CONSTRAINT [PK_MainShiftActivityLayer] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE CLUSTERED INDEX [CIX_MainShiftActivityLayer_Parent] ON [dbo].[MainShiftActivityLayer] 
(
	[Parent] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [AuditTrail].[PersonalShift](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[AuditType] [char](1) NOT NULL,
	[AuditChangeSet] [varchar](255) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
 CONSTRAINT [PK_PersonalShift] PRIMARY KEY NONCLUSTERED 
(
	[AuditId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
CREATE CLUSTERED INDEX [CIX_PersonalShift] ON [AuditTrail].[PersonalShift] 
(
	[AuditChangeSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonalShift](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
 CONSTRAINT [PK_PersonalShift] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_PersonalShift_Parent] ON [dbo].[PersonalShift] 
(
	[Parent] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PersonalSettingData](
	[Id] [uniqueidentifier] NOT NULL,
	[Key] [nvarchar](255) NOT NULL,
	[SerializedValue] [varbinary](max) NOT NULL,
	[OwnerPerson] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_PersonalSettingData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
),
 CONSTRAINT [UQ_Key_OwnerPerson] UNIQUE NONCLUSTERED 
(
	[Key] ASC,
	[OwnerPerson] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonAccountTime](
	[PersonAccount] [uniqueidentifier] NOT NULL,
	[Accrued] [bigint] NOT NULL,
	[Extra] [bigint] NOT NULL,
	[BalanceIn] [bigint] NOT NULL,
	[LatestCalculatedBalance] [bigint] NOT NULL,
 CONSTRAINT [PK_PersonAccountTime] PRIMARY KEY CLUSTERED 
(
	[PersonAccount] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonAccountDay](
	[PersonAccount] [uniqueidentifier] NOT NULL,
	[Accrued] [int] NOT NULL,
	[Extra] [int] NOT NULL,
	[BalanceIn] [int] NOT NULL,
	[LatestCalculatedBalance] [int] NOT NULL,
 CONSTRAINT [PK_PersonAccountDay] PRIMARY KEY CLUSTERED 
(
	[PersonAccount] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonAccount](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime] NULL,
	[Name] [nvarchar](50) NULL,
	[ShortName] [nvarchar](25) NULL,
	[TrackingAbsence] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PersonAccount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [AuditTrail].[OvertimeShift](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[AuditType] [char](1) NOT NULL,
	[AuditChangeSet] [varchar](255) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
 CONSTRAINT [PK_OvertimeShift] PRIMARY KEY NONCLUSTERED 
(
	[AuditId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
CREATE CLUSTERED INDEX [CIX_OvertimeShift] ON [AuditTrail].[OvertimeShift] 
(
	[AuditChangeSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OvertimeShift](
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [AuditTrail].[OvertimeShiftActivityLayer](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[AuditType] [char](1) NOT NULL,
	[AuditChangeSet] [varchar](255) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[payLoad] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[DefinitionSet] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
 CONSTRAINT [PK_OvertimeShiftActivityLayer] PRIMARY KEY NONCLUSTERED 
(
	[AuditId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
CREATE CLUSTERED INDEX [CIX_OvertimeShiftActivityLayer] ON [AuditTrail].[OvertimeShiftActivityLayer] 
(
	[AuditChangeSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OvertimeShiftActivityLayer](
	[Id] [uniqueidentifier] NOT NULL,
	[payLoad] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[DefinitionSet] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [AuditTrail].[PersonAbsence](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[AuditType] [char](1) NOT NULL,
	[AuditChangeSet] [varchar](255) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[LastChange] [datetime] NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[PayLoad] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_PersonAbsence] PRIMARY KEY NONCLUSTERED 
(
	[AuditId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
CREATE CLUSTERED INDEX [CIX_PersonAbsence] ON [AuditTrail].[PersonAbsence] 
(
	[AuditChangeSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonAbsence](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[LastChange] [datetime] NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[PayLoad] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_PersonAbsence] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MainShiftActivityLayerFix306](
	[Batch] [int] NOT NULL,
	[UpdatedOn] [smalldatetime] NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OrderIndexOld] [int] NOT NULL,
	[OrderIndexNew] [int] NOT NULL,
 CONSTRAINT [PK_MainShiftActivityLayerFix306] PRIMARY KEY CLUSTERED 
(
	[Batch] ASC,
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [AuditTrail].[PersonalShiftActivityLayer](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[AuditType] [char](1) NOT NULL,
	[AuditChangeSet] [varchar](255) NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[payLoad] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
 CONSTRAINT [PK_PersonalShiftActivityLayer] PRIMARY KEY NONCLUSTERED 
(
	[AuditId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
CREATE CLUSTERED INDEX [CIX_PersonalShiftActivityLayer] ON [AuditTrail].[PersonalShiftActivityLayer] 
(
	[AuditChangeSet] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonalShiftActivityLayer](
	[Id] [uniqueidentifier] NOT NULL,
	[payLoad] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
 CONSTRAINT [PK_PersonalShiftActivityLayer] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)
GO
CREATE CLUSTERED INDEX [CIX_PersonalShiftActivityLayer] ON [dbo].[PersonalShiftActivityLayer] 
(
	[Parent] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonAbsence_Backup](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[LastChange] [datetime] NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[PayLoad] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_PersonAbsence_Backup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonAssignment_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[CreatedOn] [datetime] NULL,
	[UpdatedOn] [datetime] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[MainShift] [uniqueidentifier] NULL,
	[Person] [uniqueidentifier] NULL,
	[Scenario] [uniqueidentifier] NULL,
	[BusinessUnit] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PersonAssignment_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonalShiftActivityLayer_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[OrderIndex] [int] NULL,
	[Payload] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PersonalShiftActivityLayer_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonalShift_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[OrderIndex] [int] NULL,
	[Parent] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PersonalShift_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MainShiftActivityLayer_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[OrderIndex] [int] NULL,
	[PayLoad] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL,
 CONSTRAINT [PK_MainShiftActivityLayer_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OvertimeShift_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[OrderIndex] [int] NULL,
	[Parent] [uniqueidentifier] NULL,
 CONSTRAINT [PK_OvertimeShift_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MainShift_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[Name] [nvarchar](50) NULL,
	[RefId] [uniqueidentifier] NULL,
	[ShiftCategory] [uniqueidentifier] NULL,
 CONSTRAINT [PK_MainShift_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OvertimeShiftActivityLayer_AUD](
	[Id] [uniqueidentifier] NOT NULL,
	[REV] [int] NOT NULL,
	[REVTYPE] [tinyint] NULL,
	[Minimum] [datetime] NULL,
	[Maximum] [datetime] NULL,
	[OrderIndex] [int] NULL,
	[Payload] [uniqueidentifier] NULL,
	[DefinitionSet] [uniqueidentifier] NULL,
	[Parent] [uniqueidentifier] NULL,
 CONSTRAINT [PK_OvertimeShiftActivityLayer_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[fact_queue](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[queue_id] [int] NOT NULL,
	[local_date_id] [int] NULL,
	[local_interval_id] [smallint] NULL,
	[offered_calls] [decimal](24, 5) NULL,
	[answered_calls] [decimal](24, 5) NULL,
	[answered_calls_within_SL] [decimal](24, 5) NULL,
	[abandoned_calls] [decimal](24, 5) NULL,
	[abandoned_calls_within_SL] [decimal](24, 5) NULL,
	[abandoned_short_calls] [decimal](18, 0) NULL,
	[overflow_out_calls] [decimal](24, 5) NULL,
	[overflow_in_calls] [decimal](24, 5) NULL,
	[talk_time_s] [decimal](24, 5) NULL,
	[after_call_work_s] [decimal](24, 5) NULL,
	[handle_time_s] [decimal](24, 5) NULL,
	[speed_of_answer_s] [decimal](24, 5) NULL,
	[time_to_abandon_s] [decimal](24, 5) NULL,
	[longest_delay_in_queue_answered_s] [decimal](24, 5) NULL,
	[longest_delay_in_queue_abandoned_s] [decimal](24, 5) NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_fact_queue] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[queue_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChildPersonGroup](
	[PersonGroupBase] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_ChildPersonGroup] PRIMARY KEY CLUSTERED 
(
	[PersonGroupBase] ASC
)
)
GO
ALTER TABLE [AuditTrail].[AuditChangeSet] ADD  CONSTRAINT [DF_AuditChangeSet_AuditStatus]  DEFAULT ((0)) FOR [AuditStatus]
GO
ALTER TABLE [dbo].[Account] ADD  CONSTRAINT [DF_Account_BalanceOut]  DEFAULT ((0)) FOR [BalanceOut]
GO
ALTER TABLE [dbo].[Activity] ADD  CONSTRAINT [DF_Activity_IsMaster]  DEFAULT ((0)) FOR [IsMaster]
GO
ALTER TABLE [dbo].[Activity] ADD  CONSTRAINT [DF_Activity_RequiresSeat]  DEFAULT ((0)) FOR [RequiresSeat]
GO
ALTER TABLE [dbo].[Contract] ADD  CONSTRAINT [DF_Contract_PositivePeriodWorkTimeTolerance]  DEFAULT ((0)) FOR [PositivePeriodWorkTimeTolerance]
GO
ALTER TABLE [dbo].[Contract] ADD  CONSTRAINT [DF_Contract_NegativePeriodWorkTimeTolerance]  DEFAULT ((0)) FOR [NegativePeriodWorkTimeTolerance]
GO
ALTER TABLE [dbo].[Contract] ADD  DEFAULT ((0)) FOR [PlanningTimeBankMax]
GO
ALTER TABLE [dbo].[Contract] ADD  DEFAULT ((0)) FOR [PlanningTimeBankMin]
GO
ALTER TABLE [dbo].[MultisiteDay] ADD  CONSTRAINT [DF_MultisiteDay_UpdatedDate]  DEFAULT ('2001-01-01T00:00:00') FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[MultisiteDayTemplate] ADD  CONSTRAINT [DF_MultisiteDayTemplate_UpdatedDate]  DEFAULT ('2001-01-01T00:00:00') FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[PayrollResult] ADD  DEFAULT ((0)) FOR [FinishedOk]
GO
ALTER TABLE [dbo].[PersonRequest] ADD  CONSTRAINT [DF_PersonRequest_DenyReason]  DEFAULT ('') FOR [DenyReason]
GO
ALTER TABLE [dbo].[PushMessage] ADD  CONSTRAINT [DF_PushMessage_AllowDialogueReply]  DEFAULT ((1)) FOR [AllowDialogueReply]
GO
ALTER TABLE [dbo].[Scenario] ADD  CONSTRAINT [DF_Scenario_EnableReporting]  DEFAULT ((0)) FOR [EnableReporting]
GO
ALTER TABLE [dbo].[Scenario] ADD  CONSTRAINT [DF_Scenario_Restricted]  DEFAULT ((0)) FOR [Restricted]
GO
ALTER TABLE [dbo].[SchedulePeriod] ADD  CONSTRAINT [DF_SchedulePeriod_BalanceIn]  DEFAULT ((0)) FOR [BalanceIn]
GO
ALTER TABLE [dbo].[SchedulePeriod] ADD  CONSTRAINT [DF_SchedulePeriod_BalanceOut]  DEFAULT ((0)) FOR [BalanceOut]
GO
ALTER TABLE [dbo].[SchedulePeriod] ADD  CONSTRAINT [DF_SchedulePeriod_Extra]  DEFAULT ((0)) FOR [Extra]
GO
ALTER TABLE [dbo].[Skill] ADD  CONSTRAINT [DF_Skill_UnderstaffingFor]  DEFAULT ((1)) FOR [UnderstaffingFor]
GO
ALTER TABLE [dbo].[SkillDay] ADD  CONSTRAINT [DF_SkillDay_UpdatedDate]  DEFAULT ('2001-01-01T00:00:00') FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[SkillDayTemplate] ADD  CONSTRAINT [DF_SkillDayTemplate_UpdatedDate]  DEFAULT ('2001-01-01T00:00:00') FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[WorkflowControlSet] ADD  CONSTRAINT [DF_WorkflowControlSet_UseShiftCategoryFairness]  DEFAULT ((0)) FOR [UseShiftCategoryFairness]
GO
ALTER TABLE [dbo].[WorkflowControlSet] ADD  DEFAULT ('1901-01-01T00:00:00') FOR [StudentAvailabilityPeriodFromDate]
GO
ALTER TABLE [dbo].[WorkflowControlSet] ADD  DEFAULT ('2029-01-01T00:00:00') FOR [StudentAvailabilityPeriodToDate]
GO
ALTER TABLE [dbo].[WorkflowControlSet] ADD  DEFAULT ('1901-01-01T00:00:00') FOR [StudentAvailabilityInputFromDate]
GO
ALTER TABLE [dbo].[WorkflowControlSet] ADD  DEFAULT ('2029-01-01T00:00:00') FOR [StudentAvailabilityInputToDate]
GO
ALTER TABLE [dbo].[WorkloadDay] ADD  CONSTRAINT [DF_WorkloadDay_UpdatedDate]  DEFAULT ('2001-01-01T00:00:00') FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[WorkloadDayTemplate] ADD  CONSTRAINT [DF_WorkloadDayTemplate_UpdatedDate]  DEFAULT ('2001-01-01T00:00:00') FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[WorkShiftRuleSet] ADD  CONSTRAINT [DF_WorkShiftRuleSet_OnlyForRestrictions]  DEFAULT ((0)) FOR [OnlyForRestrictions]
GO
ALTER TABLE [mart].[dim_date] ADD  CONSTRAINT [DF_dim_date_inserted]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_queue_name]  DEFAULT ('Not Defined') FOR [queue_name]
GO
ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
GO
ALTER TABLE [mart].[fact_queue] ADD  CONSTRAINT [DF_fact_queue_statistics_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_queue] ADD  CONSTRAINT [DF_fact_queue_statistics_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[fact_queue] ADD  CONSTRAINT [DF_fact_queue_statistics_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[fact_queue] ADD  CONSTRAINT [DF_fact_queue_statistics_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
GO
ALTER TABLE [dbo].[ActivityExtender]  WITH CHECK ADD  CONSTRAINT [CK_ActivityExtender_OrderIndex] CHECK  (([OrderIndex]>=(0)))
GO
ALTER TABLE [dbo].[ActivityExtender] CHECK CONSTRAINT [CK_ActivityExtender_OrderIndex]
GO
ALTER TABLE [dbo].[Absence]  WITH CHECK ADD  CONSTRAINT [FK_Absence_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Absence] CHECK CONSTRAINT [FK_Absence_BusinessUnit]
GO
ALTER TABLE [dbo].[Absence]  WITH CHECK ADD  CONSTRAINT [FK_Absence_GroupingAbsence] FOREIGN KEY([GroupingAbsence])
REFERENCES [dbo].[GroupingAbsence] ([Id])
GO
ALTER TABLE [dbo].[Absence] CHECK CONSTRAINT [FK_Absence_GroupingAbsence]
GO
ALTER TABLE [dbo].[Absence]  WITH CHECK ADD  CONSTRAINT [FK_Absence_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Absence] CHECK CONSTRAINT [FK_Absence_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Absence]  WITH CHECK ADD  CONSTRAINT [FK_Absence_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Absence] CHECK CONSTRAINT [FK_Absence_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[AbsenceRequest]  WITH CHECK ADD  CONSTRAINT [FK_AbsenceRequest_Absence] FOREIGN KEY([Absence])
REFERENCES [dbo].[Absence] ([Id])
GO
ALTER TABLE [dbo].[AbsenceRequest] CHECK CONSTRAINT [FK_AbsenceRequest_Absence]
GO
ALTER TABLE [dbo].[AbsenceRequest]  WITH CHECK ADD  CONSTRAINT [FK_AbsenceRequest_Request] FOREIGN KEY([Request])
REFERENCES [dbo].[Request] ([Id])
GO
ALTER TABLE [dbo].[AbsenceRequest] CHECK CONSTRAINT [FK_AbsenceRequest_Request]
GO
ALTER TABLE [dbo].[AbsenceRequestOpenPeriod]  WITH CHECK ADD  CONSTRAINT [FK_AbsenceRequestOpenPeriod_Absence] FOREIGN KEY([Absence])
REFERENCES [dbo].[Absence] ([Id])
GO
ALTER TABLE [dbo].[AbsenceRequestOpenPeriod] CHECK CONSTRAINT [FK_AbsenceRequestOpenPeriod_Absence]
GO
ALTER TABLE [dbo].[AbsenceRequestOpenPeriod]  WITH CHECK ADD  CONSTRAINT [FK_AbsenceRequestOpenPeriod_WorkflowControlSet] FOREIGN KEY([Parent])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO
ALTER TABLE [dbo].[AbsenceRequestOpenPeriod] CHECK CONSTRAINT [FK_AbsenceRequestOpenPeriod_WorkflowControlSet]
GO
ALTER TABLE [dbo].[AccessibilityDates]  WITH CHECK ADD  CONSTRAINT [FK_AccessibilityDates_WorkShiftRuleSet] FOREIGN KEY([RuleSet])
REFERENCES [dbo].[WorkShiftRuleSet] ([Id])
GO
ALTER TABLE [dbo].[AccessibilityDates] CHECK CONSTRAINT [FK_AccessibilityDates_WorkShiftRuleSet]
GO
ALTER TABLE [dbo].[AccessibilityDaysOfWeek]  WITH CHECK ADD  CONSTRAINT [FK_AccessibilityDaysOfWeek_WorkShiftRuleSet] FOREIGN KEY([RuleSet])
REFERENCES [dbo].[WorkShiftRuleSet] ([Id])
GO
ALTER TABLE [dbo].[AccessibilityDaysOfWeek] CHECK CONSTRAINT [FK_AccessibilityDaysOfWeek_WorkShiftRuleSet]
GO
ALTER TABLE [dbo].[Account]  WITH CHECK ADD  CONSTRAINT [FK_Account_PersonAbsenceAccount] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonAbsenceAccount] ([Id])
GO
ALTER TABLE [dbo].[Account] CHECK CONSTRAINT [FK_Account_PersonAbsenceAccount]
GO
ALTER TABLE [dbo].[Activity]  WITH CHECK ADD  CONSTRAINT [FK_Activity_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Activity] CHECK CONSTRAINT [FK_Activity_BusinessUnit]
GO
ALTER TABLE [dbo].[Activity]  WITH CHECK ADD  CONSTRAINT [FK_Activity_GroupingActivity] FOREIGN KEY([GroupingActivity])
REFERENCES [dbo].[GroupingActivity] ([Id])
GO
ALTER TABLE [dbo].[Activity] CHECK CONSTRAINT [FK_Activity_GroupingActivity]
GO
ALTER TABLE [dbo].[Activity]  WITH CHECK ADD  CONSTRAINT [FK_Activity_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Activity] CHECK CONSTRAINT [FK_Activity_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Activity]  WITH CHECK ADD  CONSTRAINT [FK_Activity_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Activity] CHECK CONSTRAINT [FK_Activity_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[ActivityExtender]  WITH CHECK ADD  CONSTRAINT [FK_ActivityExtender_Activity] FOREIGN KEY([ExtendWithActivity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[ActivityExtender] CHECK CONSTRAINT [FK_ActivityExtender_Activity]
GO
ALTER TABLE [dbo].[ActivityExtender]  WITH CHECK ADD  CONSTRAINT [FK_ActivityExtender_WorkShiftRuleSet] FOREIGN KEY([Parent])
REFERENCES [dbo].[WorkShiftRuleSet] ([Id])
GO
ALTER TABLE [dbo].[ActivityExtender] CHECK CONSTRAINT [FK_ActivityExtender_WorkShiftRuleSet]
GO
ALTER TABLE [dbo].[ActivityRestriction]  WITH CHECK ADD  CONSTRAINT [FK_ActivityRestriction_Activity] FOREIGN KEY([Activity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[ActivityRestriction] CHECK CONSTRAINT [FK_ActivityRestriction_Activity]
GO
ALTER TABLE [dbo].[ActivityRestriction]  WITH CHECK ADD  CONSTRAINT [FK_ActivityRestriction_PreferenceRestriction] FOREIGN KEY([Parent])
REFERENCES [dbo].[PreferenceRestriction] ([Id])
GO
ALTER TABLE [dbo].[ActivityRestriction] CHECK CONSTRAINT [FK_ActivityRestriction_PreferenceRestriction]
GO
ALTER TABLE [dbo].[ActivityRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FK_ActivityRestrictionTemplate_Activity] FOREIGN KEY([Activity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[ActivityRestrictionTemplate] CHECK CONSTRAINT [FK_ActivityRestrictionTemplate_Activity]
GO
ALTER TABLE [dbo].[ActivityRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FK_ActivityRestrictionTemplate_PreferenceRestrictionTemplate] FOREIGN KEY([Id])
REFERENCES [dbo].[PreferenceRestrictionTemplate] ([Id])
GO
ALTER TABLE [dbo].[ActivityRestrictionTemplate] CHECK CONSTRAINT [FK_ActivityRestrictionTemplate_PreferenceRestrictionTemplate]
GO
ALTER TABLE [dbo].[ActivityRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FKDF16F7969265A386] FOREIGN KEY([Parent])
REFERENCES [dbo].[PreferenceRestrictionTemplate] ([Id])
GO
ALTER TABLE [dbo].[ActivityRestrictionTemplate] CHECK CONSTRAINT [FKDF16F7969265A386]
GO
ALTER TABLE [dbo].[ActivityTimeLimiter]  WITH CHECK ADD  CONSTRAINT [FK_ActivityTimeLimiter_Activity] FOREIGN KEY([Activity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[ActivityTimeLimiter] CHECK CONSTRAINT [FK_ActivityTimeLimiter_Activity]
GO
ALTER TABLE [dbo].[ActivityTimeLimiter]  WITH CHECK ADD  CONSTRAINT [FK_ActivityTimeLimiter_WorkShiftRuleSet] FOREIGN KEY([Parent])
REFERENCES [dbo].[WorkShiftRuleSet] ([Id])
GO
ALTER TABLE [dbo].[ActivityTimeLimiter] CHECK CONSTRAINT [FK_ActivityTimeLimiter_WorkShiftRuleSet]
GO
ALTER TABLE [dbo].[AlarmType]  WITH CHECK ADD  CONSTRAINT [FK_AlarmType_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[AlarmType] CHECK CONSTRAINT [FK_AlarmType_BusinessUnit]
GO
ALTER TABLE [dbo].[AlarmType]  WITH CHECK ADD  CONSTRAINT [FK_AlarmType_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[AlarmType] CHECK CONSTRAINT [FK_AlarmType_Person_CreatedBy]
GO
ALTER TABLE [dbo].[AlarmType]  WITH CHECK ADD  CONSTRAINT [FK_AlarmType_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[AlarmType] CHECK CONSTRAINT [FK_AlarmType_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[ApplicationFunction]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationFunction_ApplicationFunction] FOREIGN KEY([Parent])
REFERENCES [dbo].[ApplicationFunction] ([Id])
GO
ALTER TABLE [dbo].[ApplicationFunction] CHECK CONSTRAINT [FK_ApplicationFunction_ApplicationFunction]
GO
ALTER TABLE [dbo].[ApplicationFunction]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationFunction_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ApplicationFunction] CHECK CONSTRAINT [FK_ApplicationFunction_Person_CreatedBy]
GO
ALTER TABLE [dbo].[ApplicationFunction]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationFunction_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ApplicationFunction] CHECK CONSTRAINT [FK_ApplicationFunction_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[ApplicationFunctionInRole]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationFunctionInRole_ApplicationFunction] FOREIGN KEY([ApplicationFunction])
REFERENCES [dbo].[ApplicationFunction] ([Id])
GO
ALTER TABLE [dbo].[ApplicationFunctionInRole] CHECK CONSTRAINT [FK_ApplicationFunctionInRole_ApplicationFunction]
GO
ALTER TABLE [dbo].[ApplicationFunctionInRole]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationFunctionInRole_ApplicationRole] FOREIGN KEY([ApplicationRole])
REFERENCES [dbo].[ApplicationRole] ([Id])
GO
ALTER TABLE [dbo].[ApplicationFunctionInRole] CHECK CONSTRAINT [FK_ApplicationFunctionInRole_ApplicationRole]
GO
ALTER TABLE [dbo].[ApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationRole_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[ApplicationRole] CHECK CONSTRAINT [FK_ApplicationRole_BusinessUnit]
GO
ALTER TABLE [dbo].[ApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationRole_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ApplicationRole] CHECK CONSTRAINT [FK_ApplicationRole_Person_CreatedBy]
GO
ALTER TABLE [dbo].[ApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationRole_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ApplicationRole] CHECK CONSTRAINT [FK_ApplicationRole_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[AuditTrailSetting]  WITH CHECK ADD  CONSTRAINT [FK_AuditTrailSetting_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[AuditTrailSetting] CHECK CONSTRAINT [FK_AuditTrailSetting_Person_CreatedBy]
GO
ALTER TABLE [dbo].[AuditTrailSetting]  WITH CHECK ADD  CONSTRAINT [FK_AuditTrailSetting_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[AuditTrailSetting] CHECK CONSTRAINT [FK_AuditTrailSetting_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[AvailabilityDay]  WITH CHECK ADD  CONSTRAINT [FK_AvailabilityDayNew_Rotation] FOREIGN KEY([Parent])
REFERENCES [dbo].[AvailabilityRotation] ([Id])
GO
ALTER TABLE [dbo].[AvailabilityDay] CHECK CONSTRAINT [FK_AvailabilityDayNew_Rotation]
GO
ALTER TABLE [dbo].[AvailabilityRestriction]  WITH CHECK ADD  CONSTRAINT [FK_AvailabilityRestriction_AvailablilityDay] FOREIGN KEY([Id])
REFERENCES [dbo].[AvailabilityDay] ([Id])
GO
ALTER TABLE [dbo].[AvailabilityRestriction] CHECK CONSTRAINT [FK_AvailabilityRestriction_AvailablilityDay]
GO
ALTER TABLE [dbo].[AvailabilityRotation]  WITH CHECK ADD  CONSTRAINT [FK_Availability_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[AvailabilityRotation] CHECK CONSTRAINT [FK_Availability_BusinessUnit]
GO
ALTER TABLE [dbo].[AvailabilityRotation]  WITH CHECK ADD  CONSTRAINT [FK_Availability_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[AvailabilityRotation] CHECK CONSTRAINT [FK_Availability_Person_CreatedBy]
GO
ALTER TABLE [dbo].[AvailabilityRotation]  WITH CHECK ADD  CONSTRAINT [FK_Availability_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[AvailabilityRotation] CHECK CONSTRAINT [FK_Availability_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[AvailableData]  WITH CHECK ADD  CONSTRAINT [FK_AvailableData_ApplicationRole] FOREIGN KEY([ApplicationRole])
REFERENCES [dbo].[ApplicationRole] ([Id])
GO
ALTER TABLE [dbo].[AvailableData] CHECK CONSTRAINT [FK_AvailableData_ApplicationRole]
GO
ALTER TABLE [dbo].[AvailableData]  WITH CHECK ADD  CONSTRAINT [FK_AvailableData_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[AvailableData] CHECK CONSTRAINT [FK_AvailableData_Person_CreatedBy]
GO
ALTER TABLE [dbo].[AvailableData]  WITH CHECK ADD  CONSTRAINT [FK_AvailableData_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[AvailableData] CHECK CONSTRAINT [FK_AvailableData_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[AvailablePersonsInApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_AvailablePersonsInApplicationRole_AvailableData] FOREIGN KEY([AvailableData])
REFERENCES [dbo].[AvailableData] ([Id])
GO
ALTER TABLE [dbo].[AvailablePersonsInApplicationRole] CHECK CONSTRAINT [FK_AvailablePersonsInApplicationRole_AvailableData]
GO
ALTER TABLE [dbo].[AvailablePersonsInApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_AvailablePersonsInApplicationRole_Person] FOREIGN KEY([AvailablePerson])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[AvailablePersonsInApplicationRole] CHECK CONSTRAINT [FK_AvailablePersonsInApplicationRole_Person]
GO
ALTER TABLE [dbo].[AvailableSitesInApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_AvailableSitesInApplicationRole_AvailableData] FOREIGN KEY([AvailableData])
REFERENCES [dbo].[AvailableData] ([Id])
GO
ALTER TABLE [dbo].[AvailableSitesInApplicationRole] CHECK CONSTRAINT [FK_AvailableSitesInApplicationRole_AvailableData]
GO
ALTER TABLE [dbo].[AvailableSitesInApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_AvailableSitesInApplicationRole_Site] FOREIGN KEY([AvailableSite])
REFERENCES [dbo].[Site] ([Id])
GO
ALTER TABLE [dbo].[AvailableSitesInApplicationRole] CHECK CONSTRAINT [FK_AvailableSitesInApplicationRole_Site]
GO
ALTER TABLE [dbo].[AvailableTeamsInApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_AvailableTeamsInApplicationRole_AvailableData] FOREIGN KEY([AvailableData])
REFERENCES [dbo].[AvailableData] ([Id])
GO
ALTER TABLE [dbo].[AvailableTeamsInApplicationRole] CHECK CONSTRAINT [FK_AvailableTeamsInApplicationRole_AvailableData]
GO
ALTER TABLE [dbo].[AvailableTeamsInApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_AvailableTeamsInApplicationRole_Team] FOREIGN KEY([AvailableTeam])
REFERENCES [dbo].[Team] ([Id])
GO
ALTER TABLE [dbo].[AvailableTeamsInApplicationRole] CHECK CONSTRAINT [FK_AvailableTeamsInApplicationRole_Team]
GO
ALTER TABLE [dbo].[AvailableUnitsInApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_AvailableUnitsInApplicationRole_AvailableData] FOREIGN KEY([AvailableData])
REFERENCES [dbo].[AvailableData] ([Id])
GO
ALTER TABLE [dbo].[AvailableUnitsInApplicationRole] CHECK CONSTRAINT [FK_AvailableUnitsInApplicationRole_AvailableData]
GO
ALTER TABLE [dbo].[AvailableUnitsInApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_AvailableUnitsInApplicationRole_BusinessUnit] FOREIGN KEY([AvailableBusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[AvailableUnitsInApplicationRole] CHECK CONSTRAINT [FK_AvailableUnitsInApplicationRole_BusinessUnit]
GO
ALTER TABLE [dbo].[BudgetDay]  WITH CHECK ADD  CONSTRAINT [FK_BudgetDay_BudgetGroup] FOREIGN KEY([BudgetGroup])
REFERENCES [dbo].[BudgetGroup] ([Id])
GO
ALTER TABLE [dbo].[BudgetDay] CHECK CONSTRAINT [FK_BudgetDay_BudgetGroup]
GO
ALTER TABLE [dbo].[BudgetDay]  WITH CHECK ADD  CONSTRAINT [FK_BudgetDay_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[BudgetDay] CHECK CONSTRAINT [FK_BudgetDay_BusinessUnit]
GO
ALTER TABLE [dbo].[BudgetDay]  WITH CHECK ADD  CONSTRAINT [FK_BudgetDay_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[BudgetDay] CHECK CONSTRAINT [FK_BudgetDay_Person_CreatedBy]
GO
ALTER TABLE [dbo].[BudgetDay]  WITH CHECK ADD  CONSTRAINT [FK_BudgetDay_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[BudgetDay] CHECK CONSTRAINT [FK_BudgetDay_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[BudgetDay]  WITH CHECK ADD  CONSTRAINT [FK_BudgetDay_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
GO
ALTER TABLE [dbo].[BudgetDay] CHECK CONSTRAINT [FK_BudgetDay_Scenario]
GO
ALTER TABLE [dbo].[BudgetGroup]  WITH CHECK ADD  CONSTRAINT [FK_BudgetGroup_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[BudgetGroup] CHECK CONSTRAINT [FK_BudgetGroup_BusinessUnit]
GO
ALTER TABLE [dbo].[BudgetGroup]  WITH CHECK ADD  CONSTRAINT [FK_BudgetGroup_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[BudgetGroup] CHECK CONSTRAINT [FK_BudgetGroup_Person_CreatedBy]
GO
ALTER TABLE [dbo].[BudgetGroup]  WITH CHECK ADD  CONSTRAINT [FK_BudgetGroup_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[BudgetGroup] CHECK CONSTRAINT [FK_BudgetGroup_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[BusinessUnit]  WITH CHECK ADD  CONSTRAINT [FK_BusinessUnit_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[BusinessUnit] CHECK CONSTRAINT [FK_BusinessUnit_Person_CreatedBy]
GO
ALTER TABLE [dbo].[BusinessUnit]  WITH CHECK ADD  CONSTRAINT [FK_BusinessUnit_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[BusinessUnit] CHECK CONSTRAINT [FK_BusinessUnit_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[ChildPersonGroup]  WITH CHECK ADD  CONSTRAINT [FK_ChildPersonGroup_PersonGroupBase1] FOREIGN KEY([PersonGroupBase])
REFERENCES [dbo].[PersonGroupBase] ([Id])
GO
ALTER TABLE [dbo].[ChildPersonGroup] CHECK CONSTRAINT [FK_ChildPersonGroup_PersonGroupBase1]
GO
ALTER TABLE [dbo].[ChildPersonGroup]  WITH CHECK ADD  CONSTRAINT [FK_ChildPersonGroup_PersonGroupBase2] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonGroupBase] ([Id])
GO
ALTER TABLE [dbo].[ChildPersonGroup] CHECK CONSTRAINT [FK_ChildPersonGroup_PersonGroupBase2]
GO
ALTER TABLE [dbo].[ChildSkill]  WITH CHECK ADD  CONSTRAINT [FK_ChildSkill_MultisiteSkill] FOREIGN KEY([ParentSkill])
REFERENCES [dbo].[MultisiteSkill] ([Skill])
GO
ALTER TABLE [dbo].[ChildSkill] CHECK CONSTRAINT [FK_ChildSkill_MultisiteSkill]
GO
ALTER TABLE [dbo].[ChildSkill]  WITH CHECK ADD  CONSTRAINT [FK_ChildSkill_Skill] FOREIGN KEY([Skill])
REFERENCES [dbo].[Skill] ([Id])
GO
ALTER TABLE [dbo].[ChildSkill] CHECK CONSTRAINT [FK_ChildSkill_Skill]
GO
ALTER TABLE [dbo].[Contract]  WITH CHECK ADD  CONSTRAINT [FK_Contract_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Contract] CHECK CONSTRAINT [FK_Contract_BusinessUnit]
GO
ALTER TABLE [dbo].[Contract]  WITH CHECK ADD  CONSTRAINT [FK_Contract_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Contract] CHECK CONSTRAINT [FK_Contract_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Contract]  WITH CHECK ADD  CONSTRAINT [FK_Contract_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Contract] CHECK CONSTRAINT [FK_Contract_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[ContractSchedule]  WITH CHECK ADD  CONSTRAINT [FK_ContractSchedule_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[ContractSchedule] CHECK CONSTRAINT [FK_ContractSchedule_BusinessUnit]
GO
ALTER TABLE [dbo].[ContractSchedule]  WITH CHECK ADD  CONSTRAINT [FK_ContractSchedule_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ContractSchedule] CHECK CONSTRAINT [FK_ContractSchedule_Person_CreatedBy]
GO
ALTER TABLE [dbo].[ContractSchedule]  WITH CHECK ADD  CONSTRAINT [FK_ContractSchedule_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ContractSchedule] CHECK CONSTRAINT [FK_ContractSchedule_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[ContractScheduleWeek]  WITH CHECK ADD  CONSTRAINT [FK_ContractScheduleWeek_ContractSchedule] FOREIGN KEY([Parent])
REFERENCES [dbo].[ContractSchedule] ([Id])
GO
ALTER TABLE [dbo].[ContractScheduleWeek] CHECK CONSTRAINT [FK_ContractScheduleWeek_ContractSchedule]
GO
ALTER TABLE [dbo].[ContractScheduleWeekWorkDays]  WITH CHECK ADD  CONSTRAINT [FK_ContractScheduleWeekWorkDays_ContractScheduleWeek] FOREIGN KEY([ContractScheduleWeek])
REFERENCES [dbo].[ContractScheduleWeek] ([Id])
GO
ALTER TABLE [dbo].[ContractScheduleWeekWorkDays] CHECK CONSTRAINT [FK_ContractScheduleWeekWorkDays_ContractScheduleWeek]
GO
ALTER TABLE [dbo].[ContractTimeLimiter]  WITH CHECK ADD  CONSTRAINT [FK_ContractTimeLimiter_WorkShiftRuleSet] FOREIGN KEY([Parent])
REFERENCES [dbo].[WorkShiftRuleSet] ([Id])
GO
ALTER TABLE [dbo].[ContractTimeLimiter] CHECK CONSTRAINT [FK_ContractTimeLimiter_WorkShiftRuleSet]
GO
ALTER TABLE [dbo].[CustomEfficiencyShrinkage]  WITH CHECK ADD  CONSTRAINT [FK_CustomEfficiencyShrinkage_BudgetGroup] FOREIGN KEY([Parent])
REFERENCES [dbo].[BudgetGroup] ([Id])
GO
ALTER TABLE [dbo].[CustomEfficiencyShrinkage] CHECK CONSTRAINT [FK_CustomEfficiencyShrinkage_BudgetGroup]
GO
ALTER TABLE [dbo].[CustomEfficiencyShrinkageBudget]  WITH CHECK ADD  CONSTRAINT [FK_CustomEfficiencyShrinkageBudget_BudgetDay] FOREIGN KEY([Parent])
REFERENCES [dbo].[BudgetDay] ([Id])
GO
ALTER TABLE [dbo].[CustomEfficiencyShrinkageBudget] CHECK CONSTRAINT [FK_CustomEfficiencyShrinkageBudget_BudgetDay]
GO
ALTER TABLE [dbo].[CustomShrinkage]  WITH CHECK ADD  CONSTRAINT [FK_CustomShrinkage_BudgetGroup] FOREIGN KEY([Parent])
REFERENCES [dbo].[BudgetGroup] ([Id])
GO
ALTER TABLE [dbo].[CustomShrinkage] CHECK CONSTRAINT [FK_CustomShrinkage_BudgetGroup]
GO
ALTER TABLE [dbo].[CustomShrinkageBudget]  WITH CHECK ADD  CONSTRAINT [FK_CustomShrinkageBudget_BudgetDay] FOREIGN KEY([Parent])
REFERENCES [dbo].[BudgetDay] ([Id])
GO
ALTER TABLE [dbo].[CustomShrinkageBudget] CHECK CONSTRAINT [FK_CustomShrinkageBudget_BudgetDay]
GO
ALTER TABLE [dbo].[DayOffTemplate]  WITH CHECK ADD  CONSTRAINT [FK_DayOff_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[DayOffTemplate] CHECK CONSTRAINT [FK_DayOff_BusinessUnit]
GO
ALTER TABLE [dbo].[DayOffTemplate]  WITH CHECK ADD  CONSTRAINT [FK_DayOff_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[DayOffTemplate] CHECK CONSTRAINT [FK_DayOff_Person_CreatedBy]
GO
ALTER TABLE [dbo].[DayOffTemplate]  WITH CHECK ADD  CONSTRAINT [FK_DayOff_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[DayOffTemplate] CHECK CONSTRAINT [FK_DayOff_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[DialogueMessage]  WITH CHECK ADD  CONSTRAINT [FK_DialogueMessage_Person_Sender] FOREIGN KEY([Sender])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[DialogueMessage] CHECK CONSTRAINT [FK_DialogueMessage_Person_Sender]
GO
ALTER TABLE [dbo].[DialogueMessage]  WITH CHECK ADD  CONSTRAINT [FK_PushMessageDialogue_DialogueReply] FOREIGN KEY([Parent])
REFERENCES [dbo].[PushMessageDialogue] ([Id])
GO
ALTER TABLE [dbo].[DialogueMessage] CHECK CONSTRAINT [FK_PushMessageDialogue_DialogueReply]
GO
ALTER TABLE [dbo].[ExtendedPreferenceTemplate]  WITH CHECK ADD  CONSTRAINT [FK_ExtendedPreferenceTemplate_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ExtendedPreferenceTemplate] CHECK CONSTRAINT [FK_ExtendedPreferenceTemplate_Person]
GO
ALTER TABLE [dbo].[ExtendedPreferenceTemplate]  WITH CHECK ADD  CONSTRAINT [FK_ExtendedPreferenceTemplate_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ExtendedPreferenceTemplate] CHECK CONSTRAINT [FK_ExtendedPreferenceTemplate_Person_CreatedBy]
GO
ALTER TABLE [dbo].[ExtendedPreferenceTemplate]  WITH CHECK ADD  CONSTRAINT [FK_ExtendedPreferenceTemplate_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ExtendedPreferenceTemplate] CHECK CONSTRAINT [FK_ExtendedPreferenceTemplate_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[ExternalLogOn]  WITH CHECK ADD  CONSTRAINT [FK_ExternalLogOn_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ExternalLogOn] CHECK CONSTRAINT [FK_ExternalLogOn_Person_CreatedBy]
GO
ALTER TABLE [dbo].[ExternalLogOn]  WITH CHECK ADD  CONSTRAINT [FK_ExternalLogOn_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ExternalLogOn] CHECK CONSTRAINT [FK_ExternalLogOn_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[ExternalLogOnCollection]  WITH CHECK ADD  CONSTRAINT [FK_ExternalLogOnCollection_ExternalLogOn] FOREIGN KEY([ExternalLogOn])
REFERENCES [dbo].[ExternalLogOn] ([Id])
GO
ALTER TABLE [dbo].[ExternalLogOnCollection] CHECK CONSTRAINT [FK_ExternalLogOnCollection_ExternalLogOn]
GO
ALTER TABLE [dbo].[ExternalLogOnCollection]  WITH CHECK ADD  CONSTRAINT [FK_ExternalLogOnCollection_PersonPeriod] FOREIGN KEY([PersonPeriod])
REFERENCES [dbo].[PersonPeriod] ([Id])
GO
ALTER TABLE [dbo].[ExternalLogOnCollection] CHECK CONSTRAINT [FK_ExternalLogOnCollection_PersonPeriod]
GO
ALTER TABLE [dbo].[GlobalSettingData]  WITH CHECK ADD  CONSTRAINT [FK_GlobalSetting_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[GlobalSettingData] CHECK CONSTRAINT [FK_GlobalSetting_Person_CreatedBy]
GO
ALTER TABLE [dbo].[GlobalSettingData]  WITH CHECK ADD  CONSTRAINT [FK_GlobalSetting_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[GlobalSettingData] CHECK CONSTRAINT [FK_GlobalSetting_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[GlobalSettingData]  WITH CHECK ADD  CONSTRAINT [FK_GlobalSettingData_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[GlobalSettingData] CHECK CONSTRAINT [FK_GlobalSettingData_BusinessUnit]
GO
ALTER TABLE [dbo].[GroupingAbsence]  WITH CHECK ADD  CONSTRAINT [FK_GroupingAbsence_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[GroupingAbsence] CHECK CONSTRAINT [FK_GroupingAbsence_Person_CreatedBy]
GO
ALTER TABLE [dbo].[GroupingAbsence]  WITH CHECK ADD  CONSTRAINT [FK_GroupingAbsence_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[GroupingAbsence] CHECK CONSTRAINT [FK_GroupingAbsence_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[GroupingActivity]  WITH CHECK ADD  CONSTRAINT [FK_GroupingActivity_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[GroupingActivity] CHECK CONSTRAINT [FK_GroupingActivity_Person_CreatedBy]
GO
ALTER TABLE [dbo].[GroupingActivity]  WITH CHECK ADD  CONSTRAINT [FK_GroupingActivity_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[GroupingActivity] CHECK CONSTRAINT [FK_GroupingActivity_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[GroupPage]  WITH CHECK ADD  CONSTRAINT [FK_GroupPage_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[GroupPage] CHECK CONSTRAINT [FK_GroupPage_BusinessUnit]
GO
ALTER TABLE [dbo].[GroupPage]  WITH CHECK ADD  CONSTRAINT [FK_GroupPage_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[GroupPage] CHECK CONSTRAINT [FK_GroupPage_Person_CreatedBy]
GO
ALTER TABLE [dbo].[GroupPage]  WITH CHECK ADD  CONSTRAINT [FK_GroupPage_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[GroupPage] CHECK CONSTRAINT [FK_GroupPage_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[KeyPerformanceIndicator]  WITH CHECK ADD  CONSTRAINT [FK_KeyPerformanceIndicator_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[KeyPerformanceIndicator] CHECK CONSTRAINT [FK_KeyPerformanceIndicator_Person_CreatedBy]
GO
ALTER TABLE [dbo].[KeyPerformanceIndicator]  WITH CHECK ADD  CONSTRAINT [FK_KeyPerformanceIndicator_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[KeyPerformanceIndicator] CHECK CONSTRAINT [FK_KeyPerformanceIndicator_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[KeyPerformanceIndicatorCollection]  WITH CHECK ADD  CONSTRAINT [FK_KeyPerformanceIndicator_KeyPerformanceIndicator] FOREIGN KEY([KeyPerformanceIndicator])
REFERENCES [dbo].[KeyPerformanceIndicator] ([Id])
GO
ALTER TABLE [dbo].[KeyPerformanceIndicatorCollection] CHECK CONSTRAINT [FK_KeyPerformanceIndicator_KeyPerformanceIndicator]
GO
ALTER TABLE [dbo].[KeyPerformanceIndicatorCollection]  WITH CHECK ADD  CONSTRAINT [FK_KeyPerformanceIndicatorCollection_Scorecard] FOREIGN KEY([Scorecard])
REFERENCES [dbo].[Scorecard] ([Id])
GO
ALTER TABLE [dbo].[KeyPerformanceIndicatorCollection] CHECK CONSTRAINT [FK_KeyPerformanceIndicatorCollection_Scorecard]
GO
ALTER TABLE [dbo].[KpiTarget]  WITH CHECK ADD  CONSTRAINT [FK_KeyPerformanceIndicatorCollection_KeyPerformanceIndicator] FOREIGN KEY([KeyPerformanceIndicator])
REFERENCES [dbo].[KeyPerformanceIndicator] ([Id])
GO
ALTER TABLE [dbo].[KpiTarget] CHECK CONSTRAINT [FK_KeyPerformanceIndicatorCollection_KeyPerformanceIndicator]
GO
ALTER TABLE [dbo].[KpiTarget]  WITH CHECK ADD  CONSTRAINT [FK_KpiTarget_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[KpiTarget] CHECK CONSTRAINT [FK_KpiTarget_BusinessUnit]
GO
ALTER TABLE [dbo].[KpiTarget]  WITH CHECK ADD  CONSTRAINT [FK_KpiTarget_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[KpiTarget] CHECK CONSTRAINT [FK_KpiTarget_Person_CreatedBy]
GO
ALTER TABLE [dbo].[KpiTarget]  WITH CHECK ADD  CONSTRAINT [FK_KpiTarget_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[KpiTarget] CHECK CONSTRAINT [FK_KpiTarget_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[KpiTarget]  WITH CHECK ADD  CONSTRAINT [FK_KpiTarget_Team] FOREIGN KEY([Team])
REFERENCES [dbo].[Team] ([Id])
GO
ALTER TABLE [dbo].[KpiTarget] CHECK CONSTRAINT [FK_KpiTarget_Team]
GO
ALTER TABLE [dbo].[License]  WITH CHECK ADD  CONSTRAINT [FK_License_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[License] CHECK CONSTRAINT [FK_License_Person_CreatedBy]
GO
ALTER TABLE [dbo].[License]  WITH CHECK ADD  CONSTRAINT [FK_License_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[License] CHECK CONSTRAINT [FK_License_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[MainShift]  WITH CHECK ADD  CONSTRAINT [FK_MainShift_PersonAssignment] FOREIGN KEY([Id])
REFERENCES [dbo].[PersonAssignment] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MainShift] CHECK CONSTRAINT [FK_MainShift_PersonAssignment]
GO
ALTER TABLE [dbo].[MainShift]  WITH CHECK ADD  CONSTRAINT [FK_MainShift_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO
ALTER TABLE [dbo].[MainShift] CHECK CONSTRAINT [FK_MainShift_ShiftCategory]
GO
ALTER TABLE [dbo].[MainShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_MainShiftAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])
GO
ALTER TABLE [dbo].[MainShift_AUD] CHECK CONSTRAINT [FK_MainShiftAUD_REV]
GO
ALTER TABLE [dbo].[MainShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_MainShiftActivityLayer_Activity] FOREIGN KEY([payLoad])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[MainShiftActivityLayer] CHECK CONSTRAINT [FK_MainShiftActivityLayer_Activity]
GO
ALTER TABLE [dbo].[MainShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_MainShiftActivityLayer_MainShift] FOREIGN KEY([Parent])
REFERENCES [dbo].[MainShift] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MainShiftActivityLayer] CHECK CONSTRAINT [FK_MainShiftActivityLayer_MainShift]
GO
ALTER TABLE [dbo].[MainShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_MainShiftActivityLayerAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])
GO
ALTER TABLE [dbo].[MainShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_MainShiftActivityLayerAUD_REV]
GO
ALTER TABLE [dbo].[MasterActivityCollection]  WITH CHECK ADD  CONSTRAINT [FK_MasterActivity_Activity] FOREIGN KEY([Activity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[MasterActivityCollection] CHECK CONSTRAINT [FK_MasterActivity_Activity]
GO
ALTER TABLE [dbo].[MasterActivityCollection]  WITH CHECK ADD  CONSTRAINT [FK_MasterActivity_Master] FOREIGN KEY([MasterActivity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[MasterActivityCollection] CHECK CONSTRAINT [FK_MasterActivity_Master]
GO
ALTER TABLE [dbo].[Meeting]  WITH CHECK ADD  CONSTRAINT [FK_Meeting_Activity] FOREIGN KEY([Activity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[Meeting] CHECK CONSTRAINT [FK_Meeting_Activity]
GO
ALTER TABLE [dbo].[Meeting]  WITH CHECK ADD  CONSTRAINT [FK_Meeting_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Meeting] CHECK CONSTRAINT [FK_Meeting_BusinessUnit]
GO
ALTER TABLE [dbo].[Meeting]  WITH CHECK ADD  CONSTRAINT [FK_Meeting_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Meeting] CHECK CONSTRAINT [FK_Meeting_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Meeting]  WITH CHECK ADD  CONSTRAINT [FK_Meeting_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Meeting] CHECK CONSTRAINT [FK_Meeting_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[Meeting]  WITH CHECK ADD  CONSTRAINT [FK_Meeting_Person3] FOREIGN KEY([Organizer])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Meeting] CHECK CONSTRAINT [FK_Meeting_Person3]
GO
ALTER TABLE [dbo].[Meeting]  WITH CHECK ADD  CONSTRAINT [FK_Meeting_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
GO
ALTER TABLE [dbo].[Meeting] CHECK CONSTRAINT [FK_Meeting_Scenario]
GO
ALTER TABLE [dbo].[MeetingPerson]  WITH CHECK ADD  CONSTRAINT [FK_MeetingPerson_Meeting] FOREIGN KEY([Parent])
REFERENCES [dbo].[Meeting] ([Id])
GO
ALTER TABLE [dbo].[MeetingPerson] CHECK CONSTRAINT [FK_MeetingPerson_Meeting]
GO
ALTER TABLE [dbo].[MeetingPerson]  WITH CHECK ADD  CONSTRAINT [FK_MeetingPerson_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[MeetingPerson] CHECK CONSTRAINT [FK_MeetingPerson_Person]
GO
ALTER TABLE [dbo].[Multiplicator]  WITH CHECK ADD  CONSTRAINT [FK_Multiplicator_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Multiplicator] CHECK CONSTRAINT [FK_Multiplicator_BusinessUnit]
GO
ALTER TABLE [dbo].[Multiplicator]  WITH CHECK ADD  CONSTRAINT [FK_Multiplicator_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Multiplicator] CHECK CONSTRAINT [FK_Multiplicator_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Multiplicator]  WITH CHECK ADD  CONSTRAINT [FK_Multiplicator_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Multiplicator] CHECK CONSTRAINT [FK_Multiplicator_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[MultiplicatorDefinition]  WITH CHECK ADD  CONSTRAINT [FK_Multiplicator_MultiplicatorDefinition] FOREIGN KEY([Multiplicator])
REFERENCES [dbo].[Multiplicator] ([Id])
GO
ALTER TABLE [dbo].[MultiplicatorDefinition] CHECK CONSTRAINT [FK_Multiplicator_MultiplicatorDefinition]
GO
ALTER TABLE [dbo].[MultiplicatorDefinition]  WITH CHECK ADD  CONSTRAINT [FK_MultiplicatorDefinition_MultiplicatorDefinitionSet] FOREIGN KEY([Parent])
REFERENCES [dbo].[MultiplicatorDefinitionSet] ([Id])
GO
ALTER TABLE [dbo].[MultiplicatorDefinition] CHECK CONSTRAINT [FK_MultiplicatorDefinition_MultiplicatorDefinitionSet]
GO
ALTER TABLE [dbo].[MultiplicatorDefinitionSet]  WITH CHECK ADD  CONSTRAINT [FK_MultiplicatorDefinitionSet_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[MultiplicatorDefinitionSet] CHECK CONSTRAINT [FK_MultiplicatorDefinitionSet_BusinessUnit]
GO
ALTER TABLE [dbo].[MultiplicatorDefinitionSet]  WITH CHECK ADD  CONSTRAINT [FK_MultiplicatorDefinitionSet_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[MultiplicatorDefinitionSet] CHECK CONSTRAINT [FK_MultiplicatorDefinitionSet_Person_CreatedBy]
GO
ALTER TABLE [dbo].[MultiplicatorDefinitionSet]  WITH CHECK ADD  CONSTRAINT [FK_MultiplicatorDefinitionSet_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[MultiplicatorDefinitionSet] CHECK CONSTRAINT [FK_MultiplicatorDefinitionSet_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[MultiplicatorDefinitionSetCollection]  WITH CHECK ADD  CONSTRAINT [FK_MultiplicatorDefinitionSet_Contract] FOREIGN KEY([Contract])
REFERENCES [dbo].[Contract] ([Id])
GO
ALTER TABLE [dbo].[MultiplicatorDefinitionSetCollection] CHECK CONSTRAINT [FK_MultiplicatorDefinitionSet_Contract]
GO
ALTER TABLE [dbo].[MultiplicatorDefinitionSetCollection]  WITH CHECK ADD  CONSTRAINT [FK_MultiplicatorDefinitionSetCollection_Contract] FOREIGN KEY([MultiplicatorDefinitionSet])
REFERENCES [dbo].[MultiplicatorDefinitionSet] ([Id])
GO
ALTER TABLE [dbo].[MultiplicatorDefinitionSetCollection] CHECK CONSTRAINT [FK_MultiplicatorDefinitionSetCollection_Contract]
GO
ALTER TABLE [dbo].[MultisiteDay]  WITH CHECK ADD  CONSTRAINT [FK_MultisiteDay_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[MultisiteDay] CHECK CONSTRAINT [FK_MultisiteDay_BusinessUnit]
GO
ALTER TABLE [dbo].[MultisiteDay]  WITH CHECK ADD  CONSTRAINT [FK_MultisiteDay_MultisiteSkill] FOREIGN KEY([Skill])
REFERENCES [dbo].[Skill] ([Id])
GO
ALTER TABLE [dbo].[MultisiteDay] CHECK CONSTRAINT [FK_MultisiteDay_MultisiteSkill]
GO
ALTER TABLE [dbo].[MultisiteDay]  WITH CHECK ADD  CONSTRAINT [FK_MultisiteDay_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[MultisiteDay] CHECK CONSTRAINT [FK_MultisiteDay_Person_CreatedBy]
GO
ALTER TABLE [dbo].[MultisiteDay]  WITH CHECK ADD  CONSTRAINT [FK_MultisiteDay_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[MultisiteDay] CHECK CONSTRAINT [FK_MultisiteDay_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[MultisiteDay]  WITH CHECK ADD  CONSTRAINT [FK_MultisiteDay_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
GO
ALTER TABLE [dbo].[MultisiteDay] CHECK CONSTRAINT [FK_MultisiteDay_Scenario]
GO
ALTER TABLE [dbo].[MultisiteDay]  WITH CHECK ADD  CONSTRAINT [FK_MultisiteDayTemplateReference_Skill] FOREIGN KEY([TemplateReferenceSkill])
REFERENCES [dbo].[MultisiteSkill] ([Skill])
GO
ALTER TABLE [dbo].[MultisiteDay] CHECK CONSTRAINT [FK_MultisiteDayTemplateReference_Skill]
GO
ALTER TABLE [dbo].[MultisiteDayTemplate]  WITH CHECK ADD  CONSTRAINT [FK_MultisiteDayTemplate_Skill] FOREIGN KEY([Parent])
REFERENCES [dbo].[Skill] ([Id])
GO
ALTER TABLE [dbo].[MultisiteDayTemplate] CHECK CONSTRAINT [FK_MultisiteDayTemplate_Skill]
GO
ALTER TABLE [dbo].[MultisitePeriod]  WITH CHECK ADD  CONSTRAINT [FK_MultisitePeriod_MultisiteDay] FOREIGN KEY([Parent])
REFERENCES [dbo].[MultisiteDay] ([Id])
GO
ALTER TABLE [dbo].[MultisitePeriod] CHECK CONSTRAINT [FK_MultisitePeriod_MultisiteDay]
GO
ALTER TABLE [dbo].[MultisitePeriodDistribution]  WITH CHECK ADD  CONSTRAINT [FK_MultisitePeriodDistribution_ChildSkill] FOREIGN KEY([ChildSkill])
REFERENCES [dbo].[ChildSkill] ([Skill])
GO
ALTER TABLE [dbo].[MultisitePeriodDistribution] CHECK CONSTRAINT [FK_MultisitePeriodDistribution_ChildSkill]
GO
ALTER TABLE [dbo].[MultisitePeriodDistribution]  WITH CHECK ADD  CONSTRAINT [FK_MultisitePeriodDistribution_MultisitePeriod] FOREIGN KEY([Parent])
REFERENCES [dbo].[MultisitePeriod] ([Id])
GO
ALTER TABLE [dbo].[MultisitePeriodDistribution] CHECK CONSTRAINT [FK_MultisitePeriodDistribution_MultisitePeriod]
GO
ALTER TABLE [dbo].[MultisiteSkill]  WITH CHECK ADD  CONSTRAINT [FK_MultisiteSkill_Skill] FOREIGN KEY([Skill])
REFERENCES [dbo].[Skill] ([Id])
GO
ALTER TABLE [dbo].[MultisiteSkill] CHECK CONSTRAINT [FK_MultisiteSkill_Skill]
GO
ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_Note_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_Note_BusinessUnit]
GO
ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_Note_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_Note_Person]
GO
ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_Note_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_Note_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_Note_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_Note_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[Note]  WITH CHECK ADD  CONSTRAINT [FK_Note_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
GO
ALTER TABLE [dbo].[Note] CHECK CONSTRAINT [FK_Note_Scenario]
GO
ALTER TABLE [dbo].[OpenHourList]  WITH CHECK ADD  CONSTRAINT [FK_OpenHourList_WorkloadDayBase] FOREIGN KEY([Parent])
REFERENCES [dbo].[WorkloadDayBase] ([Id])
GO
ALTER TABLE [dbo].[OpenHourList] CHECK CONSTRAINT [FK_OpenHourList_WorkloadDayBase]
GO
ALTER TABLE [dbo].[OptionalColumn]  WITH CHECK ADD  CONSTRAINT [FK_OptionalColumn_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[OptionalColumn] CHECK CONSTRAINT [FK_OptionalColumn_BusinessUnit]
GO
ALTER TABLE [dbo].[OptionalColumn]  WITH CHECK ADD  CONSTRAINT [FK_OptionalColumn_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[OptionalColumn] CHECK CONSTRAINT [FK_OptionalColumn_Person_CreatedBy]
GO
ALTER TABLE [dbo].[OptionalColumn]  WITH CHECK ADD  CONSTRAINT [FK_OptionalColumn_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[OptionalColumn] CHECK CONSTRAINT [FK_OptionalColumn_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[OptionalColumnValue]  WITH CHECK ADD  CONSTRAINT [FK_OptionalColumnValue_OptionalColumn] FOREIGN KEY([Parent])
REFERENCES [dbo].[OptionalColumn] ([Id])
GO
ALTER TABLE [dbo].[OptionalColumnValue] CHECK CONSTRAINT [FK_OptionalColumnValue_OptionalColumn]
GO
ALTER TABLE [dbo].[Outlier]  WITH CHECK ADD  CONSTRAINT [FK_Outlier_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Outlier] CHECK CONSTRAINT [FK_Outlier_BusinessUnit]
GO
ALTER TABLE [dbo].[Outlier]  WITH CHECK ADD  CONSTRAINT [FK_Outlier_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Outlier] CHECK CONSTRAINT [FK_Outlier_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Outlier]  WITH CHECK ADD  CONSTRAINT [FK_Outlier_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Outlier] CHECK CONSTRAINT [FK_Outlier_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[Outlier]  WITH CHECK ADD  CONSTRAINT [FK_Outlier_Workload] FOREIGN KEY([Workload])
REFERENCES [dbo].[Workload] ([Id])
GO
ALTER TABLE [dbo].[Outlier] CHECK CONSTRAINT [FK_Outlier_Workload]
GO
ALTER TABLE [dbo].[OutlierDateProviderBase]  WITH CHECK ADD  CONSTRAINT [FK_OutlierDateProviders_Outlier] FOREIGN KEY([Parent])
REFERENCES [dbo].[Outlier] ([Id])
GO
ALTER TABLE [dbo].[OutlierDateProviderBase] CHECK CONSTRAINT [FK_OutlierDateProviders_Outlier]
GO
ALTER TABLE [dbo].[OutlierDates]  WITH CHECK ADD  CONSTRAINT [FK_OutlierDates_Outlier] FOREIGN KEY([Parent])
REFERENCES [dbo].[Outlier] ([Id])
GO
ALTER TABLE [dbo].[OutlierDates] CHECK CONSTRAINT [FK_OutlierDates_Outlier]
GO
ALTER TABLE [dbo].[OvertimeShift]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShift_PersonAssignment] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonAssignment] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OvertimeShift] CHECK CONSTRAINT [FK_OvertimeShift_PersonAssignment]
GO
ALTER TABLE [dbo].[OvertimeShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])
GO
ALTER TABLE [dbo].[OvertimeShift_AUD] CHECK CONSTRAINT [FK_OvertimeShiftAUD_REV]
GO
ALTER TABLE [dbo].[OvertimeShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayer_Activity] FOREIGN KEY([payLoad])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[OvertimeShiftActivityLayer] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayer_Activity]
GO
ALTER TABLE [dbo].[OvertimeShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayer_DefinitionSet] FOREIGN KEY([DefinitionSet])
REFERENCES [dbo].[MultiplicatorDefinitionSet] ([Id])
GO
ALTER TABLE [dbo].[OvertimeShiftActivityLayer] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayer_DefinitionSet]
GO
ALTER TABLE [dbo].[OvertimeShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayer_OvertimeShift] FOREIGN KEY([Parent])
REFERENCES [dbo].[OvertimeShift] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OvertimeShiftActivityLayer] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayer_OvertimeShift]
GO
ALTER TABLE [dbo].[OvertimeShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayerAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])
GO
ALTER TABLE [dbo].[OvertimeShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayerAUD_REV]
GO
ALTER TABLE [dbo].[PartTimePercentage]  WITH CHECK ADD  CONSTRAINT [FK_PartTimePercentage_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PartTimePercentage] CHECK CONSTRAINT [FK_PartTimePercentage_BusinessUnit]
GO
ALTER TABLE [dbo].[PartTimePercentage]  WITH CHECK ADD  CONSTRAINT [FK_PartTimePercentage_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PartTimePercentage] CHECK CONSTRAINT [FK_PartTimePercentage_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PartTimePercentage]  WITH CHECK ADD  CONSTRAINT [FK_PartTimePercentage_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PartTimePercentage] CHECK CONSTRAINT [FK_PartTimePercentage_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PayrollExport]  WITH CHECK ADD  CONSTRAINT [FK_PayrollExport_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PayrollExport] CHECK CONSTRAINT [FK_PayrollExport_BusinessUnit]
GO
ALTER TABLE [dbo].[PayrollExport]  WITH CHECK ADD  CONSTRAINT [FK_PayrollExport_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PayrollExport] CHECK CONSTRAINT [FK_PayrollExport_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PayrollExport]  WITH CHECK ADD  CONSTRAINT [FK_PayrollExport_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PayrollExport] CHECK CONSTRAINT [FK_PayrollExport_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PayrollResult]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResult_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PayrollResult] CHECK CONSTRAINT [FK_PayrollResult_BusinessUnit]
GO
ALTER TABLE [dbo].[PayrollResult]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResult_PayrollExport] FOREIGN KEY([PayrollExport])
REFERENCES [dbo].[PayrollExport] ([Id])
GO
ALTER TABLE [dbo].[PayrollResult] CHECK CONSTRAINT [FK_PayrollResult_PayrollExport]
GO
ALTER TABLE [dbo].[PayrollResult]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResult_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PayrollResult] CHECK CONSTRAINT [FK_PayrollResult_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PayrollResult]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResult_Person_Owner] FOREIGN KEY([Owner])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PayrollResult] CHECK CONSTRAINT [FK_PayrollResult_Person_Owner]
GO
ALTER TABLE [dbo].[PayrollResult]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResult_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PayrollResult] CHECK CONSTRAINT [FK_PayrollResult_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PayrollResult]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResult_XmlResult] FOREIGN KEY([XmlResult])
REFERENCES [dbo].[XmlResult] ([Id])
GO
ALTER TABLE [dbo].[PayrollResult] CHECK CONSTRAINT [FK_PayrollResult_XmlResult]
GO
ALTER TABLE [dbo].[PayrollResultDetail]  WITH CHECK ADD  CONSTRAINT [FK_PayrollResultDetail_PayrollResult] FOREIGN KEY([Parent])
REFERENCES [dbo].[PayrollResult] ([Id])
GO
ALTER TABLE [dbo].[PayrollResultDetail] CHECK CONSTRAINT [FK_PayrollResultDetail_PayrollResult]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_Person_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_Person_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_Person_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_Person_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_Person_WorkflowControlSet] FOREIGN KEY([WorkflowControlSet])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_Person_WorkflowControlSet]
GO
ALTER TABLE [dbo].[Person]  WITH CHECK ADD  CONSTRAINT [FK_Person_WriteProtection_UpdatedBy] FOREIGN KEY([WriteProtectionUpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Person] CHECK CONSTRAINT [FK_Person_WriteProtection_UpdatedBy]
GO
ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Absence] FOREIGN KEY([PayLoad])
REFERENCES [dbo].[Absence] ([Id])
GO
ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Absence]
GO
ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_BusinessUnit]
GO
ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Person3] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Person3]
GO
ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
GO
ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Scenario]
GO
ALTER TABLE [dbo].[PersonAbsenceAccount]  WITH CHECK ADD  CONSTRAINT [FK_Absence_PersonAbsenceAccount] FOREIGN KEY([Absence])
REFERENCES [dbo].[Absence] ([Id])
GO
ALTER TABLE [dbo].[PersonAbsenceAccount] CHECK CONSTRAINT [FK_Absence_PersonAbsenceAccount]
GO
ALTER TABLE [dbo].[PersonAbsenceAccount]  WITH CHECK ADD  CONSTRAINT [FK_Person_PersonAbsenceAccount] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAbsenceAccount] CHECK CONSTRAINT [FK_Person_PersonAbsenceAccount]
GO
ALTER TABLE [dbo].[PersonAbsenceAccount]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsenceAccount_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAbsenceAccount] CHECK CONSTRAINT [FK_PersonAbsenceAccount_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PersonAbsenceAccount]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsenceAccount_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAbsenceAccount] CHECK CONSTRAINT [FK_PersonAbsenceAccount_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PersonalShift]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShift_PersonAssignment] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonAssignment] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PersonalShift] CHECK CONSTRAINT [FK_PersonalShift_PersonAssignment]
GO
ALTER TABLE [dbo].[PersonalShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShiftAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])
GO
ALTER TABLE [dbo].[PersonalShift_AUD] CHECK CONSTRAINT [FK_PersonalShiftAUD_REV]
GO
ALTER TABLE [dbo].[PersonalShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShiftActivityLayer_Activity] FOREIGN KEY([payLoad])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[PersonalShiftActivityLayer] CHECK CONSTRAINT [FK_PersonalShiftActivityLayer_Activity]
GO
ALTER TABLE [dbo].[PersonalShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShiftActivityLayer_PersonalShift] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonalShift] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PersonalShiftActivityLayer] CHECK CONSTRAINT [FK_PersonalShiftActivityLayer_PersonalShift]
GO
ALTER TABLE [dbo].[PersonalShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShiftActivityLayerAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])
GO
ALTER TABLE [dbo].[PersonalShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_PersonalShiftActivityLayerAUD_REV]
GO
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PersonAssignment] CHECK CONSTRAINT [FK_PersonAssignment_BusinessUnit]
GO
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAssignment] CHECK CONSTRAINT [FK_PersonAssignment_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAssignment] CHECK CONSTRAINT [FK_PersonAssignment_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Person3] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAssignment] CHECK CONSTRAINT [FK_PersonAssignment_Person3]
GO
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
GO
ALTER TABLE [dbo].[PersonAssignment] CHECK CONSTRAINT [FK_PersonAssignment_Scenario]
GO
ALTER TABLE [dbo].[PersonAssignment_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignmentAUD_REV] FOREIGN KEY([REV])
REFERENCES [dbo].[REVINFO] ([REV])
GO
ALTER TABLE [dbo].[PersonAssignment_AUD] CHECK CONSTRAINT [FK_PersonAssignmentAUD_REV]
GO
ALTER TABLE [dbo].[PersonAvailability]  WITH CHECK ADD  CONSTRAINT [FK_PersonAvailability_Availability] FOREIGN KEY([Availability])
REFERENCES [dbo].[AvailabilityRotation] ([Id])
GO
ALTER TABLE [dbo].[PersonAvailability] CHECK CONSTRAINT [FK_PersonAvailability_Availability]
GO
ALTER TABLE [dbo].[PersonAvailability]  WITH CHECK ADD  CONSTRAINT [FK_PersonAvailability_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PersonAvailability] CHECK CONSTRAINT [FK_PersonAvailability_BusinessUnit]
GO
ALTER TABLE [dbo].[PersonAvailability]  WITH CHECK ADD  CONSTRAINT [FK_PersonAvailability_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAvailability] CHECK CONSTRAINT [FK_PersonAvailability_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PersonAvailability]  WITH CHECK ADD  CONSTRAINT [FK_PersonAvailability_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAvailability] CHECK CONSTRAINT [FK_PersonAvailability_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PersonAvailability]  WITH CHECK ADD  CONSTRAINT [FK_PersonAvailability_Person3] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonAvailability] CHECK CONSTRAINT [FK_PersonAvailability_Person3]
GO
ALTER TABLE [dbo].[PersonDayOff]  WITH CHECK ADD  CONSTRAINT [FK_PersonDayOff_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PersonDayOff] CHECK CONSTRAINT [FK_PersonDayOff_BusinessUnit]
GO
ALTER TABLE [dbo].[PersonDayOff]  WITH CHECK ADD  CONSTRAINT [FK_PersonDayOff_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonDayOff] CHECK CONSTRAINT [FK_PersonDayOff_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PersonDayOff]  WITH CHECK ADD  CONSTRAINT [FK_PersonDayOff_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonDayOff] CHECK CONSTRAINT [FK_PersonDayOff_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PersonDayOff]  WITH CHECK ADD  CONSTRAINT [FK_PersonDayOff_Person3] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonDayOff] CHECK CONSTRAINT [FK_PersonDayOff_Person3]
GO
ALTER TABLE [dbo].[PersonDayOff]  WITH CHECK ADD  CONSTRAINT [FK_PersonDayOff_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
GO
ALTER TABLE [dbo].[PersonDayOff] CHECK CONSTRAINT [FK_PersonDayOff_Scenario]
GO
ALTER TABLE [dbo].[PersonGroup]  WITH CHECK ADD  CONSTRAINT [FK_PersonGroup_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonGroup] CHECK CONSTRAINT [FK_PersonGroup_Person]
GO
ALTER TABLE [dbo].[PersonGroup]  WITH CHECK ADD  CONSTRAINT [FK_PersonGroup_PersonGroupBase] FOREIGN KEY([PersonGroup])
REFERENCES [dbo].[PersonGroupBase] ([Id])
GO
ALTER TABLE [dbo].[PersonGroup] CHECK CONSTRAINT [FK_PersonGroup_PersonGroupBase]
GO
ALTER TABLE [dbo].[PersonInApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_PersonInApplicationRole_ApplicationRole] FOREIGN KEY([ApplicationRole])
REFERENCES [dbo].[ApplicationRole] ([Id])
GO
ALTER TABLE [dbo].[PersonInApplicationRole] CHECK CONSTRAINT [FK_PersonInApplicationRole_ApplicationRole]
GO
ALTER TABLE [dbo].[PersonInApplicationRole]  WITH CHECK ADD  CONSTRAINT [FK_PersonInApplicationRole_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonInApplicationRole] CHECK CONSTRAINT [FK_PersonInApplicationRole_Person]
GO
ALTER TABLE [dbo].[PersonPeriod]  WITH CHECK ADD  CONSTRAINT [FK_PersonPeriod_BudgetGroup] FOREIGN KEY([BudgetGroup])
REFERENCES [dbo].[BudgetGroup] ([Id])
GO
ALTER TABLE [dbo].[PersonPeriod] CHECK CONSTRAINT [FK_PersonPeriod_BudgetGroup]
GO
ALTER TABLE [dbo].[PersonPeriod]  WITH CHECK ADD  CONSTRAINT [FK_PersonPeriod_Contract] FOREIGN KEY([Contract])
REFERENCES [dbo].[Contract] ([Id])
GO
ALTER TABLE [dbo].[PersonPeriod] CHECK CONSTRAINT [FK_PersonPeriod_Contract]
GO
ALTER TABLE [dbo].[PersonPeriod]  WITH CHECK ADD  CONSTRAINT [FK_PersonPeriod_ContractSchedule] FOREIGN KEY([ContractSchedule])
REFERENCES [dbo].[ContractSchedule] ([Id])
GO
ALTER TABLE [dbo].[PersonPeriod] CHECK CONSTRAINT [FK_PersonPeriod_ContractSchedule]
GO
ALTER TABLE [dbo].[PersonPeriod]  WITH CHECK ADD  CONSTRAINT [FK_PersonPeriod_PartTimePercentage] FOREIGN KEY([PartTimePercentage])
REFERENCES [dbo].[PartTimePercentage] ([Id])
GO
ALTER TABLE [dbo].[PersonPeriod] CHECK CONSTRAINT [FK_PersonPeriod_PartTimePercentage]
GO
ALTER TABLE [dbo].[PersonPeriod]  WITH CHECK ADD  CONSTRAINT [FK_PersonPeriod_Person] FOREIGN KEY([Parent])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonPeriod] CHECK CONSTRAINT [FK_PersonPeriod_Person]
GO
ALTER TABLE [dbo].[PersonPeriod]  WITH CHECK ADD  CONSTRAINT [FK_PersonPeriod_RuleSetBag] FOREIGN KEY([RuleSetBag])
REFERENCES [dbo].[RuleSetBag] ([Id])
GO
ALTER TABLE [dbo].[PersonPeriod] CHECK CONSTRAINT [FK_PersonPeriod_RuleSetBag]
GO
ALTER TABLE [dbo].[PersonPeriod]  WITH CHECK ADD  CONSTRAINT [FK_PersonPeriod_Team] FOREIGN KEY([Team])
REFERENCES [dbo].[Team] ([Id])
GO
ALTER TABLE [dbo].[PersonPeriod] CHECK CONSTRAINT [FK_PersonPeriod_Team]
GO
ALTER TABLE [dbo].[PersonRequest]  WITH CHECK ADD  CONSTRAINT [FK_PersonRequest_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PersonRequest] CHECK CONSTRAINT [FK_PersonRequest_BusinessUnit]
GO
ALTER TABLE [dbo].[PersonRequest]  WITH CHECK ADD  CONSTRAINT [FK_PersonRequest_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonRequest] CHECK CONSTRAINT [FK_PersonRequest_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PersonRequest]  WITH CHECK ADD  CONSTRAINT [FK_PersonRequest_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonRequest] CHECK CONSTRAINT [FK_PersonRequest_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PersonRequest]  WITH CHECK ADD  CONSTRAINT [FK_PersonRequest_Person3] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonRequest] CHECK CONSTRAINT [FK_PersonRequest_Person3]
GO
ALTER TABLE [dbo].[PersonRotation]  WITH CHECK ADD  CONSTRAINT [FK_PersonRotation_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PersonRotation] CHECK CONSTRAINT [FK_PersonRotation_BusinessUnit]
GO
ALTER TABLE [dbo].[PersonRotation]  WITH CHECK ADD  CONSTRAINT [FK_PersonRotation_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonRotation] CHECK CONSTRAINT [FK_PersonRotation_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PersonRotation]  WITH CHECK ADD  CONSTRAINT [FK_PersonRotation_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonRotation] CHECK CONSTRAINT [FK_PersonRotation_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PersonRotation]  WITH CHECK ADD  CONSTRAINT [FK_PersonRotation_Person3] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonRotation] CHECK CONSTRAINT [FK_PersonRotation_Person3]
GO
ALTER TABLE [dbo].[PersonRotation]  WITH CHECK ADD  CONSTRAINT [FK_PersonRotation_Rotation] FOREIGN KEY([Rotation])
REFERENCES [dbo].[Rotation] ([Id])
GO
ALTER TABLE [dbo].[PersonRotation] CHECK CONSTRAINT [FK_PersonRotation_Rotation]
GO
ALTER TABLE [dbo].[PersonsInPayrollExport]  WITH CHECK ADD  CONSTRAINT [FK_PersonsInPayrollExport_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonsInPayrollExport] CHECK CONSTRAINT [FK_PersonsInPayrollExport_Person]
GO
ALTER TABLE [dbo].[PersonsInPayrollExport]  WITH CHECK ADD  CONSTRAINT [FK_PersonsInPayrollExport_PersonID] FOREIGN KEY([PersonId])
REFERENCES [dbo].[PayrollExport] ([Id])
GO
ALTER TABLE [dbo].[PersonsInPayrollExport] CHECK CONSTRAINT [FK_PersonsInPayrollExport_PersonID]
GO
ALTER TABLE [dbo].[PersonSkill]  WITH CHECK ADD  CONSTRAINT [FK_PersonSkill_PersonPeriod] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonPeriod] ([Id])
GO
ALTER TABLE [dbo].[PersonSkill] CHECK CONSTRAINT [FK_PersonSkill_PersonPeriod]
GO
ALTER TABLE [dbo].[PersonSkill]  WITH CHECK ADD  CONSTRAINT [FK_PersonSkill_Skill] FOREIGN KEY([Skill])
REFERENCES [dbo].[Skill] ([Id])
GO
ALTER TABLE [dbo].[PersonSkill] CHECK CONSTRAINT [FK_PersonSkill_Skill]
GO
ALTER TABLE [dbo].[PersonWriteProtectionInfo]  WITH CHECK ADD  CONSTRAINT [FK_WriteProtection_Person] FOREIGN KEY([Id])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonWriteProtectionInfo] CHECK CONSTRAINT [FK_WriteProtection_Person]
GO
ALTER TABLE [dbo].[PersonWriteProtectionInfo]  WITH CHECK ADD  CONSTRAINT [FK_WriteProtection_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonWriteProtectionInfo] CHECK CONSTRAINT [FK_WriteProtection_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PersonWriteProtectionInfo]  WITH CHECK ADD  CONSTRAINT [FK_WriteProtection_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PersonWriteProtectionInfo] CHECK CONSTRAINT [FK_WriteProtection_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PreferenceDay]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceDay_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PreferenceDay] CHECK CONSTRAINT [FK_PreferenceDay_BusinessUnit]
GO
ALTER TABLE [dbo].[PreferenceDay]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceDay_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PreferenceDay] CHECK CONSTRAINT [FK_PreferenceDay_Person]
GO
ALTER TABLE [dbo].[PreferenceDay]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceDay_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PreferenceDay] CHECK CONSTRAINT [FK_PreferenceDay_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PreferenceDay]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceDay_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PreferenceDay] CHECK CONSTRAINT [FK_PreferenceDay_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PreferenceRestriction]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRest_DayOff] FOREIGN KEY([DayOffTemplate])
REFERENCES [dbo].[DayOffTemplate] ([Id])
GO
ALTER TABLE [dbo].[PreferenceRestriction] CHECK CONSTRAINT [FK_PreferenceRest_DayOff]
GO
ALTER TABLE [dbo].[PreferenceRestriction]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRest_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO
ALTER TABLE [dbo].[PreferenceRestriction] CHECK CONSTRAINT [FK_PreferenceRest_ShiftCategory]
GO
ALTER TABLE [dbo].[PreferenceRestriction]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRestrictin_PreferenceDay] FOREIGN KEY([Id])
REFERENCES [dbo].[PreferenceDay] ([Id])
GO
ALTER TABLE [dbo].[PreferenceRestriction] CHECK CONSTRAINT [FK_PreferenceRestrictin_PreferenceDay]
GO
ALTER TABLE [dbo].[PreferenceRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRestictionTemplate_DayOff] FOREIGN KEY([DayOffTemplate])
REFERENCES [dbo].[DayOffTemplate] ([Id])
GO
ALTER TABLE [dbo].[PreferenceRestrictionTemplate] CHECK CONSTRAINT [FK_PreferenceRestictionTemplate_DayOff]
GO
ALTER TABLE [dbo].[PreferenceRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRestrictionTemplate_ExtendedPreferenceTemplate] FOREIGN KEY([Id])
REFERENCES [dbo].[ExtendedPreferenceTemplate] ([Id])
GO
ALTER TABLE [dbo].[PreferenceRestrictionTemplate] CHECK CONSTRAINT [FK_PreferenceRestrictionTemplate_ExtendedPreferenceTemplate]
GO
ALTER TABLE [dbo].[PreferenceRestrictionTemplate]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRestrictionTemplate_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO
ALTER TABLE [dbo].[PreferenceRestrictionTemplate] CHECK CONSTRAINT [FK_PreferenceRestrictionTemplate_ShiftCategory]
GO
ALTER TABLE [dbo].[PublicNote]  WITH CHECK ADD  CONSTRAINT [FK_PublicNote_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PublicNote] CHECK CONSTRAINT [FK_PublicNote_BusinessUnit]
GO
ALTER TABLE [dbo].[PublicNote]  WITH CHECK ADD  CONSTRAINT [FK_PublicNote_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PublicNote] CHECK CONSTRAINT [FK_PublicNote_Person]
GO
ALTER TABLE [dbo].[PublicNote]  WITH CHECK ADD  CONSTRAINT [FK_PublicNote_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PublicNote] CHECK CONSTRAINT [FK_PublicNote_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PublicNote]  WITH CHECK ADD  CONSTRAINT [FK_PublicNote_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PublicNote] CHECK CONSTRAINT [FK_PublicNote_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PublicNote]  WITH CHECK ADD  CONSTRAINT [FK_PublicNote_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
GO
ALTER TABLE [dbo].[PublicNote] CHECK CONSTRAINT [FK_PublicNote_Scenario]
GO
ALTER TABLE [dbo].[PushMessage]  WITH CHECK ADD  CONSTRAINT [FK_PushMessage_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PushMessage] CHECK CONSTRAINT [FK_PushMessage_BusinessUnit]
GO
ALTER TABLE [dbo].[PushMessage]  WITH CHECK ADD  CONSTRAINT [FK_PushMessage_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PushMessage] CHECK CONSTRAINT [FK_PushMessage_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PushMessage]  WITH CHECK ADD  CONSTRAINT [FK_PushMessage_Person_Sender] FOREIGN KEY([Sender])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PushMessage] CHECK CONSTRAINT [FK_PushMessage_Person_Sender]
GO
ALTER TABLE [dbo].[PushMessage]  WITH CHECK ADD  CONSTRAINT [FK_PushMessage_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PushMessage] CHECK CONSTRAINT [FK_PushMessage_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PushMessageDialogue]  WITH CHECK ADD  CONSTRAINT [FK_PushMessageDialogue_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[PushMessageDialogue] CHECK CONSTRAINT [FK_PushMessageDialogue_BusinessUnit]
GO
ALTER TABLE [dbo].[PushMessageDialogue]  WITH CHECK ADD  CONSTRAINT [FK_PushMessageDialogue_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PushMessageDialogue] CHECK CONSTRAINT [FK_PushMessageDialogue_Person_CreatedBy]
GO
ALTER TABLE [dbo].[PushMessageDialogue]  WITH CHECK ADD  CONSTRAINT [FK_PushMessageDialogue_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PushMessageDialogue] CHECK CONSTRAINT [FK_PushMessageDialogue_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[PushMessageDialogue]  WITH CHECK ADD  CONSTRAINT [FK_PushMessageDialogue_PushMessage] FOREIGN KEY([PushMessage])
REFERENCES [dbo].[PushMessage] ([Id])
GO
ALTER TABLE [dbo].[PushMessageDialogue] CHECK CONSTRAINT [FK_PushMessageDialogue_PushMessage]
GO
ALTER TABLE [dbo].[PushMessageDialogue]  WITH CHECK ADD  CONSTRAINT [FK_PushMessageDialogue_Receiver] FOREIGN KEY([Receiver])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[PushMessageDialogue] CHECK CONSTRAINT [FK_PushMessageDialogue_Receiver]
GO
ALTER TABLE [dbo].[QueueSource]  WITH CHECK ADD  CONSTRAINT [FK_QueueSource_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[QueueSource] CHECK CONSTRAINT [FK_QueueSource_Person_CreatedBy]
GO
ALTER TABLE [dbo].[QueueSource]  WITH CHECK ADD  CONSTRAINT [FK_QueueSource_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[QueueSource] CHECK CONSTRAINT [FK_QueueSource_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[QueueSourceCollection]  WITH CHECK ADD  CONSTRAINT [FK_QueueSourceCollection_QueueSource] FOREIGN KEY([Workload])
REFERENCES [dbo].[Workload] ([Id])
GO
ALTER TABLE [dbo].[QueueSourceCollection] CHECK CONSTRAINT [FK_QueueSourceCollection_QueueSource]
GO
ALTER TABLE [dbo].[QueueSourceCollection]  WITH CHECK ADD  CONSTRAINT [FK_QueueSourceCollection_Workload] FOREIGN KEY([QueueSource])
REFERENCES [dbo].[QueueSource] ([Id])
GO
ALTER TABLE [dbo].[QueueSourceCollection] CHECK CONSTRAINT [FK_QueueSourceCollection_Workload]
GO
ALTER TABLE [dbo].[RecurrentDailyMeeting]  WITH CHECK ADD  CONSTRAINT [FK_RecurrentDailyMeeting_RecurrentMeetingOption] FOREIGN KEY([RecurrentMeetingOption])
REFERENCES [dbo].[RecurrentMeetingOption] ([Id])
GO
ALTER TABLE [dbo].[RecurrentDailyMeeting] CHECK CONSTRAINT [FK_RecurrentDailyMeeting_RecurrentMeetingOption]
GO
ALTER TABLE [dbo].[RecurrentMeetingOption]  WITH CHECK ADD  CONSTRAINT [FK_RecurrentMeetingOption_Meeting] FOREIGN KEY([Parent])
REFERENCES [dbo].[Meeting] ([Id])
GO
ALTER TABLE [dbo].[RecurrentMeetingOption] CHECK CONSTRAINT [FK_RecurrentMeetingOption_Meeting]
GO
ALTER TABLE [dbo].[RecurrentMonthlyByDayMeeting]  WITH CHECK ADD  CONSTRAINT [FK_RecurrentMonthlyByDayMeeting_RecurrentMeetingOption] FOREIGN KEY([RecurrentMeetingOption])
REFERENCES [dbo].[RecurrentMeetingOption] ([Id])
GO
ALTER TABLE [dbo].[RecurrentMonthlyByDayMeeting] CHECK CONSTRAINT [FK_RecurrentMonthlyByDayMeeting_RecurrentMeetingOption]
GO
ALTER TABLE [dbo].[RecurrentMonthlyByWeekMeeting]  WITH CHECK ADD  CONSTRAINT [FK_RecurrentMonthlyByWeekMeeting_RecurrentMeetingOption] FOREIGN KEY([RecurrentMeetingOption])
REFERENCES [dbo].[RecurrentMeetingOption] ([Id])
GO
ALTER TABLE [dbo].[RecurrentMonthlyByWeekMeeting] CHECK CONSTRAINT [FK_RecurrentMonthlyByWeekMeeting_RecurrentMeetingOption]
GO
ALTER TABLE [dbo].[RecurrentWeeklyMeeting]  WITH CHECK ADD  CONSTRAINT [FK_RecurrentWeeklyMeeting_RecurrentMeetingOption] FOREIGN KEY([RecurrentMeetingOption])
REFERENCES [dbo].[RecurrentMeetingOption] ([Id])
GO
ALTER TABLE [dbo].[RecurrentWeeklyMeeting] CHECK CONSTRAINT [FK_RecurrentWeeklyMeeting_RecurrentMeetingOption]
GO
ALTER TABLE [dbo].[RecurrentWeeklyMeetingWeekDays]  WITH CHECK ADD  CONSTRAINT [FK_RecurrentWeeklyMeetingWeekDays_RecurrentWeeklyMeeting] FOREIGN KEY([RecurrentWeeklyMeeting])
REFERENCES [dbo].[RecurrentWeeklyMeeting] ([RecurrentMeetingOption])
GO
ALTER TABLE [dbo].[RecurrentWeeklyMeetingWeekDays] CHECK CONSTRAINT [FK_RecurrentWeeklyMeetingWeekDays_RecurrentWeeklyMeeting]
GO
ALTER TABLE [dbo].[ReplyOptions]  WITH CHECK ADD  CONSTRAINT [FK_PushMessage_ReplyOption] FOREIGN KEY([id])
REFERENCES [dbo].[PushMessage] ([Id])
GO
ALTER TABLE [dbo].[ReplyOptions] CHECK CONSTRAINT [FK_PushMessage_ReplyOption]
GO
ALTER TABLE [dbo].[Request]  WITH CHECK ADD  CONSTRAINT [FK_Request_PersonRequest] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonRequest] ([Id])
GO
ALTER TABLE [dbo].[Request] CHECK CONSTRAINT [FK_Request_PersonRequest]
GO
ALTER TABLE [dbo].[Request]  WITH CHECK ADD  CONSTRAINT [FK_RequestPart_PersonRequest] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonRequest] ([Id])
GO
ALTER TABLE [dbo].[Request] CHECK CONSTRAINT [FK_RequestPart_PersonRequest]
GO
ALTER TABLE [dbo].[RootPersonGroup]  WITH CHECK ADD  CONSTRAINT [FK_RootPersonGroup_GroupPage] FOREIGN KEY([Parent])
REFERENCES [dbo].[GroupPage] ([Id])
GO
ALTER TABLE [dbo].[RootPersonGroup] CHECK CONSTRAINT [FK_RootPersonGroup_GroupPage]
GO
ALTER TABLE [dbo].[RootPersonGroup]  WITH CHECK ADD  CONSTRAINT [FK_RootPersonGroup_PersonGroupBase] FOREIGN KEY([PersonGroupBase])
REFERENCES [dbo].[PersonGroupBase] ([Id])
GO
ALTER TABLE [dbo].[RootPersonGroup] CHECK CONSTRAINT [FK_RootPersonGroup_PersonGroupBase]
GO
ALTER TABLE [dbo].[Rotation]  WITH CHECK ADD  CONSTRAINT [FK_Rotation_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Rotation] CHECK CONSTRAINT [FK_Rotation_BusinessUnit]
GO
ALTER TABLE [dbo].[Rotation]  WITH CHECK ADD  CONSTRAINT [FK_Rotation_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Rotation] CHECK CONSTRAINT [FK_Rotation_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Rotation]  WITH CHECK ADD  CONSTRAINT [FK_Rotation_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Rotation] CHECK CONSTRAINT [FK_Rotation_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[RotationDay]  WITH CHECK ADD  CONSTRAINT [FK_RotationDay_Rotation] FOREIGN KEY([Parent])
REFERENCES [dbo].[Rotation] ([Id])
GO
ALTER TABLE [dbo].[RotationDay] CHECK CONSTRAINT [FK_RotationDay_Rotation]
GO
ALTER TABLE [dbo].[RotationRestriction]  WITH CHECK ADD  CONSTRAINT [FK_RotationDayRestrictionNew_DayOff] FOREIGN KEY([DayOffTemplate])
REFERENCES [dbo].[DayOffTemplate] ([Id])
GO
ALTER TABLE [dbo].[RotationRestriction] CHECK CONSTRAINT [FK_RotationDayRestrictionNew_DayOff]
GO
ALTER TABLE [dbo].[RotationRestriction]  WITH CHECK ADD  CONSTRAINT [FK_RotationDayRestrictionNew_RotationDay] FOREIGN KEY([Parent])
REFERENCES [dbo].[RotationDay] ([Id])
GO
ALTER TABLE [dbo].[RotationRestriction] CHECK CONSTRAINT [FK_RotationDayRestrictionNew_RotationDay]
GO
ALTER TABLE [dbo].[RotationRestriction]  WITH CHECK ADD  CONSTRAINT [FK_RotationDayRestrictionNew_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO
ALTER TABLE [dbo].[RotationRestriction] CHECK CONSTRAINT [FK_RotationDayRestrictionNew_ShiftCategory]
GO
ALTER TABLE [dbo].[RtaState]  WITH CHECK ADD  CONSTRAINT [FK_RtaState_RtaStateGroup] FOREIGN KEY([Parent])
REFERENCES [dbo].[RtaStateGroup] ([Id])
GO
ALTER TABLE [dbo].[RtaState] CHECK CONSTRAINT [FK_RtaState_RtaStateGroup]
GO
ALTER TABLE [dbo].[RtaStateGroup]  WITH CHECK ADD  CONSTRAINT [FK_RtaStateGroup_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[RtaStateGroup] CHECK CONSTRAINT [FK_RtaStateGroup_BusinessUnit]
GO
ALTER TABLE [dbo].[RtaStateGroup]  WITH CHECK ADD  CONSTRAINT [FK_RtaStateGroup_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[RtaStateGroup] CHECK CONSTRAINT [FK_RtaStateGroup_Person_CreatedBy]
GO
ALTER TABLE [dbo].[RtaStateGroup]  WITH CHECK ADD  CONSTRAINT [FK_RtaStateGroup_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[RtaStateGroup] CHECK CONSTRAINT [FK_RtaStateGroup_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[RuleSetBag]  WITH CHECK ADD  CONSTRAINT [FK_RuleSetBag_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[RuleSetBag] CHECK CONSTRAINT [FK_RuleSetBag_BusinessUnit]
GO
ALTER TABLE [dbo].[RuleSetBag]  WITH CHECK ADD  CONSTRAINT [FK_RuleSetBag_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[RuleSetBag] CHECK CONSTRAINT [FK_RuleSetBag_Person_CreatedBy]
GO
ALTER TABLE [dbo].[RuleSetBag]  WITH CHECK ADD  CONSTRAINT [FK_RuleSetBag_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[RuleSetBag] CHECK CONSTRAINT [FK_RuleSetBag_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[RuleSetRuleSetBag]  WITH CHECK ADD  CONSTRAINT [FK_RuleSetRuleSetBag_RuleSetBag] FOREIGN KEY([RuleSetBag])
REFERENCES [dbo].[RuleSetBag] ([Id])
GO
ALTER TABLE [dbo].[RuleSetRuleSetBag] CHECK CONSTRAINT [FK_RuleSetRuleSetBag_RuleSetBag]
GO
ALTER TABLE [dbo].[RuleSetRuleSetBag]  WITH CHECK ADD  CONSTRAINT [FK_RuleSetRuleSetBag_WorkShiftRuleSet] FOREIGN KEY([RuleSet])
REFERENCES [dbo].[WorkShiftRuleSet] ([Id])
GO
ALTER TABLE [dbo].[RuleSetRuleSetBag] CHECK CONSTRAINT [FK_RuleSetRuleSetBag_WorkShiftRuleSet]
GO
ALTER TABLE [dbo].[Scenario]  WITH CHECK ADD  CONSTRAINT [FK_Scenario_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Scenario] CHECK CONSTRAINT [FK_Scenario_BusinessUnit]
GO
ALTER TABLE [dbo].[Scenario]  WITH CHECK ADD  CONSTRAINT [FK_Scenario_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Scenario] CHECK CONSTRAINT [FK_Scenario_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Scenario]  WITH CHECK ADD  CONSTRAINT [FK_Scenario_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Scenario] CHECK CONSTRAINT [FK_Scenario_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[SchedulePeriod]  WITH CHECK ADD  CONSTRAINT [FK_SchedulePeriod_Person] FOREIGN KEY([Parent])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[SchedulePeriod] CHECK CONSTRAINT [FK_SchedulePeriod_Person]
GO
ALTER TABLE [dbo].[SchedulePeriodShiftCategoryLimitation]  WITH CHECK ADD  CONSTRAINT [FK_Limitation_SchedulePeriod] FOREIGN KEY([SchedulePeriod])
REFERENCES [dbo].[SchedulePeriod] ([Id])
GO
ALTER TABLE [dbo].[SchedulePeriodShiftCategoryLimitation] CHECK CONSTRAINT [FK_Limitation_SchedulePeriod]
GO
ALTER TABLE [dbo].[SchedulePeriodShiftCategoryLimitation]  WITH CHECK ADD  CONSTRAINT [FK_Limitation_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO
ALTER TABLE [dbo].[SchedulePeriodShiftCategoryLimitation] CHECK CONSTRAINT [FK_Limitation_ShiftCategory]
GO
ALTER TABLE [dbo].[Scorecard]  WITH CHECK ADD  CONSTRAINT [FK_Scorecard_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Scorecard] CHECK CONSTRAINT [FK_Scorecard_BusinessUnit]
GO
ALTER TABLE [dbo].[Scorecard]  WITH CHECK ADD  CONSTRAINT [FK_Scorecard_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Scorecard] CHECK CONSTRAINT [FK_Scorecard_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Scorecard]  WITH CHECK ADD  CONSTRAINT [FK_Scorecard_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Scorecard] CHECK CONSTRAINT [FK_Scorecard_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[ShiftCategory]  WITH CHECK ADD  CONSTRAINT [FK_ShiftCategory_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[ShiftCategory] CHECK CONSTRAINT [FK_ShiftCategory_BusinessUnit]
GO
ALTER TABLE [dbo].[ShiftCategory]  WITH CHECK ADD  CONSTRAINT [FK_ShiftCategory_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ShiftCategory] CHECK CONSTRAINT [FK_ShiftCategory_Person_CreatedBy]
GO
ALTER TABLE [dbo].[ShiftCategory]  WITH CHECK ADD  CONSTRAINT [FK_ShiftCategory_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ShiftCategory] CHECK CONSTRAINT [FK_ShiftCategory_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[ShiftCategoryJusticeValues]  WITH CHECK ADD  CONSTRAINT [FK_ShiftCategory_Justice] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO
ALTER TABLE [dbo].[ShiftCategoryJusticeValues] CHECK CONSTRAINT [FK_ShiftCategory_Justice]
GO
ALTER TABLE [dbo].[ShiftTradeRequest]  WITH CHECK ADD  CONSTRAINT [FK_ShiftTradeRequest_Request] FOREIGN KEY([Request])
REFERENCES [dbo].[Request] ([Id])
GO
ALTER TABLE [dbo].[ShiftTradeRequest] CHECK CONSTRAINT [FK_ShiftTradeRequest_Request]
GO
ALTER TABLE [dbo].[ShiftTradeSwapDetail]  WITH CHECK ADD  CONSTRAINT [FK_ShiftTradeSwapDetail_PersonFrom] FOREIGN KEY([PersonFrom])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ShiftTradeSwapDetail] CHECK CONSTRAINT [FK_ShiftTradeSwapDetail_PersonFrom]
GO
ALTER TABLE [dbo].[ShiftTradeSwapDetail]  WITH CHECK ADD  CONSTRAINT [FK_ShiftTradeSwapDetail_PersonTo] FOREIGN KEY([PersonTo])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ShiftTradeSwapDetail] CHECK CONSTRAINT [FK_ShiftTradeSwapDetail_PersonTo]
GO
ALTER TABLE [dbo].[ShiftTradeSwapDetail]  WITH CHECK ADD  CONSTRAINT [FK_ShiftTradeSwapDetail_ShiftTradeRequest] FOREIGN KEY([Parent])
REFERENCES [dbo].[ShiftTradeRequest] ([Request])
GO
ALTER TABLE [dbo].[ShiftTradeSwapDetail] CHECK CONSTRAINT [FK_ShiftTradeSwapDetail_ShiftTradeRequest]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_Site_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_Site_BusinessUnit]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_Site_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_Site_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Site]  WITH CHECK ADD  CONSTRAINT [FK_Site_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Site] CHECK CONSTRAINT [FK_Site_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[Skill]  WITH CHECK ADD  CONSTRAINT [FK_Skill_Activity] FOREIGN KEY([Activity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[Skill] CHECK CONSTRAINT [FK_Skill_Activity]
GO
ALTER TABLE [dbo].[Skill]  WITH CHECK ADD  CONSTRAINT [FK_Skill_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Skill] CHECK CONSTRAINT [FK_Skill_BusinessUnit]
GO
ALTER TABLE [dbo].[Skill]  WITH CHECK ADD  CONSTRAINT [FK_Skill_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Skill] CHECK CONSTRAINT [FK_Skill_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Skill]  WITH CHECK ADD  CONSTRAINT [FK_Skill_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Skill] CHECK CONSTRAINT [FK_Skill_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[Skill]  WITH CHECK ADD  CONSTRAINT [FK_Skill_SkillType] FOREIGN KEY([SkillType])
REFERENCES [dbo].[SkillType] ([Id])
GO
ALTER TABLE [dbo].[Skill] CHECK CONSTRAINT [FK_Skill_SkillType]
GO
ALTER TABLE [dbo].[SkillCollection]  WITH CHECK ADD  CONSTRAINT [FK_SkillCollection_BudgetGroup] FOREIGN KEY([BudgetGroup])
REFERENCES [dbo].[BudgetGroup] ([Id])
GO
ALTER TABLE [dbo].[SkillCollection] CHECK CONSTRAINT [FK_SkillCollection_BudgetGroup]
GO
ALTER TABLE [dbo].[SkillCollection]  WITH CHECK ADD  CONSTRAINT [FK_SkillCollection_Skill] FOREIGN KEY([Skill])
REFERENCES [dbo].[Skill] ([Id])
GO
ALTER TABLE [dbo].[SkillCollection] CHECK CONSTRAINT [FK_SkillCollection_Skill]
GO
ALTER TABLE [dbo].[SkillDataPeriod]  WITH CHECK ADD  CONSTRAINT [FK_SkillDataPeriod_SkillDay] FOREIGN KEY([Parent])
REFERENCES [dbo].[SkillDay] ([Id])
GO
ALTER TABLE [dbo].[SkillDataPeriod] CHECK CONSTRAINT [FK_SkillDataPeriod_SkillDay]
GO
ALTER TABLE [dbo].[SkillDay]  WITH CHECK ADD  CONSTRAINT [FK_SkillDay_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[SkillDay] CHECK CONSTRAINT [FK_SkillDay_BusinessUnit]
GO
ALTER TABLE [dbo].[SkillDay]  WITH CHECK ADD  CONSTRAINT [FK_SkillDay_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[SkillDay] CHECK CONSTRAINT [FK_SkillDay_Person_CreatedBy]
GO
ALTER TABLE [dbo].[SkillDay]  WITH CHECK ADD  CONSTRAINT [FK_SkillDay_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[SkillDay] CHECK CONSTRAINT [FK_SkillDay_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[SkillDay]  WITH CHECK ADD  CONSTRAINT [FK_SkillDay_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
GO
ALTER TABLE [dbo].[SkillDay] CHECK CONSTRAINT [FK_SkillDay_Scenario]
GO
ALTER TABLE [dbo].[SkillDay]  WITH CHECK ADD  CONSTRAINT [FK_SkillDay_Skill] FOREIGN KEY([Skill])
REFERENCES [dbo].[Skill] ([Id])
GO
ALTER TABLE [dbo].[SkillDay] CHECK CONSTRAINT [FK_SkillDay_Skill]
GO
ALTER TABLE [dbo].[SkillDay]  WITH CHECK ADD  CONSTRAINT [FK_SkillDayTemplateReference_Skill] FOREIGN KEY([TemplateReferenceSkill])
REFERENCES [dbo].[Skill] ([Id])
GO
ALTER TABLE [dbo].[SkillDay] CHECK CONSTRAINT [FK_SkillDayTemplateReference_Skill]
GO
ALTER TABLE [dbo].[SkillDayTemplate]  WITH CHECK ADD  CONSTRAINT [FK_SkillDayTemplate_Skill] FOREIGN KEY([Parent])
REFERENCES [dbo].[Skill] ([Id])
GO
ALTER TABLE [dbo].[SkillDayTemplate] CHECK CONSTRAINT [FK_SkillDayTemplate_Skill]
GO
ALTER TABLE [dbo].[SkillType]  WITH CHECK ADD  CONSTRAINT [FK_SkillType_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[SkillType] CHECK CONSTRAINT [FK_SkillType_Person_CreatedBy]
GO
ALTER TABLE [dbo].[SkillType]  WITH CHECK ADD  CONSTRAINT [FK_SkillType_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[SkillType] CHECK CONSTRAINT [FK_SkillType_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[StateGroupActivityAlarm]  WITH CHECK ADD  CONSTRAINT [FK_StateGroupActivityAlarm_Activity] FOREIGN KEY([Activity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[StateGroupActivityAlarm] CHECK CONSTRAINT [FK_StateGroupActivityAlarm_Activity]
GO
ALTER TABLE [dbo].[StateGroupActivityAlarm]  WITH CHECK ADD  CONSTRAINT [FK_StateGroupActivityAlarm_AlarmType] FOREIGN KEY([AlarmType])
REFERENCES [dbo].[AlarmType] ([Id])
GO
ALTER TABLE [dbo].[StateGroupActivityAlarm] CHECK CONSTRAINT [FK_StateGroupActivityAlarm_AlarmType]
GO
ALTER TABLE [dbo].[StateGroupActivityAlarm]  WITH CHECK ADD  CONSTRAINT [FK_StateGroupActivityAlarm_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[StateGroupActivityAlarm] CHECK CONSTRAINT [FK_StateGroupActivityAlarm_BusinessUnit]
GO
ALTER TABLE [dbo].[StateGroupActivityAlarm]  WITH CHECK ADD  CONSTRAINT [FK_StateGroupActivityAlarm_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[StateGroupActivityAlarm] CHECK CONSTRAINT [FK_StateGroupActivityAlarm_Person_CreatedBy]
GO
ALTER TABLE [dbo].[StateGroupActivityAlarm]  WITH CHECK ADD  CONSTRAINT [FK_StateGroupActivityAlarm_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[StateGroupActivityAlarm] CHECK CONSTRAINT [FK_StateGroupActivityAlarm_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[StateGroupActivityAlarm]  WITH CHECK ADD  CONSTRAINT [FK_StateGroupActivityAlarm_RtaStateGroup] FOREIGN KEY([StateGroup])
REFERENCES [dbo].[RtaStateGroup] ([Id])
GO
ALTER TABLE [dbo].[StateGroupActivityAlarm] CHECK CONSTRAINT [FK_StateGroupActivityAlarm_RtaStateGroup]
GO
ALTER TABLE [dbo].[StudentAvailabilityDay]  WITH CHECK ADD  CONSTRAINT [FK_StudentAvailabilityDay_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[StudentAvailabilityDay] CHECK CONSTRAINT [FK_StudentAvailabilityDay_BusinessUnit]
GO
ALTER TABLE [dbo].[StudentAvailabilityDay]  WITH CHECK ADD  CONSTRAINT [FK_StudentAvailabilityDay_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[StudentAvailabilityDay] CHECK CONSTRAINT [FK_StudentAvailabilityDay_Person]
GO
ALTER TABLE [dbo].[StudentAvailabilityDay]  WITH CHECK ADD  CONSTRAINT [FK_StudentAvailabilityDay_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[StudentAvailabilityDay] CHECK CONSTRAINT [FK_StudentAvailabilityDay_Person_CreatedBy]
GO
ALTER TABLE [dbo].[StudentAvailabilityDay]  WITH CHECK ADD  CONSTRAINT [FK_StudentAvailabilityDay_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[StudentAvailabilityDay] CHECK CONSTRAINT [FK_StudentAvailabilityDay_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[StudentAvailabilityRestriction]  WITH CHECK ADD  CONSTRAINT [FK_StudentAvailabilityDay_AvailabilityRestriction] FOREIGN KEY([Parent])
REFERENCES [dbo].[StudentAvailabilityDay] ([Id])
GO
ALTER TABLE [dbo].[StudentAvailabilityRestriction] CHECK CONSTRAINT [FK_StudentAvailabilityDay_AvailabilityRestriction]
GO
ALTER TABLE [dbo].[SystemRoleApplicationRoleMapper]  WITH CHECK ADD  CONSTRAINT [FK_SystemRoleApplicationRoleMapper_ApplicationRole] FOREIGN KEY([ApplicationRole])
REFERENCES [dbo].[ApplicationRole] ([Id])
GO
ALTER TABLE [dbo].[SystemRoleApplicationRoleMapper] CHECK CONSTRAINT [FK_SystemRoleApplicationRoleMapper_ApplicationRole]
GO
ALTER TABLE [dbo].[SystemRoleApplicationRoleMapper]  WITH CHECK ADD  CONSTRAINT [FK_SystemRoleApplicationRoleMapper_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[SystemRoleApplicationRoleMapper] CHECK CONSTRAINT [FK_SystemRoleApplicationRoleMapper_BusinessUnit]
GO
ALTER TABLE [dbo].[SystemRoleApplicationRoleMapper]  WITH CHECK ADD  CONSTRAINT [FK_SystemRoleApplicationRoleMapper_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[SystemRoleApplicationRoleMapper] CHECK CONSTRAINT [FK_SystemRoleApplicationRoleMapper_Person_CreatedBy]
GO
ALTER TABLE [dbo].[SystemRoleApplicationRoleMapper]  WITH CHECK ADD  CONSTRAINT [FK_SystemRoleApplicationRoleMapper_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[SystemRoleApplicationRoleMapper] CHECK CONSTRAINT [FK_SystemRoleApplicationRoleMapper_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK_Team_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK_Team_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK_Team_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK_Team_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK_Team_Scorecard] FOREIGN KEY([Scorecard])
REFERENCES [dbo].[Scorecard] ([Id])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK_Team_Scorecard]
GO
ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK_Team_Site] FOREIGN KEY([Site])
REFERENCES [dbo].[Site] ([Id])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK_Team_Site]
GO
ALTER TABLE [dbo].[TemplateMultisitePeriod]  WITH CHECK ADD  CONSTRAINT [FK_TemplateMultisitePeriod_MultisiteDayTemplate] FOREIGN KEY([Parent])
REFERENCES [dbo].[MultisiteDayTemplate] ([Id])
GO
ALTER TABLE [dbo].[TemplateMultisitePeriod] CHECK CONSTRAINT [FK_TemplateMultisitePeriod_MultisiteDayTemplate]
GO
ALTER TABLE [dbo].[TemplateMultisitePeriodDistribution]  WITH CHECK ADD  CONSTRAINT [FK_TemplateMultisitePeriodDistribution_ChildSkill] FOREIGN KEY([ChildSkill])
REFERENCES [dbo].[ChildSkill] ([Skill])
GO
ALTER TABLE [dbo].[TemplateMultisitePeriodDistribution] CHECK CONSTRAINT [FK_TemplateMultisitePeriodDistribution_ChildSkill]
GO
ALTER TABLE [dbo].[TemplateMultisitePeriodDistribution]  WITH CHECK ADD  CONSTRAINT [FK_TemplateMultisitePeriodDistribution_TemplateMultisitePeriod] FOREIGN KEY([Parent])
REFERENCES [dbo].[TemplateMultisitePeriod] ([Id])
GO
ALTER TABLE [dbo].[TemplateMultisitePeriodDistribution] CHECK CONSTRAINT [FK_TemplateMultisitePeriodDistribution_TemplateMultisitePeriod]
GO
ALTER TABLE [dbo].[TemplateSkillDataPeriod]  WITH CHECK ADD  CONSTRAINT [FK_TemplateSkillDataPeriod_SkillDayTemplate] FOREIGN KEY([Parent])
REFERENCES [dbo].[SkillDayTemplate] ([Id])
GO
ALTER TABLE [dbo].[TemplateSkillDataPeriod] CHECK CONSTRAINT [FK_TemplateSkillDataPeriod_SkillDayTemplate]
GO
ALTER TABLE [dbo].[TemplateTaskPeriod]  WITH CHECK ADD  CONSTRAINT [FK_TemplateTaskPeriod_WorkloadDayBase] FOREIGN KEY([Parent])
REFERENCES [dbo].[WorkloadDayBase] ([Id])
GO
ALTER TABLE [dbo].[TemplateTaskPeriod] CHECK CONSTRAINT [FK_TemplateTaskPeriod_WorkloadDayBase]
GO
ALTER TABLE [dbo].[TextRequest]  WITH CHECK ADD  CONSTRAINT [FK_TextRequest_Request] FOREIGN KEY([Request])
REFERENCES [dbo].[Request] ([Id])
GO
ALTER TABLE [dbo].[TextRequest] CHECK CONSTRAINT [FK_TextRequest_Request]
GO
ALTER TABLE [dbo].[UserDetail]  WITH CHECK ADD  CONSTRAINT [FK_UserDetail_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[UserDetail] CHECK CONSTRAINT [FK_UserDetail_Person]
GO
ALTER TABLE [dbo].[ValidatedVolumeDay]  WITH CHECK ADD  CONSTRAINT [FK_ValidatedVolumeDay_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[ValidatedVolumeDay] CHECK CONSTRAINT [FK_ValidatedVolumeDay_BusinessUnit]
GO
ALTER TABLE [dbo].[ValidatedVolumeDay]  WITH CHECK ADD  CONSTRAINT [FK_ValidatedVolumeDay_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ValidatedVolumeDay] CHECK CONSTRAINT [FK_ValidatedVolumeDay_Person_CreatedBy]
GO
ALTER TABLE [dbo].[ValidatedVolumeDay]  WITH CHECK ADD  CONSTRAINT [FK_ValidatedVolumeDay_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ValidatedVolumeDay] CHECK CONSTRAINT [FK_ValidatedVolumeDay_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[ValidatedVolumeDay]  WITH CHECK ADD  CONSTRAINT [FK_ValidatedVolumeDay_Workload] FOREIGN KEY([Workload])
REFERENCES [dbo].[Workload] ([Id])
GO
ALTER TABLE [dbo].[ValidatedVolumeDay] CHECK CONSTRAINT [FK_ValidatedVolumeDay_Workload]
GO
ALTER TABLE [dbo].[WorkflowControlSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSet_Activity_AllowedPreferenceActivity] FOREIGN KEY([AllowedPreferenceActivity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[WorkflowControlSet] CHECK CONSTRAINT [FK_WorkflowControlSet_Activity_AllowedPreferenceActivity]
GO
ALTER TABLE [dbo].[WorkflowControlSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSet_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[WorkflowControlSet] CHECK CONSTRAINT [FK_WorkflowControlSet_BusinessUnit]
GO
ALTER TABLE [dbo].[WorkflowControlSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSet_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[WorkflowControlSet] CHECK CONSTRAINT [FK_WorkflowControlSet_Person_CreatedBy]
GO
ALTER TABLE [dbo].[WorkflowControlSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSet_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[WorkflowControlSet] CHECK CONSTRAINT [FK_WorkflowControlSet_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[WorkflowControlSetAllowedDayOffs]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetAllowedDayOffs_DayOff] FOREIGN KEY([DayOffTemplate])
REFERENCES [dbo].[DayOffTemplate] ([Id])
GO
ALTER TABLE [dbo].[WorkflowControlSetAllowedDayOffs] CHECK CONSTRAINT [FK_WorkflowControlSetAllowedDayOffs_DayOff]
GO
ALTER TABLE [dbo].[WorkflowControlSetAllowedDayOffs]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetAllowedDayOffs_WorkflowControlSet] FOREIGN KEY([WorkflowControlSet])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO
ALTER TABLE [dbo].[WorkflowControlSetAllowedDayOffs] CHECK CONSTRAINT [FK_WorkflowControlSetAllowedDayOffs_WorkflowControlSet]
GO
ALTER TABLE [dbo].[WorkflowControlSetAllowedShiftCategories]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetAllowedShiftCategories_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO
ALTER TABLE [dbo].[WorkflowControlSetAllowedShiftCategories] CHECK CONSTRAINT [FK_WorkflowControlSetAllowedShiftCategories_ShiftCategory]
GO
ALTER TABLE [dbo].[WorkflowControlSetAllowedShiftCategories]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetAllowedShiftCategories_WorkflowControlSet] FOREIGN KEY([WorkflowControlSet])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO
ALTER TABLE [dbo].[WorkflowControlSetAllowedShiftCategories] CHECK CONSTRAINT [FK_WorkflowControlSetAllowedShiftCategories_WorkflowControlSet]
GO
ALTER TABLE [dbo].[WorkflowControlSetSkills]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetSkills_Skill] FOREIGN KEY([Skill])
REFERENCES [dbo].[Skill] ([Id])
GO
ALTER TABLE [dbo].[WorkflowControlSetSkills] CHECK CONSTRAINT [FK_WorkflowControlSetSkills_Skill]
GO
ALTER TABLE [dbo].[WorkflowControlSetSkills]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowControlSetSkills_WorkflowControlSet] FOREIGN KEY([WorkflowControlSet])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO
ALTER TABLE [dbo].[WorkflowControlSetSkills] CHECK CONSTRAINT [FK_WorkflowControlSetSkills_WorkflowControlSet]
GO
ALTER TABLE [dbo].[Workload]  WITH CHECK ADD  CONSTRAINT [FK_Workload_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[Workload] CHECK CONSTRAINT [FK_Workload_BusinessUnit]
GO
ALTER TABLE [dbo].[Workload]  WITH CHECK ADD  CONSTRAINT [FK_Workload_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Workload] CHECK CONSTRAINT [FK_Workload_Person_CreatedBy]
GO
ALTER TABLE [dbo].[Workload]  WITH CHECK ADD  CONSTRAINT [FK_Workload_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[Workload] CHECK CONSTRAINT [FK_Workload_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[Workload]  WITH CHECK ADD  CONSTRAINT [FK_Workload_Skill] FOREIGN KEY([Skill])
REFERENCES [dbo].[Skill] ([Id])
GO
ALTER TABLE [dbo].[Workload] CHECK CONSTRAINT [FK_Workload_Skill]
GO
ALTER TABLE [dbo].[WorkloadDay]  WITH CHECK ADD  CONSTRAINT [FK_WorkloadDay_SkillDay] FOREIGN KEY([Parent])
REFERENCES [dbo].[SkillDay] ([Id])
GO
ALTER TABLE [dbo].[WorkloadDay] CHECK CONSTRAINT [FK_WorkloadDay_SkillDay]
GO
ALTER TABLE [dbo].[WorkloadDay]  WITH CHECK ADD  CONSTRAINT [FK_WorkloadDay_Workload] FOREIGN KEY([Workload])
REFERENCES [dbo].[Workload] ([Id])
GO
ALTER TABLE [dbo].[WorkloadDay] CHECK CONSTRAINT [FK_WorkloadDay_Workload]
GO
ALTER TABLE [dbo].[WorkloadDay]  WITH CHECK ADD  CONSTRAINT [FK_WorkloadDay_WorkloadDayBase] FOREIGN KEY([WorkloadDayBase])
REFERENCES [dbo].[WorkloadDayBase] ([Id])
GO
ALTER TABLE [dbo].[WorkloadDay] CHECK CONSTRAINT [FK_WorkloadDay_WorkloadDayBase]
GO
ALTER TABLE [dbo].[WorkloadDayBase]  WITH CHECK ADD  CONSTRAINT [FK_WorkloadDayBase_Workload] FOREIGN KEY([Workload])
REFERENCES [dbo].[Workload] ([Id])
GO
ALTER TABLE [dbo].[WorkloadDayBase] CHECK CONSTRAINT [FK_WorkloadDayBase_Workload]
GO
ALTER TABLE [dbo].[WorkloadDayTemplate]  WITH CHECK ADD  CONSTRAINT [FK_WorkloadDayTemplate_Workload] FOREIGN KEY([Parent])
REFERENCES [dbo].[Workload] ([Id])
GO
ALTER TABLE [dbo].[WorkloadDayTemplate] CHECK CONSTRAINT [FK_WorkloadDayTemplate_Workload]
GO
ALTER TABLE [dbo].[WorkloadDayTemplate]  WITH CHECK ADD  CONSTRAINT [FK_WorkloadDayTemplate_WorkloadDayBase] FOREIGN KEY([WorkloadDayBase])
REFERENCES [dbo].[WorkloadDayBase] ([Id])
GO
ALTER TABLE [dbo].[WorkloadDayTemplate] CHECK CONSTRAINT [FK_WorkloadDayTemplate_WorkloadDayBase]
GO
ALTER TABLE [dbo].[WorkShiftRuleSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkShiftRuleSet_Activity] FOREIGN KEY([BaseActivity])
REFERENCES [dbo].[Activity] ([Id])
GO
ALTER TABLE [dbo].[WorkShiftRuleSet] CHECK CONSTRAINT [FK_WorkShiftRuleSet_Activity]
GO
ALTER TABLE [dbo].[WorkShiftRuleSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkShiftRuleSet_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO
ALTER TABLE [dbo].[WorkShiftRuleSet] CHECK CONSTRAINT [FK_WorkShiftRuleSet_BusinessUnit]
GO
ALTER TABLE [dbo].[WorkShiftRuleSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkShiftRuleSet_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[WorkShiftRuleSet] CHECK CONSTRAINT [FK_WorkShiftRuleSet_Person_CreatedBy]
GO
ALTER TABLE [dbo].[WorkShiftRuleSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkShiftRuleSet_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[WorkShiftRuleSet] CHECK CONSTRAINT [FK_WorkShiftRuleSet_Person_UpdatedBy]
GO
ALTER TABLE [dbo].[WorkShiftRuleSet]  WITH CHECK ADD  CONSTRAINT [FK_WorkShiftRuleSet_ShiftCategory] FOREIGN KEY([Category])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO
ALTER TABLE [dbo].[WorkShiftRuleSet] CHECK CONSTRAINT [FK_WorkShiftRuleSet_ShiftCategory]
GO
ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_date]
GO
ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_interval]
GO
ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_queue] FOREIGN KEY([queue_id])
REFERENCES [mart].[dim_queue] ([queue_id])
GO
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_queue]
GO
SET NOCOUNT OFF
GO
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (329,'7.1.329') 