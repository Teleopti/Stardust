----------------  
--Name: Anders (but David's code...)
--Desc: bug #33307 
---------------- 
--First we must fix all incorrectly named PK's on Azure
DECLARE @tablename sysname
DECLARE @pkname sysname
DECLARE @newpkname sysname
DECLARE @schema sysname
DECLARE @DynamicSQL nvarchar(4000)

DECLARE cur CURSOR FOR
SELECT 
s2.name AS tablename, 
s1.name AS PKname,
s3.name
FROM sys.objects s1
INNER JOIN sys.objects s2 ON s2.object_id = s1.parent_object_id
inner join sys.schemas s3
on s1.schema_id = s3.schema_id
WHERE OBJECTPROPERTY(s1.object_id, N'IsPrimaryKey') = 1
AND s1.name <> 'PK_'+s2.name
ORDER BY s1.name;
OPEN cur;
            FETCH NEXT FROM cur INTO @tablename, @pkname, @schema;
            WHILE @@FETCH_STATUS = 0
            BEGIN
            SET @newpkname = 'PK_' + @tablename
            SELECT @DynamicSQL = 'sp_rename N''[' + @schema + '].[' + @tablename + '].['+ @pkname +']'', N'''+ @newpkname +''', N''INDEX'''

            PRINT @pkname + ' > ' + @newpkname
            PRINT @DynamicSQL 
            EXEC sp_executesql @DynamicSQL 


FETCH NEXT FROM cur INTO @tablename, @pkname, @schema;
END
CLOSE cur;
DEALLOCATE cur;
GO

--Now we can fix the bug
----------------  
--Name: David
--Desc: bug #33307 - database CPU and IO load is too high. Table scan
---------------- 
--Only apply if CIX does not already exist (delivered add-hoc/manually by Service Desk)
IF NOT EXISTS (
	SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PreferenceDay]')
	AND name = N'CIX_PreferenceDay_DatePersonBU'
	)
BEGIN
	ALTER TABLE [dbo].[PreferenceDay] DROP CONSTRAINT [FK_PreferenceDay_BusinessUnit]
	ALTER TABLE [dbo].[PreferenceDay] DROP CONSTRAINT [FK_PreferenceDay_Person]
	ALTER TABLE [dbo].[PreferenceDay] DROP CONSTRAINT [FK_PreferenceDay_Person_UpdatedBy]
	ALTER TABLE [dbo].[PreferenceRestriction] DROP CONSTRAINT [FK_PreferenceRestrictin_PreferenceDay]

	EXEC sp_rename @objname = N'[dbo].[PreferenceDay]', @newname = N'PreferenceDay_old', @objtype = N'OBJECT'
	EXEC sp_rename @objname = N'[dbo].[PreferenceDay_old].[PK_PreferenceDay]', @newname =  N'PK_PreferenceDay_old', @objtype = N'INDEX'

	CREATE TABLE [dbo].[PreferenceDay](
		[Id] [uniqueidentifier] NOT NULL,
		[Version] [int] NOT NULL,
		[UpdatedBy] [uniqueidentifier] NOT NULL,
		[UpdatedOn] [datetime] NOT NULL,
		[Person] [uniqueidentifier] NOT NULL,
		[RestrictionDate] [datetime] NOT NULL,
		[BusinessUnit] [uniqueidentifier] NOT NULL,
		[TemplateName] [nvarchar](50) NULL
		)

	--PK non-clustered
	ALTER TABLE [dbo].[PreferenceDay] ADD  CONSTRAINT [PK_PreferenceDay] PRIMARY KEY NONCLUSTERED 
	(
		[Id] ASC
	)
	
	--Clustered index
	CREATE CLUSTERED INDEX [CIX_PreferenceDay_DatePersonBU] ON [dbo].[PreferenceDay]
	(
		   [RestrictionDate] ASC,
		   [Person] ASC,
		   [BusinessUnit] ASC
	)

	--Get the data
	INSERT INTO [dbo].[PreferenceDay]
	SELECT * FROM [dbo].[PreferenceDay_old]

	--re-Add FKs
	ALTER TABLE [dbo].[PreferenceDay]  WITH NOCHECK ADD  CONSTRAINT [FK_PreferenceDay_BusinessUnit] FOREIGN KEY([BusinessUnit])
	REFERENCES [dbo].[BusinessUnit] ([Id])
	ALTER TABLE [dbo].[PreferenceDay] CHECK CONSTRAINT [FK_PreferenceDay_BusinessUnit]

	ALTER TABLE [dbo].[PreferenceDay]  WITH NOCHECK ADD  CONSTRAINT [FK_PreferenceDay_Person] FOREIGN KEY([Person])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[PreferenceDay] CHECK CONSTRAINT [FK_PreferenceDay_Person]
	
	ALTER TABLE [dbo].[PreferenceDay]  WITH NOCHECK ADD  CONSTRAINT [FK_PreferenceDay_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[PreferenceDay] CHECK CONSTRAINT [FK_PreferenceDay_Person_UpdatedBy]

	ALTER TABLE [dbo].[PreferenceRestriction]  WITH NOCHECK ADD  CONSTRAINT [FK_PreferenceRestrictin_PreferenceDay] FOREIGN KEY([Id])
	REFERENCES [dbo].[PreferenceDay] ([Id])
	ALTER TABLE [dbo].[PreferenceRestriction] CHECK CONSTRAINT [FK_PreferenceRestrictin_PreferenceDay]

	--Clean up
	DROP TABLE [dbo].[PreferenceDay_old]
END
GO
