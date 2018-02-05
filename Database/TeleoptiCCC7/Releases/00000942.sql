----------------  
--Name: CodeMonkeys
--Date: 2018-02-05
--Desc: Add MaximumContinuousWorkTime setting for overtime request
----------------  

ALTER TABLE [dbo].[WorkflowControlSet]
ADD  OvertimeRequestMaximumContinuousWorkTime BIGINT,
	OvertimeRequestMinimumRestTimeThreshold BIGINT,
	OvertimeRequestMaximumContinuousWorkTimeEnabled BIT NOT NULL DEFAULT(0),
	OvertimeRequestMaximumContinuousWorkTimeHandleType INT 
