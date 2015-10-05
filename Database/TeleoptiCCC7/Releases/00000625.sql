/* OnPrem way, is not the only way ...
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'IX_PersonAssignment_Scenario_UpdatedBy_UpdatedOn')
DROP INDEX [IX_PersonAssignment_Scenario_UpdatedBy_UpdatedOn] ON [dbo].[PersonAssignment]

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'IX_PersonAssignment_Date')
DROP INDEX [IX_PersonAssignment_Date] ON [dbo].[PersonAssignment]

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UQ_PersonAssignment_Date_Scenario_Person]') AND type_desc LIKE 'UNIQUE_CONSTRAINT')
ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [UQ_PersonAssignment_Date_Scenario_Person]

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'CIX_PersonAssignment_PersonDate_Scenario')
DROP INDEX [CIX_PersonAssignment_PersonDate_Scenario] ON [dbo].[PersonAssignment]

CREATE UNIQUE CLUSTERED INDEX [CIX_PersonAssignment_PersonDate_Scenario] ON [dbo].[PersonAssignment]
(
	[Person] ASC,
	[Date] ASC,
	[Scenario] ASC
)
*/
--bug #35191 - make it work in Azure
--Drop current FK
ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [FK_PersonAssignment_DayOffTemplate]
ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [FK_PersonAssignment_Person]
ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [FK_PersonAssignment_Person_UpdatedBy]
ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [FK_PersonAssignment_Scenario]
ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [FK_PersonAssignment_ShiftCategory]
ALTER TABLE [dbo].[ShiftLayer] DROP CONSTRAINT [FK_ShiftLayer_PersonAssignment]
GO

--create new table and clustered key
CREATE TABLE [dbo].[PersonAssignment_new](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NOT NULL,
	[ShiftCategory] [uniqueidentifier] NULL,
	[DayOffTemplate] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PersonAssignment_new] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)

CREATE UNIQUE CLUSTERED INDEX [CIX_PersonAssignment_PersonDate_Scenario] ON [dbo].[PersonAssignment_new]
(
                [Person] ASC,
                [Date] ASC,
                [Scenario] ASC
)

--get data into new table
INSERT INTO [dbo].[PersonAssignment_new]
SELECT  Id, Version, UpdatedBy, UpdatedOn, Person, Scenario, Date, ShiftCategory, DayOffTemplate
FROM [dbo].[PersonAssignment]
GO

--rename tables
EXEC dbo.sp_rename @objname = N'[dbo].[PersonAssignment]', @newname = N'PersonAssignment_old', @objtype = N'OBJECT'
EXEC sp_rename N'[dbo].[PersonAssignment_old].[PK_PersonAssignment]', N'PK_PersonAssignment_old', N'INDEX'
EXEC dbo.sp_rename @objname = N'[dbo].[PersonAssignment_new]', @newname = N'PersonAssignment', @objtype = N'OBJECT'
EXEC sp_rename N'[dbo].[PersonAssignment].[PK_PersonAssignment_new]', N'PK_PersonAssignment', N'INDEX'
GO

--re-create FKs
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_DayOffTemplate] FOREIGN KEY([DayOffTemplate])
REFERENCES [dbo].[DayOffTemplate] ([Id])
ALTER TABLE [dbo].[PersonAssignment]  CHECK CONSTRAINT [FK_PersonAssignment_DayOffTemplate]

ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[PersonAssignment]  CHECK CONSTRAINT [FK_PersonAssignment_Person]

ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[PersonAssignment]  CHECK CONSTRAINT [FK_PersonAssignment_Person_UpdatedBy]

ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
ALTER TABLE [dbo].[PersonAssignment]  CHECK CONSTRAINT [FK_PersonAssignment_Scenario]

ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
ALTER TABLE [dbo].[PersonAssignment]  CHECK CONSTRAINT [FK_PersonAssignment_ShiftCategory]

ALTER TABLE [dbo].[ShiftLayer]  WITH CHECK ADD  CONSTRAINT [FK_ShiftLayer_PersonAssignment] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonAssignment] ([Id])
ON DELETE CASCADE
ALTER TABLE [dbo].[ShiftLayer] CHECK CONSTRAINT [FK_ShiftLayer_PersonAssignment]
GO

--drop old table
DROP TABLE [dbo].[PersonAssignment_old]
GO