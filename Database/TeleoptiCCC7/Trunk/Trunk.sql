
/*
Make sure that all persons in the database has write protection info. This is created automatically in the domain.
*/

INSERT INTO [dbo].[PersonWriteProtectionInfo] (Id,CreatedBy,UpdatedBy,CreatedOn,UpdatedOn,PersonWriteProtectedDate) SELECT p.Id,p.CreatedBy,p.UpdatedBy,p.CreatedOn,p.UpdatedOn,null FROM [dbo].[Person] p WHERE p.Id NOT IN (SELECT id FROM [dbo].[PersonWriteProtectionInfo])
GO

/*
We have changed the lowest possible resolution to one hour to avoid issues with daylight savings.
*/

UPDATE dbo.Skill SET DefaultResolution = 60 WHERE DefaultResolution > 60
GO

----------------  
--Name: David Jonsson
--Date: 2012-03-23
--Desc: #18738 - very slow to fetch request from MyTimeWeb
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShiftTradeSwapDetail]') AND name = N'IX_ShiftTradeSwapDetail_Parent')
CREATE NONCLUSTERED INDEX IX_ShiftTradeSwapDetail_Parent
ON [dbo].[ShiftTradeSwapDetail] ([Parent])
INCLUDE ([PersonFrom],[PersonTo])
GO

----------------  
--Name: AndersF
--Date: 2012-03-30
--Desc: #18789 - Performance: The person day off query is now one of the most resource intense things on sql
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonDayOff]') AND name = N'IX_PersonDayOff_Scenario_BU_Anchor')
CREATE NONCLUSTERED INDEX [IX_PersonDayOff_Scenario_BU_Anchor]
ON [dbo].[PersonDayOff] ([Scenario],[BusinessUnit],[Anchor])
INCLUDE ([Id],[Version],[CreatedBy],[UpdatedBy],[CreatedOn],[UpdatedOn],[Person],[TargetLength],[Flexibility],[Name],[ShortName],[DisplayColor],[PayrollCode])
GO

----------------  
--Name: RobinK
--Date: 2012-04-11
--Desc: #18919 - Require unique items in table to avoid future problems!
----------------  
BEGIN
CREATE TABLE #sgaa
(
	Activity uniqueidentifier null,
	StateGroup uniqueidentifier null,
	BusinessUnit uniqueidentifier null
)

INSERT INTO #sgaa (activity,stategroup,businessunit)
SELECT sgaa1.[Activity]
      ,sgaa1.[StateGroup]
      ,sgaa1.[BusinessUnit]
  FROM [dbo].[StateGroupActivityAlarm] sgaa1
  WHERE sgaa1.IsDeleted = 0
  group by sgaa1.[Activity]
      ,sgaa1.[StateGroup]
      ,sgaa1.[BusinessUnit]
      having count(sgaa1.businessunit) > 1

DELETE sgaa1 FROM [dbo].[StateGroupActivityAlarm] sgaa1 inner join #sgaa sgaa2 on (sgaa2.activity=sgaa1.activity or (sgaa2.activity is null and sgaa1.activity is null)) and (sgaa2.stategroup  = sgaa1.stategroup or (sgaa2.stategroup is null and sgaa1.stategroup is null)) and sgaa2.businessunit=sgaa1.businessunit WHERE sgaa1.IsDeleted = 0

DROP TABLE #sgaa

END
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[StateGroupActivityAlarm]') AND name = N'UQ_StateGroupActivityAlarm')
ALTER TABLE dbo.StateGroupActivityAlarm ADD CONSTRAINT
	UQ_StateGroupActivityAlarm UNIQUE NONCLUSTERED 
	(
	StateGroup,
	Activity,
	BusinessUnit
	)
GO