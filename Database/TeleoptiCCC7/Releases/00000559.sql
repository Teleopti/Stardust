----------------  
--Name: Asad
--Desc: Added table for planning period. 
---------------- 
CREATE TABLE dbo.PlanningPeriod
	(
	Id uniqueidentifier NOT NULL,
	BusinessUnit uniqueidentifier NOT NULL,
	UpdatedBy uniqueidentifier NOT NULL,
	UpdatedOn datetime NOT NULL,
	StartDate datetime NOT NULL,
	EndDate datetime NOT NULL
	)  
GO

ALTER TABLE dbo.PlanningPeriod ADD CONSTRAINT
	PK_PlanningPeriod PRIMARY KEY CLUSTERED 
	(
	Id
	) 
GO
ALTER TABLE dbo.PlanningPeriod ADD CONSTRAINT
	FK_PlanningPeriod_BusinessUnit FOREIGN KEY
	(
	BusinessUnit
	) REFERENCES dbo.BusinessUnit
	(
	Id
	) 
	
GO
ALTER TABLE dbo.PlanningPeriod ADD CONSTRAINT
	FK_PlanningPeriod_Person_UpdatedBy FOREIGN KEY
	(
	UpdatedBy
	) REFERENCES dbo.Person
	(
	Id
	) 
GO
