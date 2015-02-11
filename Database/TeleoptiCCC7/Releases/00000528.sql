----------------  
--Name: Jianguang Fang/Mingdi Wu
--Date: 2015-02-11
--Desc: Add new column "LockTrading" for table WorkflowControlSet
---------------- 


IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'LockTrading' AND Object_ID = Object_ID(N'dbo.WorkflowControlSet'))
	RETURN

ALTER TABLE dbo.[WorkflowControlSet]
ADD [LockTrading] bit NOT NULL
DEFAULT 0

GO
