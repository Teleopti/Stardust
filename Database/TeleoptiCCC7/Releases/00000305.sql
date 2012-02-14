/* 
Trunk initiated: 
2010-10-04 
10:50
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Peter Westlin 
--Date: 2010-09-29
--Desc: BudgetGroups  
----------------  

    create table BudgetGroup (
        Id UNIQUEIDENTIFIER not null,
       Version INT not null,
       CreatedBy UNIQUEIDENTIFIER not null,
       UpdatedBy UNIQUEIDENTIFIER not null,
       CreatedOn DATETIME not null,
       UpdatedOn DATETIME not null,
       Name NVARCHAR(50) not null,
       DaysPerYear INT not null,
       TimeZone NVARCHAR(50) not null,
       BusinessUnit UNIQUEIDENTIFIER not null,
       IsDeleted BIT not null,
       primary key (Id)
    )

    create table SkillCollection (
        BudgetGroup UNIQUEIDENTIFIER not null,
       Skill UNIQUEIDENTIFIER not null
    )

    alter table BudgetGroup 
    add constraint FK_BudgetGroup_Person_CreatedBy 
    foreign key (CreatedBy) 
    references Person

    alter table BudgetGroup 
        add constraint FK_BudgetGroup_Person_UpdatedBy 
        foreign key (UpdatedBy) 
        references Person

    alter table BudgetGroup 
        add constraint FK_BudgetGroup_BusinessUnit 
        foreign key (BusinessUnit) 
        references BusinessUnit

    alter table SkillCollection 
        add constraint FK_SkillCollection_Skill 
        foreign key (Skill) 
        references Skill

    alter table SkillCollection 
        add constraint FK_SkillCollection_BudgetGroup 
        foreign key (BudgetGroup) 
        references BudgetGroup

		
----------------  
--Name: Robin Karlsson
--Date: 2010-09-29
--Desc: Removing old stuff for budgeting not used any more  
----------------  
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_BudgetRow_PlanningGroup]') AND parent_object_id = OBJECT_ID(N'[dbo].[BudgetRow]'))
ALTER TABLE [dbo].[BudgetRow] DROP CONSTRAINT [FK_BudgetRow_PlanningGroup]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PlanningGroup_BusinessUnit]') AND parent_object_id = OBJECT_ID(N'[dbo].[PlanningGroup]'))
ALTER TABLE [dbo].[PlanningGroup] DROP CONSTRAINT [FK_PlanningGroup_BusinessUnit]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PlanningGroup_Person_CreatedBy]') AND parent_object_id = OBJECT_ID(N'[dbo].[PlanningGroup]'))
ALTER TABLE [dbo].[PlanningGroup] DROP CONSTRAINT [FK_PlanningGroup_Person_CreatedBy]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PlanningGroup_Person_UpdatedBy]') AND parent_object_id = OBJECT_ID(N'[dbo].[PlanningGroup]'))
ALTER TABLE [dbo].[PlanningGroup] DROP CONSTRAINT [FK_PlanningGroup_Person_UpdatedBy]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PlanningGroup_Skill]') AND parent_object_id = OBJECT_ID(N'[dbo].[PlanningGroupSkill]'))
ALTER TABLE [dbo].[PlanningGroupSkill] DROP CONSTRAINT [FK_PlanningGroup_Skill]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Skill_PlanningGroup]') AND parent_object_id = OBJECT_ID(N'[dbo].[PlanningGroupSkill]'))
ALTER TABLE [dbo].[PlanningGroupSkill] DROP CONSTRAINT [FK_Skill_PlanningGroup]
GO

/****** Object:  Table [dbo].[BudgetRow]    Script Date: 09/29/2010 17:45:11 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BudgetRow]') AND type in (N'U'))
DROP TABLE [dbo].[BudgetRow]
GO

/****** Object:  Table [dbo].[PlanningGroup]    Script Date: 09/29/2010 17:45:11 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PlanningGroup]') AND type in (N'U'))
DROP TABLE [dbo].[PlanningGroup]
GO

/****** Object:  Table [dbo].[PlanningGroupSkill]    Script Date: 09/29/2010 17:45:11 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PlanningGroupSkill]') AND type in (N'U'))
DROP TABLE [dbo].[PlanningGroupSkill]
GO

----------------  
--Name: Robin Karlsson
--Date: 2010-09-29
--Desc: Added handling of custom shrinkage rows  
----------------  
CREATE TABLE [dbo].[CustomShrinkage](
	[Id] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[ShrinkageName] [nvarchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[CustomShrinkage]  WITH CHECK ADD  CONSTRAINT [FK_CustomShrinkage_BudgetGroup] FOREIGN KEY([Parent])
REFERENCES [dbo].[BudgetGroup] ([Id])
GO

ALTER TABLE [dbo].[CustomShrinkage] CHECK CONSTRAINT [FK_CustomShrinkage_BudgetGroup]
GO
----------------  
--Name: Robin Karlsson
--Date: 2010-09-30
--Desc: The basic stuff for budget days
----------------  
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

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

----------------  
--Name: Robin Karlsson
--Date: 2010-09-29
--Desc: Added handling of custom efficiency shrinkage rows  
---------------- 
CREATE TABLE [dbo].[CustomEfficiencyShrinkage](
	[Id] [uniqueidentifier] NOT NULL,
	[OrderIndex] [int] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
	[ShrinkageName] [nvarchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[CustomEfficiencyShrinkage]  WITH CHECK ADD  CONSTRAINT [FK_CustomEfficiencyShrinkage_BudgetGroup] FOREIGN KEY([Parent])
REFERENCES [dbo].[BudgetGroup] ([Id])
GO

ALTER TABLE [dbo].[CustomEfficiencyShrinkage] CHECK CONSTRAINT [FK_CustomEfficiencyShrinkage_BudgetGroup]
GO

CREATE TABLE [dbo].[CustomEfficiencyShrinkageBudget](
	[Parent] [uniqueidentifier] NOT NULL,
	[Value] [float] NOT NULL,
	[CustomEfficiencyShrinkage] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Parent] ASC,
	[CustomEfficiencyShrinkage] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[CustomShrinkageBudget](
	[Parent] [uniqueidentifier] NOT NULL,
	[Value] [float] NOT NULL,
	[CustomShrinkage] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Parent] ASC,
	[CustomShrinkage] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[CustomEfficiencyShrinkageBudget]  WITH CHECK ADD  CONSTRAINT [FK_CustomEfficiencyShrinkageBudget_BudgetDay] FOREIGN KEY([Parent])
REFERENCES [dbo].[BudgetDay] ([Id])
GO

ALTER TABLE [dbo].[CustomEfficiencyShrinkageBudget] CHECK CONSTRAINT [FK_CustomEfficiencyShrinkageBudget_BudgetDay]
GO

ALTER TABLE [dbo].[CustomShrinkageBudget]  WITH CHECK ADD  CONSTRAINT [FK_CustomShrinkageBudget_BudgetDay] FOREIGN KEY([Parent])
REFERENCES [dbo].[BudgetDay] ([Id])
GO

ALTER TABLE [dbo].[CustomShrinkageBudget] CHECK CONSTRAINT [FK_CustomShrinkageBudget_BudgetDay]
GO

----------------  
--Name: Robin Karlsson
--Date: 2010-10-21
--Desc: Add BudgetGroup to PersonPeriod
----------------  
ALTER TABLE [dbo].[PersonPeriod] ADD 
	[BudgetGroup] [uniqueidentifier] NULL

GO

ALTER TABLE [dbo].[PersonPeriod]  WITH CHECK ADD  CONSTRAINT [FK_PersonPeriod_BudgetGroup] FOREIGN KEY([BudgetGroup])
REFERENCES [dbo].[BudgetGroup] ([Id])
GO

ALTER TABLE [dbo].[PersonPeriod] CHECK CONSTRAINT [FK_PersonPeriod_BudgetGroup]
GO

 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (305,'7.1.305') 
