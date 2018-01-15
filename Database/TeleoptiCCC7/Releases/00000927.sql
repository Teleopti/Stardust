----------------  
--Name: Mingdi
--Date: 2018-01-15
--Desc: unify the data type between BadgeSetting and ExternalPerformanceData
----------------  

alter table [dbo].[BadgeSetting]
alter column Threshold float 
go

alter table [dbo].[BadgeSetting]
alter column BronzeThreshold float
go

alter table [dbo].[BadgeSetting]
alter column SilverThreshold float
go

alter table [dbo].[BadgeSetting]
alter column GoldThreshold float
go