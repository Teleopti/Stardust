----------------  
--Name: Henke Andersson
--Date: 2014-12-18
--Desc: Add new table for team adherence 
---------------- 

CREATE TABLE [ReadModel].[TeamAdherence](
         [TeamId] [uniqueidentifier] NOT NULL,
         [AgentsOutOfAdherence] [int] NOT NULL,
CONSTRAINT [PK_TeamAdherence] PRIMARY KEY CLUSTERED 
(
         [TeamId] ASC   
)
)
GO