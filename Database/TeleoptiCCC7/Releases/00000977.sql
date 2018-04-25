  ----------------  
--Name: Mingdi
--Date: 2018-04-25
--Desc: enlarge the value for imported number and thresholds
---------------- 
  
  alter table dbo.ExternalPerformanceData
  alter column Score decimal(10, 4) not null
  go

  alter table dbo.BadgeSetting
  alter column Threshold decimal(10, 4) not null
  go
  alter table dbo.BadgeSetting
  alter column BronzeThreshold decimal(10, 4) not null
  go
  alter table dbo.BadgeSetting
  alter column SilverThreshold decimal(10, 4) not null
  go
  alter table dbo.BadgeSetting
  alter column GoldThreshold decimal(10, 4) not null
  go