----------------  
--Name: Tamas Balog
--Date: 2014-09-25 
--Desc: Add Fairness type to WorkFlowControlSet table as part of the PBI #28317: Hide the points fairness system (Version 8)
----------------
ALTER TABLE dbo.WorkflowControlSet ADD
	FairnessType tinyint NOT NULL CONSTRAINT DF_WorkflowControlSet_FairnessType DEFAULT 1
GO
UPDATE dbo.WorkflowControlSet
	SET dbo.FairnessType = dbo.UseShiftCategoryFairness 
GO