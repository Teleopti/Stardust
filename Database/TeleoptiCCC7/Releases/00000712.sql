IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[RuleMappings]') AND type in (N'U'))
BEGIN
	DROP TABLE [ReadModel].[RuleMappings]
END

GO

CREATE TABLE [ReadModel].[RuleMappings] (
	BusinessUnitId uniqueidentifier,
	StateCode nvarchar(25),
	PlatformTypeId uniqueidentifier,
	StateGroupId uniqueidentifier,
	StateGroupName nvarchar(max),
	ActivityId uniqueidentifier,
	RuleId uniqueidentifier,
	RuleName nvarchar(max),
	Adherence int,
	StaffingEffect float,
	DisplayColor int,
	IsAlarm bit,
	ThresholdTime int,
	AlarmColor int
)

GO 

CREATE CLUSTERED INDEX [IDX_RuleMappings] ON [ReadModel].[RuleMappings] (
	[ActivityId] ASC,
	[StateCode] ASC
)

GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[KeyValueStore]') AND type in (N'U'))
BEGIN
	DROP TABLE [ReadModel].[KeyValueStore]
END

GO

CREATE TABLE [ReadModel].[KeyValueStore] (
	[Key] nvarchar(100) not null,
	Value nvarchar(max)

	CONSTRAINT [PK_KeyValueStore] PRIMARY KEY CLUSTERED ([Key])
)