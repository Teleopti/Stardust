ALTER TABLE [dbo].[OptionalColumnValue] DROP CONSTRAINT [FK_OptionalColumnValue_OptionalColumn]
GO

DROP INDEX [CIX_OptionalColumnValue_ReferenceId] ON [dbo].[OptionalColumnValue] WITH ( ONLINE = OFF )
GO

DROP INDEX [IX_OptionalColumnValue_AK1] ON [dbo].[OptionalColumnValue]
GO

EXEC dbo.sp_rename @objname = N'[dbo].[OptionalColumnValue]', @newname = N'OptionalColumnValue_old', @objtype = N'OBJECT'
EXEC dbo.sp_rename @objname = N'[dbo].[OptionalColumnValue_old].[PK_OptionalColumnValue]', @newname = N'PK_OptionalColumnValue_old', @objtype =N'INDEX'


CREATE TABLE [dbo].[OptionalColumnValue](
	[Id] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[ReferenceId] [uniqueidentifier] NOT NULL,
	[Parent] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_OptionalColumnValue] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)
GO


CREATE CLUSTERED INDEX [CIX_OptionalColumnValue_Parent] ON [dbo].[OptionalColumnValue]
(
	[Parent] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_OptionalColumnValue_AK1] ON [dbo].[OptionalColumnValue]
(
	[ReferenceId] ASC
)
INCLUDE ( 	[Id],
	[Description],
	[Parent])
GO


ALTER TABLE [dbo].[OptionalColumnValue]  WITH CHECK ADD  CONSTRAINT 
	[FK_OptionalColumnValue_OptionalColumn] FOREIGN KEY([ReferenceId])
	REFERENCES [dbo].[OptionalColumn] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[OptionalColumnValue] CHECK CONSTRAINT [FK_OptionalColumnValue_OptionalColumn]
GO

ALTER TABLE dbo.OptionalColumnValue 
	ADD CONSTRAINT FK_OptionalColumnValue_Person FOREIGN KEY (Parent)
	REFERENCES dbo.Person (Id)
GO

-- Insert back data and swap columns so Parent is Person and RefrerenceId is OptionalColumn
INSERT INTO [dbo].[OptionalColumnValue] (Id, Description, ReferenceId, Parent)
	SELECT Id, Description, Parent, ReferenceId
	FROM [dbo].[OptionalColumnValue_old]
GO

DROP TABLE [dbo].[OptionalColumnValue_old]
GO