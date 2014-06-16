----------------  
--Name: Jonas, Kunning
--Date: 2014-06-13  
--Desc: Remove UnderConstruction and MobileReports from application functions
----------------  
delete from ApplicationFunctionInRole 
where ApplicationFunction in (select Id from ApplicationFunction where FunctionCode in ('UnderConstruction', 'MobileReports'))

delete from ApplicationFunction where FunctionCode in ('UnderConstruction', 'MobileReports')

GO

