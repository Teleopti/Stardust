/* 
BuildTime is: 
2008-11-26 
12:26
*/ 
/* 
Trunk initiated: 
2008-11-19 
16:38
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: David Jonsson
--Date: 2008-11-19
--Desc: Critical IX for MainShiftActivityLayer
--Removed! We will build a clustered index on parent instead
----------------
--We did force this index into test, outside the release ....
--Result: I'm breaking the version control => IF EXISTS

--IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MainShiftActivityLayer]') AND name = N'IX_MainShiftActivityLayer_Parent_Minimum_Maximum')
--CREATE NONCLUSTERED INDEX [IX_MainShiftActivityLayer_Parent_Minimum_Maximum] ON [dbo].[MainShiftActivityLayer] 
--(
	--[Parent] ASC,
	--[Minimum] ASC,
	--[Maximum] ASC
--)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
--GO

----------------  
--Name: HENRIK Andersson
--Date: 2008-11-19
--Desc: Absence on PersonAccount instead of description 
----------------  
alter table PersonAccount add TrackingAbsence UNIQUEIDENTIFIER
alter table PersonAccount add constraint FK_PersonAccount_TrackingAbsence foreign key (TrackingAbsence) references Absence
GO
----------------  
--Name: Zoë Trender
--Date: 2008-11-19
--Desc: InWorkTime and InPaidTime added to Absence and Activity 
----------------  
----------------  
--Name: Zoë Trender
--Date: 2008-11-19
--Desc: InWorkTime and InPaidTime added to Absence and Activity
--Updated by David to avoid DF in SQL Server (would create a diff vs. Nhib)
--The column is brand new so need for WHERE InPaidTime IS NULL in the UPDATE
----------------
ALTER TABLE Activity 
ADD InPaidTime bit NULL
GO
UPDATE Activity
SET InPaidTime = 0
ALTER TABLE Activity 
ALTER COLUMN InPaidTime bit NOT NULL
---  
ALTER TABLE Activity 
ADD InWorkTime bit NULL
GO
UPDATE Activity
SET InWorkTime = 0
ALTER TABLE Activity 
ALTER COLUMN InWorkTime bit NOT NULL
---
ALTER TABLE Absence 
ADD InWorkTime bit NULL
GO
UPDATE Absence
SET InWorkTime = 0
ALTER TABLE Absence 
ALTER COLUMN InWorkTime bit NOT NULL
---
ALTER TABLE Absence 
ADD InPaidTime bit NULL
GO
UPDATE Absence
SET InPaidTime = 0
ALTER TABLE Absence 
ALTER COLUMN InPaidTime bit NOT NULL
----------------  
--Name: Robin Karlsson
--Date: 2008-11-21
--Desc: IsLogOutState added to RtaStateGroup 
--Updated by David to avoid DF in SQL Server (would create a diff vs. Nhib)
----------------  
ALTER TABLE RtaStateGroup 
ADD IsLogOutState bit NULL
GO
UPDATE RtaStateGroup 
SET IsLogOutState = 0
GO
ALTER TABLE RtaStateGroup 
ALTER COLUMN IsLogOutState bit NOT NULL
GO
----------------  
--Name: Robin Karlsson
--Date: 2008-11-21
--Desc: Remove not null for column activity in table
----------------  
alter table StateGroupActivityAlarm
alter column Activity uniqueidentifier null
GO

----------------  
--Name: David Jonsson
--Date: 2008-11-21
--Desc: Nhib diff
----------------  
ALTER TABLE License
ALTER COLUMN XmlString [nvarchar] (4000) NOT NULL

----------------  
--Name: Ola Håkansson
--Date: 2008-11-24
--Desc: Some indexes to speed up the loading of SkillDays
--Changed: David Jonsson IX => CIX for clustered indexes. Typo error on CIX_TemplateTaskPeriod(?), not created as clusted
---------------- 
-- Index on Parent on WorkloadDay
CREATE NONCLUSTERED INDEX IX_WorkloadDay_Parent ON dbo.WorkloadDay
	(
	Parent
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

-- Index on Workload on WorkloadDayBase
CREATE NONCLUSTERED INDEX IX_WorkloadDayBase_Workload ON dbo.WorkloadDayBase
	(
	Workload
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

-- Change the clustered index on TemplateTaskPeriod from  Id to Parent
ALTER TABLE dbo.TemplateTaskPeriod
	DROP CONSTRAINT PK_TemplateTaskPeriod
GO
ALTER TABLE dbo.TemplateTaskPeriod ADD CONSTRAINT
	PK_TemplateTaskPeriod PRIMARY KEY NONCLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE CLUSTERED INDEX CIX_TemplateTaskPeriod ON dbo.TemplateTaskPeriod
	(
	Parent
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

-- Change the clustered index on SkillDataPeriod from Id to Parent
ALTER TABLE dbo.SkillDataPeriod
	DROP CONSTRAINT PK_SkillDataPeriod
GO
ALTER TABLE dbo.SkillDataPeriod ADD CONSTRAINT
	PK_SkillDataPeriod PRIMARY KEY NONCLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE CLUSTERED INDEX CIX_SkillDataPeriod_Parent ON dbo.SkillDataPeriod
	(
	Parent
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

-- Index on OpenHourList
CREATE CLUSTERED INDEX [CIX_OpenHourList_Parent] ON [dbo].[OpenHourList] 
(
	[Parent] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

GO

----------------  
--Name: David Jonsson
--Date: 2008-11-25
--Desc: Some indexes to speed up the loading of Shifts:
--MainShiftActivityLayer
--PersonalShiftActivityLayer
--MainShift
---------------
--Drop the Panic IX created Add-hoc in some environments:
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MainShiftActivityLayer]') AND name = N'IX_MainShiftActivityLayer_Parent_Minimum_Maximum')
DROP INDEX IX_MainShiftActivityLayer_Parent_Minimum_Maximum ON dbo.MainShiftActivityLayer
GO

--MainShiftActivityLayer
--Drop PK, OK since it's not referenced by any other table
ALTER TABLE dbo.MainShiftActivityLayer
	DROP CONSTRAINT PK_MainShiftActivityLayer
GO
--Add same PK, but as NONCLUSTERED
ALTER TABLE dbo.MainShiftActivityLayer ADD CONSTRAINT
	PK_MainShiftActivityLayer PRIMARY KEY NONCLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
--Rebuild Clustered Index on Parent
CREATE CLUSTERED INDEX CIX_MainShiftActivityLayer_Parent ON dbo.MainShiftActivityLayer
	(
	Parent
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

--PersonalShiftActivityLayer
--Drop PK, OK since it's not referenced by any other table
ALTER TABLE dbo.PersonalShiftActivityLayer
	DROP CONSTRAINT PK_PersonalShiftActivityLayer
GO
--Add same PK, but as NONCLUSTERED
ALTER TABLE dbo.PersonalShiftActivityLayer ADD CONSTRAINT
	PK_PersonalShiftActivityLayer PRIMARY KEY NONCLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
--Rebuild Clustered Index on Parent
CREATE CLUSTERED INDEX CIX_PersonalShiftActivityLayer ON dbo.PersonalShiftActivityLayer
	(
	Parent
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

--MainShift
--Add index to support ShiftCategory joins
CREATE NONCLUSTERED INDEX IX_MainShift_ShiftCategory ON dbo.MainShift
	(
	ShiftCategory
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

----------------  
--Name: Madhuranga Pinnagoda
--Date: 2008-11-25
--Desc: Altered PreferenceRestriction to use DayOff domain rather struct.
--In PreferenceRestriction drop COLUMN DayOff & ADD DayOff uniqueidentifier & its ref key 
---------------
ALTER TABLE PreferenceRestriction
DROP COLUMN DayOff
GO
ALTER TABLE PreferenceRestriction
ADD DayOff uniqueidentifier null
GO
ALTER TABLE [PreferenceRestriction]  WITH CHECK ADD  CONSTRAINT [FK_PreferenceRestriction_DayOff] FOREIGN KEY([DayOff])
REFERENCES [DayOff] ([Id])
GO
ALTER TABLE [PreferenceRestriction] CHECK CONSTRAINT [FK_PreferenceRestriction_DayOff]
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (13,'7.0.13') 
