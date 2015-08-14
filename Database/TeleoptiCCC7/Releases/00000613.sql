----------------  
--Name: Chundan Xu
--Desc: 2015-08-13 Added unique index for teamGamificationSetting. 
---------------- 
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Key_Team_TeamGamificationSetting]
ON [dbo].[TeamGamificationSetting] 
(
	[Team] ASC
)
GO