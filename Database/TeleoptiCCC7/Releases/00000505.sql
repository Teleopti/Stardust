----------------  
--Name: Henke Andersson
--Date: 2014-12-22
--Desc: Add new table for site adherence 
---------------- 

CREATE TABLE [ReadModel].[SiteAdherence](
         [SiteId] [uniqueidentifier] NOT NULL,
         [AgentsOutOfAdherence] [int] NOT NULL,
CONSTRAINT [PK_SiteAdherence] PRIMARY KEY CLUSTERED 
(
         [SiteId] ASC   
)
)
GO