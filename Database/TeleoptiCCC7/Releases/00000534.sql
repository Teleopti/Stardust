----------------  
--Name: CS
--Date: 2015-02-13
--Desc: Add new column "Rank" for table ShiftCategory
---------------- 


IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'Rank' AND Object_ID = Object_ID(N'dbo.ShiftCategory'))
	RETURN

ALTER TABLE dbo.[ShiftCategory]
ADD [Rank] int NULL

GO