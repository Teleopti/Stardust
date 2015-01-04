IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetAgentBadgesWithRank]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ResetAgentBadgesWithRank]
GO

-- =============================================
-- Author: Xinfeng Li
-- Create date: 2014-12-24
-- Description: it will clean the whole table dbo.AgentBadgeWithRankTransaction
-- exec dbo.ResetAgentBadgesWithRank
-- =============================================
CREATE PROCEDURE [dbo].[ResetAgentBadgesWithRank]
WITH EXECUTE AS OWNER
AS
BEGIN
SET NOCOUNT ON;
TRUNCATE TABLE dbo.AgentBadgeWithRankTransaction
END
