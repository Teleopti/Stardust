
ALTER TABLE ReadModel.ExternalLogon 
ADD [TimeZone] nvarchar(50) NULL 
GO

UPDATE ReadModel.KeyValueStore SET [Value] = 'True' WHERE [Key] = 'PersonAssociationChangedPublishTrigger'
GO
