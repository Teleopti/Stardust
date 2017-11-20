----------------  
--Name: Mingdi
--Date: 2017-11-17
--Desc: Rename table for furture use both internal and external badge setting.
----------------  

EXEC sp_rename 'dbo.ExternalBadgeSetting', 'BadgeSetting'
