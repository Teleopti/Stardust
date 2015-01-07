----------------  
--Name: Henke Andersson
--Date: 2015-01-07
--Desc: Add new table for team adherence 
---------------- 
DROP TABLE [ReadModel].[TeamAdherence];
CREATE TABLE [ReadModel].[TeamAdherence](      
         [SiteId] [uniqueidentifier] NOT NULL,
		 [TeamId] [uniqueidentifier] NOT NULL,
         [AgentsOutOfAdherence] [int] NOT NULL,
CONSTRAINT [PK_TeamAdherence] PRIMARY KEY CLUSTERED 
(
         [TeamId] ASC   
)
)
GO