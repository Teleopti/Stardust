--Name: Chundan Xu, Mingdi Wu
--Date: 2014-05-20
--Desc: Add minTimePerWeek to work flow control set table
----------------  

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[WorkflowControlSet]') 
         AND name = 'MinTimePerWeek'
)
BEGIN
	ALTER TABLE [dbo].[WorkflowControlSet]
	ADD MinTimePerWeek bigint null;
END
GO