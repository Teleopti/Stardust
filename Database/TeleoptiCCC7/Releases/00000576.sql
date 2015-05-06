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
