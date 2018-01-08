----------------  
--Name: Mingdi
--Date: 2018-01-08
--Desc: rename column from "UnitType" to "DataType"
---------------- 

exec sp_rename 'dbo.BadgeSetting.UnitType', 'DataType', 'COLUMN'
go