ALTER TABLE [dbo].[WorkflowControlSet]
ADD [MaximumConsecutiveWorkingDays] int NOT NULL 
CONSTRAINT MAXIMUM_CONSECUTIVE_WORKING_DAYS_DEFAULT_VALUE_CONSTRAINT DEFAULT 9
WITH VALUES
