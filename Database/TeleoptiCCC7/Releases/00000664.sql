----------------  
--Name: Rob Wright/Xue Li
--Date: 2016-02-06
--Desc: Add new column "AbsenceRequestWaitlistEnabled" for table WorkflowControlSet
---------------- 

IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'AbsenceRequestWaitlistEnabled' AND Object_ID = Object_ID(N'dbo.WorkflowControlSet'))
	RETURN

ALTER TABLE dbo.[WorkflowControlSet]
ADD [AbsenceRequestWaitlistEnabled] bit NOT NULL
DEFAULT 0

GO
