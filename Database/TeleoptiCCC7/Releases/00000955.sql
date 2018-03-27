----------------  
--Name: Robin
--Date: 2018-03-27
--Desc: Add table to store model for shift category selection
----------------  
CREATE TABLE dbo.ShiftCategorySelection
	(
	Id uniqueidentifier NOT NULL,
	UpdatedBy uniqueidentifier NOT NULL,
	UpdatedOn datetime NOT NULL,
	Model nvarchar(MAX) NOT NULL,
	BusinessUnit uniqueidentifier NOT NULL
	)
GO
ALTER TABLE dbo.ShiftCategorySelection ADD CONSTRAINT
	PK_ShiftCategorySelection PRIMARY KEY CLUSTERED 
	(
	Id asc
	) 

GO
ALTER TABLE dbo.ShiftCategorySelection ADD CONSTRAINT
	FK_ShiftCategorySelection_Person_UpdatedBy FOREIGN KEY
	(
	UpdatedBy
	) REFERENCES dbo.Person
	(
	Id
	) 
	
GO
ALTER TABLE dbo.ShiftCategorySelection ADD CONSTRAINT
	FK_ShiftCategorySelection_BusinessUnit FOREIGN KEY
	(
	BusinessUnit
	) REFERENCES dbo.BusinessUnit
	(
	Id
	) 
GO

