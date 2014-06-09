--fix constraint name
declare @conststraint_oldname sysname
declare @conststraint_newname sysname

set @conststraint_newname='DF_fact_schedule_overtime_id'

SELECT @conststraint_oldname='[mart].[' + default_constraints.name + ']'
FROM sys.all_columns
INNER JOIN sys.tables
	ON all_columns.object_id = tables.object_id
INNER JOIN sys.schemas
	ON tables.schema_id = schemas.schema_id
INNER JOIN sys.default_constraints
	ON all_columns.default_object_id = default_constraints.object_id
WHERE schemas.name = 'mart'
    AND tables.name = 'fact_schedule'
    AND all_columns.name = 'overtime_id'

exec sp_rename @objname=@conststraint_oldname, @newname=@conststraint_newname,@objtype='object'

--add index that might have been shipped in early 392
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dim_person_person_id_time_zone_id')
CREATE NONCLUSTERED INDEX IX_dim_person_person_id_time_zone_id
ON [mart].[dim_person]
(
	[person_id] ASC,
	[time_zone_id] ASC
)
GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (396,'7.5.396') 
