
CREATE TABLE ReadModel.ExternalLogon
(
	PersonId uniqueidentifier NULL,
	DataSourceId int NULL,
	UserCode varchar(130) NULL,
	Deleted bit NULL default 0,
	Added bit NULL default 0
)
GO

UPDATE ReadModel.KeyValueStore SET Value = 'True' WHERE [Key] = 'PersonAssociationChangedPublishTrigger'
GO
