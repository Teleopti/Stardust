----------------  
--Name: Jianguang Fang/Mingdi Wu
--Date: 2015-02-03
--Desc: Add new column "AnonymousTrading" for table WorkflowControlSet
---------------- 


IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'AnonymousTrading' AND Object_ID = Object_ID(N'dbo.WorkflowControlSet'))
	RETURN

ALTER TABLE dbo.[WorkflowControlSet]
ADD [AnonymousTrading] bit NOT NULL
DEFAULT 0

GO
