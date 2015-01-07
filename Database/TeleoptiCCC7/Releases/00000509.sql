----------------  
--Name: Henke Andersson
--Date: 2014-12-22
--Desc: Add new table for site adherence 
---------------- 
DROP TABLE [ReadModel].[SiteAdherence];
CREATE TABLE [ReadModel].[SiteAdherence](
         [BusinessUnitId] [uniqueidentifier] NOT NULL,
         [SiteId] [uniqueidentifier] NOT NULL,
         [AgentsOutOfAdherence] [int] NOT NULL,
CONSTRAINT [PK_SiteAdherence] PRIMARY KEY CLUSTERED 
(
         [SiteId] ASC   
)
)
GO