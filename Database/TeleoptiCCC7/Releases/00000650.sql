CREATE NONCLUSTERED INDEX IX_ExternalLogOn
ON [dbo].[ExternalLogOn] ([AcdLogOnOriginalId],[DataSourceId])

CREATE NONCLUSTERED INDEX IX_ExternalLogOnCollection
ON [dbo].[ExternalLogOnCollection] ([ExternalLogOn])
INCLUDE ([PersonPeriod])