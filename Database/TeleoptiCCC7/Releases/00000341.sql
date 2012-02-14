-- =============================================
-- Author:		Xianwei Shen
-- Create date: 2011-11-16
-- Description:	Add TotalAllowance and Allowance into budget day, add a
-- Change:		
-- =============================================
ALTER TABLE dbo.BudgetDay ADD
	TotalAllowance float(53) NOT NULL CONSTRAINT DF_BudgetDay_TotalAllowance DEFAULT 0,
	Allowance float(53) NOT NULL CONSTRAINT DF_BudgetDay_Allowance DEFAULT 0
GO

CREATE TABLE [dbo].[BudgetAbsenceCollection](
	[CustomShrinkage] [uniqueidentifier] NOT NULL, 
	[Absence] [uniqueidentifier] NOT NULL
) 

ALTER TABLE dbo.BudgetAbsenceCollection ADD CONSTRAINT
	PK_BudgetAbsenceCollection PRIMARY KEY CLUSTERED 
	(
	CustomShrinkage,
	Absence
	)

ALTER TABLE [dbo].[BudgetAbsenceCollection]  WITH CHECK ADD  CONSTRAINT [FK_BudgetAbsenceCollection_Absence] FOREIGN KEY([Absence])
REFERENCES [dbo].[Absence] ([Id])

ALTER TABLE [dbo].[BudgetAbsenceCollection] CHECK CONSTRAINT [FK_BudgetAbsenceCollection_Absence]

ALTER TABLE [dbo].[BudgetAbsenceCollection]  WITH CHECK ADD  CONSTRAINT [FK_BudgetAbsenceCollection_CustomShrinkage] FOREIGN KEY([CustomShrinkage])
REFERENCES [dbo].[CustomShrinkage] ([Id])

ALTER TABLE [dbo].[BudgetAbsenceCollection] CHECK CONSTRAINT [FK_BudgetAbsenceCollection_CustomShrinkage]
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (341,'7.1.341') 
