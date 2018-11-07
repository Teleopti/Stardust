ALTER TABLE [rta].[Events] ADD [BelongsToDate] [datetime] NULL
GO
ALTER TABLE [rta].[Events] ADD [StoreVersion] int NULL
GO
UPDATE rta.Events SET [StoreVersion] = 1
GO
