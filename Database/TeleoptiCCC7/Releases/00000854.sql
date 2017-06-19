----------------  
--Name: Jianfeng
--Date: 2017-06-19
--Desc: Add new column "AbsenceProbabilityEnabled" for table WorkflowControlSet
---------------- 

IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'AbsenceProbabilityEnabled' AND Object_ID = Object_ID(N'dbo.WorkflowControlSet'))
	RETURN

ALTER TABLE dbo.[WorkflowControlSet]
ADD [AbsenceProbabilityEnabled] bit NOT NULL
DEFAULT 0

GO
