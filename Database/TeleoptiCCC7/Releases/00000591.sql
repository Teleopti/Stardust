----------------  
--Name: Mingdi Wu
--Date: 2015-05-26  
--Desc: use toggle instead delete
---------------- 

UPDATE [dbo].[ApplicationFunction] SET IsDeleted = 0
WHERE ForeignSource='Raptor' AND IsDeleted=1 AND ForeignId Like('0120%') AND FunctionCode='Outbound'

UPDATE [dbo].[ApplicationFunction] SET IsDeleted = 0
WHERE ForeignSource='Raptor' AND IsDeleted=1 AND ForeignId Like('0107%') AND FunctionCode='SeatPlanner'