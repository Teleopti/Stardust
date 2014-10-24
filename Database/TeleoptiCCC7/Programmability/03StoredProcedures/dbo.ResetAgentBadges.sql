IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetAgentBadges]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ResetAgentBadges]
GO
-- =============================================
-- Author:		Mingdi
-- Create date: 2014-10-24
-- Description:	it will clean the whole table dbo.AgentBadgeTransaction
-- exec dbo.ResetAgentBadges
-- =============================================
CREATE PROCEDURE [dbo].[ResetAgentBadges]
WITH EXECUTE AS OWNER
AS
BEGIN
SET NOCOUNT ON;
TRUNCATE TABLE dbo.AgentBadgeTransaction
END




