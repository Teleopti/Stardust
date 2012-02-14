/* 
Trunk initiated: 
2011-06-27 
11:26
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Peter Westlin  
--Date: 2011-06-07  
--Desc: Closed property for budgetday  
----------------


alter table dbo.BudgetDay 
		add IsClosed BIT Null

GO

update dbo.BudgetDay set IsClosed = 0

GO

alter table dbo.BudgetDay
	alter column IsClosed BIT Not null
  

----------------  
--Name: Roger Kratz 
--Date: 2011-07-23  
--Desc: Removed MainShift.Name  
----------------
alter table dbo.MainShift
drop column Name 
GO

alter table dbo.MainShift_AUD
drop column Name
GO

alter table AuditTrail.MainShift
drop column Name
GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (331,'7.1.331') 
