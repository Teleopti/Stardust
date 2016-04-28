----------------  
--Name: Rob Wright
--Date: 2016-03-05
--Desc: Add new column "AbsenceRequestCancellationThreshold" for table WorkflowControlSet
---------------- 

IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'AbsenceRequestCancellationThreshold' AND Object_ID = Object_ID(N'dbo.WorkflowControlSet'))
	RETURN

ALTER TABLE dbo.[WorkflowControlSet]
ADD [AbsenceRequestCancellationThreshold] int NULL
DEFAULT 0

GO
