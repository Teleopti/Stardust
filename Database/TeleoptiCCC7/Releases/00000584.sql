update [dbo].[WorkflowControlSet]
set FairnessType = 1
where FairnessType = 0

DROP TABLE [dbo].[ShiftCategoryJusticeValues]
