ALTER TABLE dbo.DayOffRules ADD
	AgentGroup uniqueidentifier NULL 

ALTER TABLE dbo.DayOffRules 
ADD CONSTRAINT FK_DayOffRules_AgentGroup
FOREIGN KEY (AgentGroup) REFERENCES dbo.AgentGroup

DROP INDEX UQ_DayOffRules_DefaultSettings ON [dbo].[DayOffRules]

CREATE UNIQUE NONCLUSTERED INDEX [UQ_DayOffRules_DefaultSettings] ON [dbo].[DayOffRules]
(
	[DefaultSettings] ASC,
	[BusinessUnit] ASC,
	[AgentGroup] ASC
)
WHERE ([DefaultSettings]=(1))

GO

INSERT INTO dbo.DayOffRules (Id, UpdatedBy, UpdatedOn, BusinessUnit, MinDayOffsPerWeek, MaxDayOffsPerWeek, MinConsecutiveWorkDays, MaxConsecutiveWorkDays, MinConsecutiveDayOffs, MaxConsecutiveDayOffs, DefaultSettings, Name, AgentGroup)
       SELECT newid(), UpdatedBy, UpdatedOn, BusinessUnit, 1, 3, 2, 6, 1, 3, 1, 'Default', Id
       FROM dbo.AgentGroup;

GO