----------------  
--Name: Mingdi
--Date: 2017-06-20
--Desc: Add new column "OvertimeProbabilityEnabled" for table WorkflowControlSet
---------------- 

IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'OvertimeProbabilityEnabled' AND Object_ID = Object_ID(N'dbo.WorkflowControlSet'))
	RETURN

ALTER TABLE dbo.[WorkflowControlSet]
ADD OvertimeProbabilityEnabled bit NOT NULL
DEFAULT 0

GO