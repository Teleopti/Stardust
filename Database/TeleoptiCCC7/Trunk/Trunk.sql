--Empty the table, must be filled using the security tool!
DELETE FROM [dbo].[LicenseStatus]
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