----------------  
--Name: Mingdi Wu
--Date: 2015-05-22  
--Desc: delete Outbound and SeatPLanner in permission setting since it is not ready for release to customer
---------------- 

UPDATE [dbo].[ApplicationFunction] SET IsDeleted = 1
WHERE ForeignSource='Raptor' AND IsDeleted=0 AND ForeignId Like('0120%') AND FunctionCode='Outbound'

UPDATE [dbo].[ApplicationFunction] SET IsDeleted = 1
WHERE ForeignSource='Raptor' AND IsDeleted=0 AND ForeignId Like('0107%') AND FunctionCode='SeatPlanner'