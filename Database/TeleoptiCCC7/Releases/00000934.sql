----------------  
--Name: Drangonfly
--Date: 2018-01-25
--Desc: add one more column to identity whether the badge type is imported from external
----------------  

ALTER TABLE dbo.AgentBadgeTransaction
	ADD [IsExternal] BIT not null DEFAULT 0
GO

ALTER TABLE dbo.AgentBadgeWithRankTransaction
	ADD [IsExternal] BIT not null DEFAULT 0
GO