----------------  
--Name: Mingdi
--Date: 2018-03-30
--Desc: add a new property of GamificationSetting for rolling period setting
--      0 means Ongoing, 1 means weekly, 2 means monthly
----------------  

alter table dbo.GamificationSetting
add RollingPeriodSet int not null default 0
go