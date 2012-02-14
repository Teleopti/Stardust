/* 
Trunk initiated: 
2010-06-02 
10:39
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Robin Karlsson
--Date: 2010-06-03
--Desc: Added column that specifies if shift trades should be left pending or auto granted
----------------  
ALTER TABLE dbo.WorkflowControlSet ADD [AutoGrantShiftTradeRequest] [bit] NULL
GO
UPDATE dbo.WorkflowControlSet SET [AutoGrantShiftTradeRequest]=0
GO
ALTER TABLE dbo.WorkflowControlSet ALTER COLUMN [AutoGrantShiftTradeRequest] [bit] NOT NULL
GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (271,'7.1.271') 
