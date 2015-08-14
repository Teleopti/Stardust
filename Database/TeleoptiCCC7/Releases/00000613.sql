----------------  
--Name: Chundan Xu
--Desc: 2015-08-13 Remove duplicated setting for the same team 
--                 Added unique index for teamGamificationSetting. 
---------------- 
Delete from [dbo] .[TeamGamificationSetting]
where Id in (
select dupl. id
from    [dbo]. [TeamGamificationSetting]   orig ,
       [dbo] . [TeamGamificationSetting]   dupl
where   orig. Team   =    dupl . Team
and     orig. id     <    dupl . id
)
Go

CREATE UNIQUE NONCLUSTERED INDEX [UQ_Key_Team_TeamGamificationSetting]
ON [dbo].[TeamGamificationSetting] 
(
	[Team] ASC
)
GO