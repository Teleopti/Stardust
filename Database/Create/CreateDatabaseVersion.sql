/*******************************************************************************/
CREATE TABLE dbo.DatabaseVersion
(
	BuildNumber				INT					NOT NULL,
	SystemVersion			VARCHAR(100)		NOT NULL,
--	SchemaName				VARCHAR(200)		NOT NULL,	
	AddedDate				DATETIME			NOT NULL,
	AddedBy					VARCHAR(1000)		NOT NULL
)

--Add PK
ALTER TABLE dbo.DatabaseVersion ADD CONSTRAINT
PK_DatabaseVersion PRIMARY KEY CLUSTERED 
(
BuildNumber
)

--Default values
ALTER TABLE dbo.DatabaseVersion ADD CONSTRAINT
	DF_DatabaseVersion_AddedDate DEFAULT CURRENT_TIMESTAMP
	FOR AddedDate

ALTER TABLE dbo.DatabaseVersion ADD CONSTRAINT
	DF_DatabaseVersion_AddedBy DEFAULT SYSTEM_USER
	FOR AddedBy
	
--Add version 0 for 'dbo'
INSERT INTO dbo.DatabaseVersion (BuildNumber, SystemVersion)
VALUES (0, 'Not defined')